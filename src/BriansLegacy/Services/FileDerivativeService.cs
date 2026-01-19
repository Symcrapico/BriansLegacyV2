// ============================================================================
// File: Services/FileDerivativeService.cs
// Purpose: FileDerivative tracking with idempotent processing support
// ============================================================================

using BriansLegacy.Data;
using BriansLegacy.Models;
using Microsoft.EntityFrameworkCore;

namespace BriansLegacy.Services;

/// <summary>
/// Service for managing file derivatives (thumbnails, page images, extracted text, title block crops).
/// Supports idempotent processing via unique constraint on (LibraryFileId, DerivativeType, GeneratorVersion, InputHash).
/// </summary>
public class FileDerivativeService
{
    private readonly ApplicationDbContext _db;
    private readonly FileStorageService _fileStorage;
    private readonly ILogger<FileDerivativeService> _logger;

    public FileDerivativeService(
        ApplicationDbContext db,
        FileStorageService fileStorage,
        ILogger<FileDerivativeService> logger)
    {
        _db = db;
        _fileStorage = fileStorage;
        _logger = logger;
    }

    /// <summary>
    /// Gets an existing derivative or returns null if not found.
    /// Used to check if processing can be skipped (idempotent).
    /// </summary>
    /// <param name="libraryFileId">Source file ID.</param>
    /// <param name="derivativeType">Type of derivative.</param>
    /// <param name="generatorVersion">Version of the generator.</param>
    /// <param name="inputHash">Hash of the input used to generate the derivative.</param>
    /// <returns>Existing derivative if found; null otherwise.</returns>
    public async Task<FileDerivative?> GetExistingDerivativeAsync(
        Guid libraryFileId,
        DerivativeType derivativeType,
        string generatorVersion,
        string inputHash)
    {
        return await _db.FileDerivatives
            .FirstOrDefaultAsync(d =>
                d.LibraryFileId == libraryFileId &&
                d.DerivativeType == derivativeType &&
                d.GeneratorVersion == generatorVersion &&
                d.InputHash == inputHash);
    }

    /// <summary>
    /// Gets or creates a derivative. If a matching derivative already exists, returns it.
    /// Otherwise, stores the new derivative file and creates a database record.
    /// This is the main method for idempotent derivative creation.
    /// </summary>
    /// <param name="libraryFileId">Source file ID.</param>
    /// <param name="derivativeType">Type of derivative.</param>
    /// <param name="generatorVersion">Version of the generator (e.g., "tesseract-5.0", "gemini-1.5").</param>
    /// <param name="inputHash">Hash of the input used (enables safe reprocessing).</param>
    /// <param name="contentStream">Stream containing the derivative content.</param>
    /// <param name="fileExtension">File extension for the derivative (e.g., "png", "txt").</param>
    /// <returns>Result indicating whether derivative was created or already existed.</returns>
    public async Task<DerivativeResult> GetOrCreateDerivativeAsync(
        Guid libraryFileId,
        DerivativeType derivativeType,
        string generatorVersion,
        string inputHash,
        Stream contentStream,
        string fileExtension)
    {
        // Check for existing derivative
        var existing = await GetExistingDerivativeAsync(libraryFileId, derivativeType, generatorVersion, inputHash);
        if (existing != null)
        {
            _logger.LogDebug(
                "Derivative already exists. FileId: {FileId}, Type: {Type}, Version: {Version}",
                libraryFileId, derivativeType, generatorVersion);

            return new DerivativeResult
            {
                Success = true,
                AlreadyExisted = true,
                Derivative = existing
            };
        }

        // Store the derivative file
        var fileName = $"{libraryFileId}_{derivativeType}_{generatorVersion}.{fileExtension}";
        var storeResult = await _fileStorage.StoreFileAsync(contentStream, fileName, "derivatives");

        // Create database record
        var derivative = new FileDerivative
        {
            Id = Guid.NewGuid(),
            LibraryFileId = libraryFileId,
            DerivativeType = derivativeType,
            Path = storeResult.RelativePath,
            GeneratedAt = DateTime.UtcNow,
            GeneratorVersion = generatorVersion,
            InputHash = inputHash
        };

        try
        {
            _db.FileDerivatives.Add(derivative);
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Created derivative. Id: {DerivativeId}, FileId: {FileId}, Type: {Type}, Version: {Version}",
                derivative.Id, libraryFileId, derivativeType, generatorVersion);

            return new DerivativeResult
            {
                Success = true,
                AlreadyExisted = false,
                Derivative = derivative
            };
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            // Race condition: another process created the same derivative
            // Delete the file we just stored and return the existing one
            _fileStorage.DeleteFile(storeResult.RelativePath);

            var existingAfterRace = await GetExistingDerivativeAsync(libraryFileId, derivativeType, generatorVersion, inputHash);
            if (existingAfterRace != null)
            {
                _logger.LogDebug(
                    "Derivative created by concurrent process. FileId: {FileId}, Type: {Type}",
                    libraryFileId, derivativeType);

                return new DerivativeResult
                {
                    Success = true,
                    AlreadyExisted = true,
                    Derivative = existingAfterRace
                };
            }

            // Unexpected: constraint violation but no existing record found
            throw;
        }
    }

    /// <summary>
    /// Creates a derivative record directly (when file is already stored).
    /// Use this when the derivative file was created by an external process.
    /// </summary>
    public async Task<DerivativeResult> CreateDerivativeRecordAsync(
        Guid libraryFileId,
        DerivativeType derivativeType,
        string generatorVersion,
        string inputHash,
        string relativePath)
    {
        // Check for existing derivative
        var existing = await GetExistingDerivativeAsync(libraryFileId, derivativeType, generatorVersion, inputHash);
        if (existing != null)
        {
            return new DerivativeResult
            {
                Success = true,
                AlreadyExisted = true,
                Derivative = existing
            };
        }

        var derivative = new FileDerivative
        {
            Id = Guid.NewGuid(),
            LibraryFileId = libraryFileId,
            DerivativeType = derivativeType,
            Path = relativePath,
            GeneratedAt = DateTime.UtcNow,
            GeneratorVersion = generatorVersion,
            InputHash = inputHash
        };

        try
        {
            _db.FileDerivatives.Add(derivative);
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Created derivative record. Id: {DerivativeId}, FileId: {FileId}, Type: {Type}",
                derivative.Id, libraryFileId, derivativeType);

            return new DerivativeResult
            {
                Success = true,
                AlreadyExisted = false,
                Derivative = derivative
            };
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            var existingAfterRace = await GetExistingDerivativeAsync(libraryFileId, derivativeType, generatorVersion, inputHash);
            if (existingAfterRace != null)
            {
                return new DerivativeResult
                {
                    Success = true,
                    AlreadyExisted = true,
                    Derivative = existingAfterRace
                };
            }

            throw;
        }
    }

    /// <summary>
    /// Gets all derivatives for a library file.
    /// </summary>
    public async Task<List<FileDerivative>> GetDerivativesForFileAsync(Guid libraryFileId)
    {
        return await _db.FileDerivatives
            .Where(d => d.LibraryFileId == libraryFileId)
            .OrderBy(d => d.DerivativeType)
            .ThenByDescending(d => d.GeneratedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all derivatives of a specific type for a library file.
    /// </summary>
    public async Task<List<FileDerivative>> GetDerivativesByTypeAsync(
        Guid libraryFileId,
        DerivativeType derivativeType)
    {
        return await _db.FileDerivatives
            .Where(d => d.LibraryFileId == libraryFileId && d.DerivativeType == derivativeType)
            .OrderByDescending(d => d.GeneratedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets the most recent derivative of a specific type for a library file.
    /// </summary>
    public async Task<FileDerivative?> GetLatestDerivativeAsync(
        Guid libraryFileId,
        DerivativeType derivativeType)
    {
        return await _db.FileDerivatives
            .Where(d => d.LibraryFileId == libraryFileId && d.DerivativeType == derivativeType)
            .OrderByDescending(d => d.GeneratedAt)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Gets a derivative by ID.
    /// </summary>
    public async Task<FileDerivative?> GetByIdAsync(Guid derivativeId)
    {
        return await _db.FileDerivatives
            .Include(d => d.LibraryFile)
            .FirstOrDefaultAsync(d => d.Id == derivativeId);
    }

    /// <summary>
    /// Deletes a derivative (both database record and physical file).
    /// </summary>
    public async Task<bool> DeleteDerivativeAsync(Guid derivativeId)
    {
        var derivative = await _db.FileDerivatives.FindAsync(derivativeId);
        if (derivative == null)
        {
            return false;
        }

        // Delete physical file
        _fileStorage.DeleteFile(derivative.Path);

        // Delete database record
        _db.FileDerivatives.Remove(derivative);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Deleted derivative: {DerivativeId}", derivativeId);
        return true;
    }

    /// <summary>
    /// Deletes all derivatives for a library file.
    /// Called when cleaning up a file or reprocessing from scratch.
    /// </summary>
    public async Task<int> DeleteAllDerivativesForFileAsync(Guid libraryFileId)
    {
        var derivatives = await _db.FileDerivatives
            .Where(d => d.LibraryFileId == libraryFileId)
            .ToListAsync();

        foreach (var derivative in derivatives)
        {
            _fileStorage.DeleteFile(derivative.Path);
        }

        _db.FileDerivatives.RemoveRange(derivatives);
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "Deleted {Count} derivatives for file: {FileId}",
            derivatives.Count, libraryFileId);

        return derivatives.Count;
    }

    /// <summary>
    /// Deletes all derivatives of a specific type for a library file.
    /// Used when reprocessing a specific derivative type.
    /// </summary>
    public async Task<int> DeleteDerivativesByTypeAsync(
        Guid libraryFileId,
        DerivativeType derivativeType)
    {
        var derivatives = await _db.FileDerivatives
            .Where(d => d.LibraryFileId == libraryFileId && d.DerivativeType == derivativeType)
            .ToListAsync();

        foreach (var derivative in derivatives)
        {
            _fileStorage.DeleteFile(derivative.Path);
        }

        _db.FileDerivatives.RemoveRange(derivatives);
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "Deleted {Count} derivatives of type {Type} for file: {FileId}",
            derivatives.Count, derivativeType, libraryFileId);

        return derivatives.Count;
    }

    /// <summary>
    /// Opens a stream to read a derivative file.
    /// </summary>
    public FileStream? OpenDerivativeStream(FileDerivative derivative)
    {
        return _fileStorage.OpenRead(derivative.Path);
    }

    /// <summary>
    /// Checks if a derivative file exists on disk.
    /// </summary>
    public bool DerivativeFileExists(FileDerivative derivative)
    {
        return _fileStorage.FileExists(derivative.Path);
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        // SQL Server unique constraint violation
        return ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true
            || ex.InnerException?.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase) == true
            || ex.InnerException?.Message.Contains("2627") == true   // SQL Server unique constraint
            || ex.InnerException?.Message.Contains("2601") == true;  // SQL Server unique index
    }
}

/// <summary>
/// Result of a derivative creation operation.
/// </summary>
public class DerivativeResult
{
    /// <summary>Whether the operation succeeded.</summary>
    public bool Success { get; init; }

    /// <summary>Whether the derivative already existed (idempotent hit).</summary>
    public bool AlreadyExisted { get; init; }

    /// <summary>The derivative (either existing or newly created).</summary>
    public FileDerivative? Derivative { get; init; }
}
