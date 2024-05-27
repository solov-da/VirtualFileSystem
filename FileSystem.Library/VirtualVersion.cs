using System;
using System.IO;
using FileSystem.Library.Common;
using FileSystem.Library.Interfaces;

namespace FileSystem.Library;

internal sealed class VirtualVersion : IVirtualVersion
{
    private readonly VirtualVersionBuffer _buffer;
    private readonly VirtualFile _owner;
    
    /// <summary>
    /// Ctor for first version.
    /// </summary>
    /// <param name="owner">File.</param>
    /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
    internal VirtualVersion(VirtualFile owner)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner), Constants.Messages.NotBeNull);
        _buffer = new();
    }

    /// <summary>
    /// Ctor for child version.
    /// </summary>
    /// <param name="owner">File.</param>
    /// <param name="parent">Parent version.</param>
    /// <param name="buffer">Child buffer.</param>
    /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
    private VirtualVersion(VirtualFile owner, IVirtualVersion parent, VirtualVersionBuffer buffer)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner), Constants.Messages.NotBeNull);
        _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer), Constants.Messages.NotBeNull);
        ParentVersion = parent ?? throw new ArgumentNullException(nameof(parent), Constants.Messages.NotBeNull);
    }
    
    public Stream GetStream() => new VirtualVersionStream(_buffer);
    public long Length => _buffer.Length;
    public IVirtualFile Owner => _owner;
    public IVirtualVersion? ParentVersion { get; }
    public IVirtualVersion? ChildVersion { get; private set; }
    
    public IVirtualVersion CreateVersion()
    {
        if (_owner.Directory.FileSystem.Options?.MaximumVersionsPerFile == Owner.Versions.Count)
            throw new InvalidOperationException(Constants.Messages.MaxVersionsCreated);
        
        if (ChildVersion is not null)
            throw new InvalidOperationException(Constants.Messages.ChildVersionCreated);
        
        var version = new VirtualVersion(_owner, this, _buffer.CreateChild());
        ChildVersion = version;
        _owner.Directory.FileSystem.OnVersionCreated(version);
        return version;
    }
}