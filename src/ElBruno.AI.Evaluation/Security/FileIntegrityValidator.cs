using System.Text;

namespace ElBruno.AI.Evaluation.Security;

/// <summary>
/// Validates file integrity with size limits and format checks before loading.
/// </summary>
public static class FileIntegrityValidator
{
    // File size limits
    private const long MaxJsonFileSize = 100 * 1024 * 1024; // 100 MB
    private const long MaxCsvFileSize = 100 * 1024 * 1024;  // 100 MB
    private const long MaxDatabaseSize = 1024 * 1024 * 1024; // 1 GB

    /// <summary>Validates a JSON file before loading.</summary>
    /// <param name="path">File path to validate.</param>
    /// <exception cref="InvalidOperationException">Thrown when file fails integrity checks.</exception>
    public static void ValidateJsonFile(string path)
    {
        if (!File.Exists(path))
            return; // Let File.OpenRead throw FileNotFoundException

        ValidateFileSize(path, MaxJsonFileSize, "JSON");
        ValidateJsonFormat(path);
    }

    /// <summary>Validates a CSV file before loading.</summary>
    /// <param name="path">File path to validate.</param>
    /// <exception cref="InvalidOperationException">Thrown when file fails integrity checks.</exception>
    public static void ValidateCsvFile(string path)
    {
        if (!File.Exists(path))
            return; // Let File.ReadAllLinesAsync throw FileNotFoundException

        ValidateFileSize(path, MaxCsvFileSize, "CSV");
        ValidateUtf8Encoding(path);
    }

    /// <summary>Validates a SQLite database file before opening.</summary>
    /// <param name="path">File path to validate.</param>
    /// <exception cref="InvalidOperationException">Thrown when file fails integrity checks.</exception>
    public static void ValidateDatabaseFile(string path)
    {
        // For new database files that don't exist yet, skip validation
        if (!File.Exists(path))
            return;

        ValidateFileSize(path, MaxDatabaseSize, "database");
        ValidateSqliteFormat(path);
    }

    private static void ValidateFileSize(string path, long maxSize, string fileType)
    {
        var fileInfo = new FileInfo(path);
        if (fileInfo.Length > maxSize)
        {
            throw new InvalidOperationException(
                $"{fileType} file exceeds maximum size limit. " +
                $"File size: {fileInfo.Length:N0} bytes, Limit: {maxSize:N0} bytes ({maxSize / (1024 * 1024)} MB)");
        }

        if (fileInfo.Length == 0)
        {
            throw new InvalidOperationException($"{fileType} file is empty: {path}");
        }
    }

    private static void ValidateJsonFormat(string path)
    {
        try
        {
            // Read first few bytes to check for JSON structure
            using var stream = File.OpenRead(path);
            Span<byte> header = stackalloc byte[4];
            int bytesRead = stream.Read(header);

            if (bytesRead == 0)
                throw new InvalidOperationException("File is empty");

            // Check for BOM (UTF-8 or UTF-16)
            if (bytesRead >= 3 && header[0] == 0xEF && header[1] == 0xBB && header[2] == 0xBF)
            {
                // UTF-8 BOM detected, skip it
                stream.Position = 3;
                bytesRead = stream.Read(header);
            }
            else if (bytesRead >= 2 && ((header[0] == 0xFF && header[1] == 0xFE) || (header[0] == 0xFE && header[1] == 0xFF)))
            {
                throw new InvalidOperationException("JSON file appears to be UTF-16 encoded. Only UTF-8 is supported.");
            }

            // Look for JSON start characters (allowing whitespace)
            bool foundJsonStart = false;
            for (int i = 0; i < bytesRead; i++)
            {
                byte b = header[i];
                // Skip whitespace
                if (b == ' ' || b == '\t' || b == '\r' || b == '\n')
                    continue;

                // Check for JSON start characters: { or [
                if (b == '{' || b == '[')
                {
                    foundJsonStart = true;
                    break;
                }

                // If we encounter other characters, it's not valid JSON
                throw new InvalidOperationException(
                    "File does not appear to be valid JSON format. Expected '{' or '[' at start.");
            }

            if (!foundJsonStart)
            {
                throw new InvalidOperationException(
                    "File does not appear to be valid JSON format. Expected '{' or '[' at start.");
            }
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException($"Failed to validate JSON file format: {ex.Message}", ex);
        }
    }

    private static void ValidateUtf8Encoding(string path)
    {
        try
        {
            using var stream = File.OpenRead(path);
            Span<byte> buffer = stackalloc byte[512];
            int bytesRead = stream.Read(buffer);

            if (bytesRead == 0)
                return;

            // Check for UTF-8 validity by attempting to decode
            try
            {
                Encoding.UTF8.GetString(buffer[..bytesRead]);
            }
            catch (DecoderFallbackException)
            {
                throw new InvalidOperationException(
                    "File contains invalid UTF-8 encoding. Please ensure the file is properly encoded.");
            }
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException($"Failed to validate file encoding: {ex.Message}", ex);
        }
    }

    private static void ValidateSqliteFormat(string path)
    {
        try
        {
            // SQLite files start with "SQLite format 3\0" (16 bytes)
            using var stream = File.OpenRead(path);
            Span<byte> header = stackalloc byte[16];
            int bytesRead = stream.Read(header);

            if (bytesRead < 16)
            {
                throw new InvalidOperationException(
                    "File is too small to be a valid SQLite database (less than 16 bytes)");
            }

            // Check for SQLite magic header
            ReadOnlySpan<byte> expectedMagic = "SQLite format 3\0"u8;
            if (!header.SequenceEqual(expectedMagic))
            {
                throw new InvalidOperationException(
                    "File does not appear to be a valid SQLite database. Invalid magic header.");
            }
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException($"Failed to validate SQLite database format: {ex.Message}", ex);
        }
    }
}
