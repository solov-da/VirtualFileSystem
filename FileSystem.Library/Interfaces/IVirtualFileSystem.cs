using System;

namespace FileSystem.Library.Interfaces;

public interface IVirtualFileSystem
{
    /// <summary>
    /// Root file system directory.
    /// </summary>
    IVirtualDirectory Root { get; }

    /// <summary>
    /// Get a directory by its absolute path.
    /// </summary>
    /// <param name="path">Absolute directory path.</param>
    /// <returns>Found directory.</returns>
    /// <exception cref="System.IO.DirectoryNotFoundException">Thrown when directory path is not found.</exception>
    IVirtualDirectory GetDirectory(string path);

    /// <summary>
    /// Get a file by its absolute path.
    /// </summary>
    /// <param name="path">Absolute file path.</param>
    /// <returns>Found file.</returns>
    /// <exception cref="System.IO.FileNotFoundException">Thrown when file path is not found.</exception>
    IVirtualFile GetFile(string path);

    /// <summary>
    /// The event is raised when a new directory, file or version is created.
    /// </summary>
    event EventHandler<VirtualFileSystemEventArgs> FileSystemChanged;
}