namespace ElBruno.AI.Evaluation.Security;

/// <summary>
/// Provides validation methods to prevent path traversal vulnerabilities in file operations.
/// </summary>
public static class PathValidator
{
    private static readonly char[] InvalidFileNameChars = ['<', '>', ':', '"', '|', '?', '*', '\\', '/', '\0'];

    /// <summary>
    /// Validates a file path to prevent path traversal attacks and other security issues.
    /// </summary>
    /// <param name="path">The file path to validate.</param>
    /// <param name="paramName">The parameter name for exception messages.</param>
    /// <param name="allowAbsolutePaths">Whether to allow absolute paths. Default is false.</param>
    /// <exception cref="ArgumentException">Thrown when the path contains invalid or unsafe patterns.</exception>
    public static void ValidateFilePath(string path, string paramName, bool allowAbsolutePaths = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path, paramName);

        // Reject path traversal sequences
        if (path.Contains(".."))
            throw new ArgumentException("Path traversal detected. Paths containing '..' are not allowed.", paramName);

        // Reject absolute paths if not allowed
        if (!allowAbsolutePaths && Path.IsPathRooted(path))
            throw new ArgumentException("Absolute paths are not allowed. Use relative paths only.", paramName);

        // Reject paths with null characters
        if (path.Contains('\0'))
            throw new ArgumentException("Path contains null characters.", paramName);

        // Validate path length (Windows MAX_PATH = 260)
        if (path.Length > 260)
            throw new ArgumentException("Path exceeds maximum length (260 characters).", paramName);
    }

    /// <summary>
    /// Validates a file name (not a full path) for cross-platform compatibility.
    /// </summary>
    /// <param name="fileName">The file name to validate.</param>
    /// <param name="paramName">The parameter name for exception messages.</param>
    /// <exception cref="ArgumentException">Thrown when the file name contains invalid characters.</exception>
    public static void ValidateFileName(string fileName, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName, paramName);

        // Check for invalid characters
        if (fileName.IndexOfAny(InvalidFileNameChars) >= 0)
            throw new ArgumentException($"File name contains invalid characters.", paramName);

        // Reject leading/trailing whitespace
        if (fileName.Trim() != fileName)
            throw new ArgumentException("File name cannot start or end with whitespace.", paramName);

        // Validate length
        if (fileName.Length > 255)
            throw new ArgumentException("File name exceeds maximum length (255 characters).", paramName);
    }

    /// <summary>
    /// Validates a database path, with additional checks for SQLite URI patterns.
    /// </summary>
    /// <param name="dbPath">The database path to validate.</param>
    /// <param name="paramName">The parameter name for exception messages.</param>
    /// <exception cref="ArgumentException">Thrown when the path contains unsafe patterns.</exception>
    public static void ValidateDatabasePath(string dbPath, string paramName)
    {
        ValidateFilePath(dbPath, paramName, allowAbsolutePaths: false);

        // Additional SQLite-specific validation to prevent URI parameters
        if (dbPath.Contains(';') || dbPath.Contains('?'))
            throw new ArgumentException("SQLite URI parameters are not allowed.", paramName);
    }
}
