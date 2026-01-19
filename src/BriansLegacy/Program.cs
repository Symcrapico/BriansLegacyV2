using System.Security.Claims;
using System.Text.Json;
using BriansLegacy.Data;
using BriansLegacy.Infrastructure;
using BriansLegacy.Models;
using BriansLegacy.Services;
using Hangfire;
using Hangfire.SqlServer;
using HealthChecks.Hangfire;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Load allowed emails from configuration
var allowedEmails = builder.Configuration.GetSection("AllowedEmails").Get<string[]>() ?? [];
var adminEmails = builder.Configuration.GetSection("AdminEmails").Get<string[]>() ?? [];

// Database - SQL Server for main application data
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cookie policy for authentication
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => false;
    options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
});

// Google OAuth authentication with email whitelist
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // 30 min session (aligned with FRMv2)
    options.SlidingExpiration = true;
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
    options.CorrelationCookie.SameSite = SameSiteMode.Unspecified;
    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.CorrelationCookie.HttpOnly = true;

    // Email whitelist validation + admin role assignment
    options.Events.OnCreatingTicket = context =>
    {
        var email = context.Principal?.FindFirst(ClaimTypes.Email)?.Value;

        // Validate email against whitelist
        if (string.IsNullOrEmpty(email) || !allowedEmails.Contains(email, StringComparer.OrdinalIgnoreCase))
        {
            context.Fail("Unauthorized email address.");
            return Task.CompletedTask;
        }

        // Add role claim based on admin list
        var identity = context.Principal?.Identity as ClaimsIdentity;
        if (identity != null)
        {
            var role = adminEmails.Contains(email, StringComparer.OrdinalIgnoreCase)
                ? Roles.Admin
                : Roles.Viewer;
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }

        return Task.CompletedTask;
    };
});

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(Roles.Admin));
    options.AddPolicy("ViewerOrAdmin", policy => policy.RequireRole(Roles.Viewer, Roles.Admin));
});

// Hangfire - Background job processing with SQL Server storage
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true,
        SchemaName = "Hangfire"
    }));

// Single queue for simplicity (as per design doc)
builder.Services.AddHangfireServer(options =>
{
    options.Queues = ["default"];
    options.WorkerCount = Environment.ProcessorCount * 2;
});

// File storage service
builder.Services.AddSingleton<FileStorageService>();

// Library file service (scoped - uses DbContext)
builder.Services.AddScoped<LibraryFileService>();

// Health checks - validates all dependencies
builder.Services.AddHealthChecks()
    .AddSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "sqlserver",
        tags: ["db", "sql"])
    .AddNpgSql(
        builder.Configuration.GetConnectionString("VectorConnection")!,
        name: "postgresql",
        tags: ["db", "vector"])
    .AddHangfire(options =>
    {
        options.MinimumAvailableServers = 1;
    }, name: "hangfire", tags: ["jobs"])
    .AddCheck<FileSystemHealthCheck>(
        "filesystem",
        tags: ["storage"]);

builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCookiePolicy();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Login endpoint - initiates Google OAuth flow
app.MapGet("/login", (string? returnUrl, HttpContext context) =>
{
    // Validate returnUrl to prevent open redirect attacks
    var safeReturnUrl = "/";
    if (!string.IsNullOrEmpty(returnUrl) && Uri.IsWellFormedUriString(returnUrl, UriKind.Relative) && returnUrl.StartsWith('/'))
    {
        safeReturnUrl = returnUrl;
    }
    var props = new AuthenticationProperties { RedirectUri = safeReturnUrl };
    return Results.Challenge(props, [GoogleDefaults.AuthenticationScheme]);
});

// Logout endpoint - signs out and redirects to home
app.MapGet("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/");
});

// Secure file serving - download (requires authentication)
app.MapGet("/files/{fileId:guid}", async (
    Guid fileId,
    ApplicationDbContext db,
    FileStorageService fileStorage,
    ILogger<Program> logger) =>
{
    var file = await db.LibraryFiles.FindAsync(fileId);
    if (file == null)
    {
        logger.LogWarning("File not found: {FileId}", fileId);
        return Results.NotFound();
    }

    var stream = fileStorage.OpenRead(file.OriginalPath);
    if (stream == null)
    {
        logger.LogError("File exists in database but not on disk: {FileId}, Path: {Path}", fileId, file.OriginalPath);
        return Results.NotFound();
    }

    var contentType = FileStorageService.GetContentType(file.FileType);
    var fileName = Path.GetFileName(file.OriginalPath);

    return Results.File(stream, contentType, fileName);
})
.RequireAuthorization("ViewerOrAdmin");

// Secure file serving - inline view (for PDF viewer, requires authentication)
app.MapGet("/files/{fileId:guid}/view", async (
    Guid fileId,
    ApplicationDbContext db,
    FileStorageService fileStorage,
    ILogger<Program> logger) =>
{
    var file = await db.LibraryFiles.FindAsync(fileId);
    if (file == null)
    {
        logger.LogWarning("File not found for viewing: {FileId}", fileId);
        return Results.NotFound();
    }

    var stream = fileStorage.OpenRead(file.OriginalPath);
    if (stream == null)
    {
        logger.LogError("File exists in database but not on disk: {FileId}, Path: {Path}", fileId, file.OriginalPath);
        return Results.NotFound();
    }

    var contentType = FileStorageService.GetContentType(file.FileType);

    // Return file inline (no Content-Disposition: attachment)
    return Results.File(stream, contentType, enableRangeProcessing: true);
})
.RequireAuthorization("ViewerOrAdmin");

// Hangfire dashboard - available at /admin/jobs (Admin role only)
app.MapHangfireDashboard("/admin/jobs", new DashboardOptions
{
    DashboardTitle = "Brian's Legacy - Jobs",
    DisplayStorageConnectionString = false,
    Authorization = [new HangfireAuthorizationFilter()]
});

// Health endpoint - validates all dependencies (no auth required for monitoring)
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var result = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.TotalMilliseconds,
                description = e.Value.Description,
                tags = e.Value.Tags
            })
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
    }
});

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
