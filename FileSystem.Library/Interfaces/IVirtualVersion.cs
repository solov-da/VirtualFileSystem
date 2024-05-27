using System.IO;

namespace FileSystem.Library.Interfaces;

public interface IVirtualVersion
{
    /// <summary>
    /// Get a new stream to access version's binary data.
    /// </summary>
    /// <returns>Version stream.</returns>
    Stream GetStream();

    /// <summary>
    /// Current data length.
    /// </summary>
    long Length { get; }

    /// <summary>
    /// Reference to the file this version belongs to.
    /// </summary>
    IVirtualFile Owner { get; }

    /// <summary>
    /// Reference to the parent version.
    /// Null if this version is the first file version (root version).
    /// </summary>
    IVirtualVersion? ParentVersion { get; }

    /// <summary>
    /// Reference to the child version.
    /// Null if child version is not yet created.
    /// </summary>
    IVirtualVersion? ChildVersion { get; }

    /// <summary>
    /// Create a new child version based on the current version.
    /// </summary>
    /// <returns>Created child version.</returns>
    /// <exception cref="System.InvalidOperationException">Thrown when number of versions exceeds the limit.</exception>
    /// <exception cref="System.InvalidOperationException">Thrown when another child version has been already created.</exception>
    IVirtualVersion CreateVersion();
}