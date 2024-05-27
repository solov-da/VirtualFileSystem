using System;
using System.IO;
using System.Linq;
using FileSystem.Library.Common;
using FileSystem.Library.Interfaces;

namespace FileSystem.Library;

internal sealed class VirtualFileSystem : IVirtualFileSystem
{
    /// <summary>
    /// Ctor for factory.
    /// </summary>
    /// <param name="options">File system options.</param>
    internal VirtualFileSystem(VirtualFileSystemOptions? options)
    {
        Options = options;
        Root = new VirtualDirectory(this);
    }

    internal VirtualFileSystemOptions? Options { get; }
    
    public IVirtualDirectory Root { get; }

    public IVirtualDirectory GetDirectory(string path)
        => Root.Enumerate()
            .OfType<IVirtualDirectory>()
            .FirstOrDefault(p => p.Path == path) 
           ?? throw new DirectoryNotFoundException(Constants.Messages.DirectoryNotFound);
    
    public IVirtualFile GetFile(string path)
        => Root.Enumerate()
            .OfType<IVirtualFile>()
            .FirstOrDefault(p => p.Path == path) 
           ?? throw new FileNotFoundException(Constants.Messages.FileNotFound);

    public event EventHandler<VirtualFileSystemEventArgs>? FileSystemChanged;
    
    internal void OnVersionCreated(IVirtualVersion version)
    {
        FileSystemChanged?.Invoke(this, new VirtualFileSystemEventArgs(version, VirtualFileSystemEventType.CreatedVersion));
    }
    
    internal void OnFileCreated(IVirtualFile file)
    {
        FileSystemChanged?.Invoke(this, new VirtualFileSystemEventArgs(file, VirtualFileSystemEventType.CreatedFile));
    }
    
    internal void OnDirectoryCreated(IVirtualDirectory directory)
    {
        FileSystemChanged?.Invoke(this, new VirtualFileSystemEventArgs(directory, VirtualFileSystemEventType.CreatedDirectory));
    }
}