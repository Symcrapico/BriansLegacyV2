// ============================================================================
// File: Infrastructure/FileSystemHealthCheck.cs
// Purpose: Health check for filesystem read/write operations
// ============================================================================

using BriansLegacy.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BriansLegacy.Infrastructure;

/// <summary>
/// Health check that validates filesystem read/write operations in the storage directory.
/// </summary>
public class FileSystemHealthCheck : IHealthCheck
{
    private readonly FileStorageService _fileStorage;
    private readonly ILogger<FileSystemHealthCheck> _logger;

    public FileSystemHealthCheck(FileStorageService fileStorage, ILogger<FileSystemHealthCheck> logger)
    {
        _fileStorage = fileStorage;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var testFileName = $".health-check-{Guid.NewGuid():N}.tmp";
        var testFilePath = Path.Combine(_fileStorage.BasePath, testFileName);

        try
        {
            // Test write
            var testContent = $"Health check at {DateTime.UtcNow:O}";
            await File.WriteAllTextAsync(testFilePath, testContent, cancellationToken);

            // Test read
            var readContent = await File.ReadAllTextAsync(testFilePath, cancellationToken);
            if (readContent != testContent)
            {
                return HealthCheckResult.Unhealthy("Filesystem read/write mismatch");
            }

            // Test delete
            File.Delete(testFilePath);

            return HealthCheckResult.Healthy($"Filesystem OK at {_fileStorage.BasePath}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Filesystem health check failed for path: {Path}", _fileStorage.BasePath);
            return HealthCheckResult.Unhealthy($"Filesystem error: {ex.Message}");
        }
        finally
        {
            // Cleanup in case of partial failure
            try
            {
                if (File.Exists(testFilePath))
                {
                    File.Delete(testFilePath);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
