using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileSystem.Library.Common;

namespace FileSystem.Library;

/// <summary>
/// Internal buffer for VirtualVersion.
/// </summary>
internal sealed class VirtualVersionBuffer
{
    private readonly Dictionary<long, byte> _bytes = new();
    private long _length;
    private readonly VirtualVersionBuffer? _parent;
    private VirtualVersionBuffer? _child;
    
    /// <summary>
    /// Ctor for empty buffer.
    /// </summary>
    internal VirtualVersionBuffer()
    {
        _length = 0;
    }

    /// <summary>
    /// Ctor for child buffer.
    /// </summary>
    /// <param name="parent">Parent buffer.</param>
    private VirtualVersionBuffer(VirtualVersionBuffer parent) : this()
    {
        _parent = parent;
    }
    
    /// <summary>
    /// Buffer length.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when set length is negative.</exception>
    public long Length
    {
        get => _length;
        set
        {
            EnsureImmutable();
            
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), Constants.Messages.NotBeNegative);
            
            if (value < _length)
                _bytes.TruncateBytes(value);

            _length = value;
        }
    }
    
    /// <summary>
    /// Read from VirtualVersionBuffer.
    /// </summary>
    /// <param name="buffer">Buffer for write read bytes.</param>
    /// <param name="offset">Buffer offset.</param>
    /// <param name="count">Count bytes for read.</param>
    /// <param name="position">Position into VirtualVersionBuffer.</param>
    /// <returns>Count read bytes.</returns>
    public int Read(byte[] buffer, int offset, int count, long position)
    {
        EnsureBuffer(buffer, offset, count);
        
        if (position >= Length)
            return 0;
        
        var readCount = (int) ((position + count) <= Length ? count : (Length - position));
        
        Array.Clear(buffer, offset, readCount);
        
        var parent = this;

        do
        {
            var bytes = parent.GetWrittenBytes(position, readCount);
            
            foreach (var b in bytes)
            {
                var index = offset + (b.Key - position);
            
                if (buffer[index] != 0)
                    continue;
            
                buffer[index] = b.Value;
            }

            parent = parent._parent;

        } while (parent is not null);
        
        return readCount;
    }

    /// <summary>
    /// Write into VirtualVersionBuffer.
    /// </summary>
    /// <param name="buffer">Buffer for write bytes.</param>
    /// <param name="offset">Buffer offset.</param>
    /// <param name="count">Count bytes for write.</param>
    /// <param name="position">Position into VirtualVersionBuffer.</param>
    /// <exception cref="EndOfStreamException">Thrown when VirtualVersionBuffer is too short.</exception>
    public void Write(byte[] buffer, int offset, int count, long position)
    {
        EnsureImmutable();
        EnsureBuffer(buffer, offset, count);
        
        if (position + count > Length)
            throw new EndOfStreamException(Constants.Messages.BufferTooShort);

        for (var i = offset; i < offset + count; i++)
        {
            _bytes[position++] = buffer[i];
        }
    }
    
    /// <summary>
    /// Create a new child buffer based on the current buffer.
    /// </summary>
    /// <returns>Created child buffer.</returns>
    public VirtualVersionBuffer CreateChild()
    {
        var child = new VirtualVersionBuffer(this);
        _child = child;
        return child;
    }

    private KeyValuePair<long, byte>[] GetWrittenBytes(long position, long count)
    {
        return _bytes.Where(p => p.Key >= position && p.Key < (position + count)).ToArray();
    }
    
    private void EnsureImmutable()
    {
        if (_child is not null)
            throw new InvalidOperationException(Constants.Messages.WriteNotAvailable);
    }
    
    private static void EnsureBuffer(byte[] buffer, int offset, int count)
    {
        if (buffer is null)
            throw new ArgumentNullException(nameof(buffer), Constants.Messages.NotBeNull);

        if (offset < 0)
            throw new ArgumentOutOfRangeException(nameof(offset), Constants.Messages.NotBeNegative);

        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), Constants.Messages.NotBeNegative);

        if (buffer.Length - offset < count)
            throw new ArgumentException(Constants.Messages.BufferTooShort);
    }
}