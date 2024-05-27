using FileSystem.Library.Interfaces;

namespace FileSystem.Library;

/// <summary>
/// Virtual file system factory.
/// </summary>
public static class VirtualFileSystemFactory
{
    /// <summary>
    /// Create a new virtual file system.
    /// </summary>
    /// <param name="options">Virtual file system options.</param>
    /// <returns>Created virtual file system object.</returns>
    public static IVirtualFileSystem Create(VirtualFileSystemOptions? options = null) =>
        new VirtualFileSystem(options);
}