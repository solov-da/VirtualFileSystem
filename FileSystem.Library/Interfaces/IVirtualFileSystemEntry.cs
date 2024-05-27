namespace FileSystem.Library.Interfaces;

public interface IVirtualFileSystemEntry
{
    /// <summary>
    /// File system entry name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Absolute file system entry path.
    /// </summary>
    string Path { get; }
}