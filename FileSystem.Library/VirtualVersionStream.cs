using System;
using System.IO;
using FileSystem.Library.Common;

namespace FileSystem.Library;

internal sealed class VirtualVersionStream : Stream
{
    private VirtualVersionBuffer _buffer;
    private long _position = 0;
    private bool _disposed = false;
    
    /// <summary>
    /// Ctor for create from VirtualVersionBuffer.
    /// </summary>
    /// <param name="buffer">VirtualVersionBuffer.</param>
    /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
    internal VirtualVersionStream(VirtualVersionBuffer buffer)
    {
        _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer), Constants.Messages.NotBeNull);
    }
    
    public override void Flush()
    {
        EnsureNotDisposed();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        EnsureNotDisposed();
        
        var readCount = _buffer.Read(buffer, offset, count, Position);
        Position += readCount;
        return readCount;
    }
    
    public override long Seek(long offset, SeekOrigin origin)
    {
        EnsureNotDisposed();

        switch (origin)
        {
            case SeekOrigin.Begin:
                Position = offset;
                break;
            case SeekOrigin.Current:
                Position += offset;
                break;
            case SeekOrigin.End:
                Position = Length + offset;
                break;
        }

        return Position;
    }

    public override void SetLength(long value)
    {
        EnsureNotDisposed();
        
        _buffer.Length = value;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        EnsureNotDisposed();
        
        _buffer.Write(buffer, offset, count, Position);
        Position += count;
    }
    
    public override bool CanRead => !_disposed;
    public override bool CanSeek => !_disposed;
    public override bool CanWrite => !_disposed;
    public override long Length => _buffer?.Length ?? 0;
    
    public override long Position
    {
        get => _position;
        set
        {
            EnsureNotDisposed();

            if (Length == 0)
                throw new EndOfStreamException(Constants.Messages.BufferTooShort);

            if (value < 0)
                throw new EndOfStreamException(Constants.Messages.NotBeNegative);
            
            _position = value;
        }
    }
    
    protected override void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        
        if (disposing)
        {
            _buffer = null!;
        }

        _disposed = true;
    }

    private void EnsureNotDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(VirtualVersionStream));
    }
}