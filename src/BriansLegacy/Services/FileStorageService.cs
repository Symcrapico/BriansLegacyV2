// ============================================================================
// File: Services/FileStorageService.cs
// Purpose: Secure file storage operations with path traversal prevention
// ============================================================================

namespace BriansLegacy.Services;

/// <summary>
/// Configuration options for file storage.
/// </summary>
public class FileStorageOptions
{
    /// <summary>Section name in appsettings.json.</summary>
    public const string SectionName = "FileStorage";

    /// <summary>Base path for file storage (relative to content root or absolute).</summary>
    public string BasePath { get; set; } = "app_data";
}

/// <summary>
/// Service for secure file operations with path traversal prevention.
/// All file access should go through this service.
/// </summary>
public class FileStorageService
{
    private readonly string _basePath;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(IConfiguration configuration, IWebHostEnvironment environment, ILogger<FileStorageService> logger)
    {
        _logger = logger;

        var options = configuration.GetSection(FileStorageOptions.SectionName).Get<FileStorageOptions>()
            ?? new FileStorageOptions();

        // Resolve base path (relative to content root if not absolute)
        if (Path.IsPathRooted(options.BasePath))
        {
            _basePath = Path.GetFullPath(options.BasePath);
        }
        else
        {
            _basePath = Path.GetFullPath(Path.Combine(environment.ContentRootPath, options.BasePath));
        }

        // Ensure the base directory exists
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
            _logger.LogInformation("Created file storage directory: {BasePath}", _basePath);
        }
    }

    /// <summary>
    /// Gets the absolute path to a file, with path traversal prevention.
    /// </summary>
    /// <param name="relativePath">Path relative to the storage base (from LibraryFile.OriginalPath).</param>
    /// <returns>Absolute path if valid and within base directory; null if invalid.</returns>
    public string? GetSecurePath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            _logger.LogWarning("Attempted to access file with empty path");
            return null;
        }

        try
        {
            // Combine and normalize the path
            var fullPath = Path.GetFullPath(Path.Combine(_basePath, relativePath));

            // Security check: ensure the resolved path is within our base directory
            if (!fullPath.StartsWith(_basePath, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Path traversal attempt detected: {RelativePath} resolved to {FullPath}",
                    relativePath, fullPath);
                return null;
            }

            return fullPath;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Invalid path: {RelativePath}", relativePath);
            return null;
        }
    }

    /// <summary>
    /// Checks if a file exists at the given relative path.
    /// </summary>
    public bool FileExists(string relativePath)
    {
        var fullPath = GetSecurePath(relativePath);
        return fullPath != null && File.Exists(fullPath);
    }

    /// <summary>
    /// Opens a file stream for reading.
    /// </summary>
    /// <param name="relativePath">Path relative to storage base.</param>
    /// <returns>FileStream if successful; null if file not found or path invalid.</returns>
    public FileStream? OpenRead(string relativePath)
    {
        var fullPath = GetSecurePath(relativePath);
        if (fullPath == null || !File.Exists(fullPath))
        {
            return null;
        }

        return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    /// <summary>
    /// Gets the content type for a file based on its extension.
    /// </summary>
    public static string GetContentType(string fileType)
    {
        return fileType.ToLowerInvariant() switch
        {
            "pdf" => "application/pdf",
            "jpg" or "jpeg" => "image/jpeg",
            "png" => "image/png",
            "gif" => "image/gif",
            "webp" => "image/webp",
            "tiff" or "tif" => "image/tiff",
            "bmp" => "image/bmp",
            "svg" => "image/svg+xml",
            "doc" => "application/msword",
            "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "xls" => "application/vnd.ms-excel",
            "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "txt" => "text/plain",
            "zip" => "application/zip",
            _ => "application/octet-stream"
        };
    }

    /// <summary>
    /// Gets the base storage path.
    /// </summary>
    public string BasePath => _basePath;
}
