using System.Collections.Generic;

namespace FileSystem.Library.Interfaces;

public interface IVirtualFile : IVirtualFileSystemEntry
{
    /// <summary>
    /// Get current (latest) file version.
    /// </summary>
    IVirtualVersion CurrentVersion { get; }

    /// <summary>
    /// Get collection of all registered file versions.
    /// </summary>
    IReadOnlyCollection<IVirtualVersion> Versions { get; }
}