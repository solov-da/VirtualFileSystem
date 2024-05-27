namespace FileSystem.Library;

/// <summary>
/// Virtual file system options.
/// </summary>
public class VirtualFileSystemOptions
{
    /// <summary>
    /// Defines the maximum allowed number of entries per single directory.
    /// </summary>
    public int MaximumEntriesPerDirectory { get; set; }
    /// <summary>
    /// Defines the maximum allowed number of versions per single file.
    /// </summary>
    public int MaximumVersionsPerFile { get; set; }

    /// <summary>
    /// Default options.
    /// </summary>
    public static VirtualFileSystemOptions Default => new VirtualFileSystemOptions
    {
        MaximumEntriesPerDirectory = 1000,
        MaximumVersionsPerFile = 10
    };
}