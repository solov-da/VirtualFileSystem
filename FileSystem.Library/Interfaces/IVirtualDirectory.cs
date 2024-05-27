using System.Collections.Generic;

namespace FileSystem.Library.Interfaces;

public interface IVirtualDirectory : IVirtualFileSystemEntry
{
    /// <summary>
    /// Create a new file.
    /// </summary>
    /// <param name="name">File name.</param>
    /// <returns>Created file.</returns>
    /// <exception cref="System.InvalidOperationException">Thrown when number of entries exceeds the limit.</exception>
    /// <exception cref="System.ArgumentException">Thrown when file with the same name already exists.</exception>
    /// <exception cref="System.ArgumentException">Thrown when file name contains '/' character.</exception>
    IVirtualFile CreateFile(string name);

    /// <summary>
    /// Create a new directory.
    /// </summary>
    /// <param name="name">Directory name.</param>
    /// <returns>Created directory.</returns>
    /// <exception cref="System.InvalidOperationException">Thrown when number of entries exceeds the limit.</exception>
    /// <exception cref="System.ArgumentException">Thrown when directory with the same name already exists.</exception>
    /// <exception cref="System.ArgumentException">Thrown when directory name contains '/' character.</exception>
    IVirtualDirectory CreateDirectory(string name);

    /// <summary>
    /// Get a collection of all files and directories inside a given directory.
    /// </summary>
    /// <returns>Collection of files and directories.</returns>
    IReadOnlyCollection<IVirtualFileSystemEntry> GetEntries();

    /// <summary>
    /// Recursively enumerate all files and directories inside a given directory including all sub-directories.
    /// </summary>
    /// <returns>Enumerated files and directories.</returns>
    IEnumerable<IVirtualFileSystemEntry> Enumerate();
}