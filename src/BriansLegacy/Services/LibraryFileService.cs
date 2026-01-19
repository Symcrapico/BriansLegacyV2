// ============================================================================
// File: Services/LibraryFileService.cs
// Purpose: Library file management with duplicate detection
// ============================================================================

using BriansLegacy.Data;
using BriansLegacy.Models;
using Microsoft.EntityFrameworkCore;

namespace BriansLegacy.Services;

/// <summary>
/// Service for managing library files with hash-based duplicate detection.
/// </summary>
public class LibraryFileService
{
    private readonly ApplicationDbContext _db;
    private readonly FileStorageService _fileStorage;
    private readonly ILogger<LibraryFileService> _logger;

    public LibraryFileService(
        ApplicationDbContext db,
        FileStorageService fileStorage,
        ILogger<LibraryFileService> logger)
    {
        _db = db;
        _fileStorage = fileStorage;
        _logger = logger;
    }

    /// <summary>
    /// Uploads a file for a library item, checking for duplicates.
    /// </summary>
    /// <param name="stream">File stream to upload.</param>
    /// <param name="originalFileName">Original filename.</param>
    /// <param name="libraryItemId">The library item this file belongs to.</param>
    /// <returns>Upload result with file info or duplicate detection.</returns>
    public async Task<FileUploadResult> UploadFileAsync(
        Stream stream,
        string originalFileName,
        Guid libraryItemId)
    {
        // Store file and get hash
        var storeResult = await _fileStorage.StoreFileAsync(stream, originalFileName);

        // Check for duplicate by hash
        var existingFile = await _db.LibraryFiles
            .Include(f => f.LibraryItem)
            .FirstOrDefaultAsync(f => f.ContentHash == storeResult.ContentHash);

        if (existingFile != null)
        {
            // Delete the just-stored file since it's a duplicate
            _fileStorage.DeleteFile(storeResult.RelativePath);

            _logger.LogWarning(
                "Duplicate file detected. Hash: {Hash}, Existing file: {ExistingId} for item: {ExistingItemId}",
                storeResult.ContentHash, existingFile.Id, existingFile.LibraryItemId);

            return new FileUploadResult
            {
                Success = false,
                IsDuplicate = true,
                DuplicateFileId = existingFile.Id,
                DuplicateLibraryItemId = existingFile.LibraryItemId,
                DuplicateLibraryItemTitle = existingFile.LibraryItem?.Title,
                ContentHash = storeResult.ContentHash
            };
        }

        // Create database record
        var libraryFile = new LibraryFile
        {
            Id = Guid.NewGuid(),
            LibraryItemId = libraryItemId,
            OriginalPath = storeResult.RelativePath,
            FileType = storeResult.FileType,
            SizeBytes = storeResult.SizeBytes,
            ContentHash = storeResult.ContentHash,
            UploadedAt = DateTime.UtcNow
        };

        _db.LibraryFiles.Add(libraryFile);
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "File uploaded successfully. Id: {FileId}, Item: {ItemId}, Hash: {Hash}",
            libraryFile.Id, libraryItemId, storeResult.ContentHash);

        return new FileUploadResult
        {
            Success = true,
            IsDuplicate = false,
            FileId = libraryFile.Id,
            RelativePath = storeResult.RelativePath,
            ContentHash = storeResult.ContentHash,
            SizeBytes = storeResult.SizeBytes,
            FileType = storeResult.FileType
        };
    }

    /// <summary>
    /// Checks if a file with the given hash already exists.
    /// Call this before uploading to provide early duplicate warning.
    /// </summary>
    /// <param name="stream">Stream to check (will be reset to beginning after).</param>
    /// <returns>Existing file info if duplicate found; null otherwise.</returns>
    public async Task<DuplicateCheckResult?> CheckForDuplicateAsync(Stream stream)
    {
        var hash = await FileStorageService.CalculateHashAsync(stream);

        // Reset stream for subsequent upload
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        var existingFile = await _db.LibraryFiles
            .Include(f => f.LibraryItem)
            .FirstOrDefaultAsync(f => f.ContentHash == hash);

        if (existingFile == null)
        {
            return null;
        }

        return new DuplicateCheckResult
        {
            ContentHash = hash,
            ExistingFileId = existingFile.Id,
            ExistingLibraryItemId = existingFile.LibraryItemId,
            ExistingLibraryItemTitle = existingFile.LibraryItem?.Title
        };
    }

    /// <summary>
    /// Gets a library file by ID.
    /// </summary>
    public async Task<LibraryFile?> GetByIdAsync(Guid fileId)
    {
        return await _db.LibraryFiles
            .Include(f => f.LibraryItem)
            .FirstOrDefaultAsync(f => f.Id == fileId);
    }

    /// <summary>
    /// Gets all files for a library item.
    /// </summary>
    public async Task<List<LibraryFile>> GetFilesForItemAsync(Guid libraryItemId)
    {
        return await _db.LibraryFiles
            .Where(f => f.LibraryItemId == libraryItemId)
            .OrderBy(f => f.UploadedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Deletes a library file (both database record and physical file).
    /// </summary>
    public async Task<bool> DeleteFileAsync(Guid fileId)
    {
        var file = await _db.LibraryFiles.FindAsync(fileId);
        if (file == null)
        {
            return false;
        }

        // Delete physical file
        _fileStorage.DeleteFile(file.OriginalPath);

        // Delete database record
        _db.LibraryFiles.Remove(file);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Deleted file: {FileId}", fileId);
        return true;
    }
}

/// <summary>
/// Result of a file upload operation.
/// </summary>
public class FileUploadResult
{
    public bool Success { get; init; }
    public bool IsDuplicate { get; init; }

    // On success
    public Guid? FileId { get; init; }
    public string? RelativePath { get; init; }
    public string? ContentHash { get; init; }
    public long? SizeBytes { get; init; }
    public string? FileType { get; init; }

    // On duplicate
    public Guid? DuplicateFileId { get; init; }
    public Guid? DuplicateLibraryItemId { get; init; }
    public string? DuplicateLibraryItemTitle { get; init; }
}

/// <summary>
/// Result of a duplicate check.
/// </summary>
public class DuplicateCheckResult
{
    public required string ContentHash { get; init; }
    public Guid ExistingFileId { get; init; }
    public Guid ExistingLibraryItemId { get; init; }
    public string? ExistingLibraryItemTitle { get; init; }
}
