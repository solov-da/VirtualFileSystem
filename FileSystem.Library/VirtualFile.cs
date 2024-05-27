using System;
using System.Collections.Generic;
using FileSystem.Library.Common;
using FileSystem.Library.Interfaces;

namespace FileSystem.Library;

internal sealed class VirtualFile : IVirtualFile
{
    private readonly List<IVirtualVersion> _versions = new();
    
    /// <summary>
    /// Ctor for create from directory.
    /// </summary>
    /// <param name="name">File name.</param>
    /// <param name="directory">Parent directory.</param>
    /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
    internal VirtualFile(string name, VirtualDirectory directory)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name), Constants.Messages.NotBeNull);
        Directory = directory ?? throw new ArgumentNullException(nameof(directory), Constants.Messages.NotBeNull);
        Path = directory.Path.CombineEntry(Name);
        Directory.FileSystem.FileSystemChanged += OnVersionCreatedHandler;
        CurrentVersion = new VirtualVersion(this);
        _versions.Add(CurrentVersion);
    }
    
    public string Name { get; }
    public string Path { get; }
    public IVirtualVersion CurrentVersion { get; private set; }
    public IReadOnlyCollection<IVirtualVersion> Versions => _versions;
    
    internal VirtualDirectory Directory { get; }

    private void OnVersionCreatedHandler(object? sender, VirtualFileSystemEventArgs e)
    {
        if (e.EventType != VirtualFileSystemEventType.CreatedVersion) 
            return;
        
        var version = (VirtualVersion)e.Entry;
        
        if (version == CurrentVersion)
            return;
        
        CurrentVersion = version;
        _versions.Add(version);
    }
}