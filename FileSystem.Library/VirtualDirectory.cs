using System;
using System.Collections.Generic;
using System.Linq;
using FileSystem.Library.Common;
using FileSystem.Library.Interfaces;

namespace FileSystem.Library;

internal sealed class VirtualDirectory : IVirtualDirectory
{
    private readonly Dictionary<string, IVirtualFileSystemEntry> _entries = new();

    /// <summary>
    /// Ctor for root directory.
    /// </summary>
    /// <param name="fileSystem">File system.</param>
    /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
    internal VirtualDirectory(VirtualFileSystem fileSystem)
    {
        FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem), Constants.Messages.NotBeNull);
        Name = Constants.Path.Delimiter;
        Path = Constants.Path.Delimiter;
    }
    
    /// <summary>
    /// Ctor for child directory.
    /// </summary>
    /// <param name="name">Name directory.</param>
    /// <param name="directory">Parent directory.</param>
    /// <exception cref="ArgumentNullException">Thrown when any argument is null</exception>
    private VirtualDirectory(string name, VirtualDirectory directory)
    {
        if (directory is null)
            throw new ArgumentNullException(nameof(directory), Constants.Messages.NotBeNull);
        
        Name = name ?? throw new ArgumentNullException(nameof(name), Constants.Messages.NotBeNull);
        Path = directory.Path.CombineEntry(Name);
        FileSystem = directory.FileSystem;
    }
    
    public string Name { get; }
    public string Path { get; }
    internal VirtualFileSystem FileSystem { get; }
    
    public IVirtualFile CreateFile(string name)
    {
        EnsureMaxEntries();
        EnsureName(name);
        EnsureAlreadyExists(name);
        
        var file = new VirtualFile(name, this);
        _entries.Add(name, file);
        FileSystem.OnFileCreated(file);
        FileSystem.OnVersionCreated(file.CurrentVersion);
        return file;
    }

    public IVirtualDirectory CreateDirectory(string name)
    {
        EnsureMaxEntries();
        EnsureName(name);
        EnsureAlreadyExists(name);
        
        var dir = new VirtualDirectory(name, this);
        _entries.Add(name, dir);
        FileSystem.OnDirectoryCreated(dir);
        return dir;
    }

    public IReadOnlyCollection<IVirtualFileSystemEntry> GetEntries() => _entries.Values.ToArray();

    public IEnumerable<IVirtualFileSystemEntry> Enumerate()
    {
        IVirtualDirectory directory = this;
        var queue = new Queue<IVirtualDirectory>();
        queue.Enqueue(directory);
        
        do
        {
            foreach (var entry in queue.Dequeue().GetEntries())
            {
                yield return entry;
                
                if (entry is IVirtualDirectory innerDirectory)
                {
                    queue.Enqueue(innerDirectory);
                }
            }
            
        } while (queue.Any());
    }
    
    private void EnsureMaxEntries()
    { 
        if (FileSystem.Options?.MaximumEntriesPerDirectory == _entries.Count) 
            throw new InvalidOperationException(Constants.Messages.MaxEntriesCreated);
    }

    private void EnsureAlreadyExists(string name)
    {
        if (_entries.ContainsKey(name))
            throw new ArgumentException(Constants.Messages.EntryAlreadyExists);
    }
    
    private static void EnsureName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Contains(Constants.Path.Delimiter))
            throw new ArgumentException(Constants.Messages.InvalidEntryName);
    }
}