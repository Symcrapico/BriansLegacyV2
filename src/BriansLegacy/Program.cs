using BriansLegacy.Data;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Database - SQL Server for main application data
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
    options.Queues = new[] { "default" };
    options.WorkerCount = Environment.ProcessorCount * 2;
});

builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

// Hangfire dashboard - available at /admin/jobs
// TODO: Add authorization filter once Identity is set up (TASK-001-C-2)
app.MapHangfireDashboard("/admin/jobs", new DashboardOptions
{
    DashboardTitle = "Brian's Legacy - Jobs",
    DisplayStorageConnectionString = false
});

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
