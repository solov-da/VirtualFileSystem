using System;

namespace FileSystem.Library;

/// <summary>
/// Virtual file system event arguments.
/// </summary>
public class VirtualFileSystemEventArgs : EventArgs
{
    public object Entry { get; }
    public VirtualFileSystemEventType EventType { get; }

    public VirtualFileSystemEventArgs(object entry, VirtualFileSystemEventType eventType)
    {
        Entry = entry;
        EventType = eventType;
    }
}