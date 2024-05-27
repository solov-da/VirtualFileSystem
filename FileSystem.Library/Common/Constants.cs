namespace FileSystem.Library.Common;

public static class Constants
{
    public static class Path
    {
        public const string Delimiter = "/";
    }
    
    public static class Messages
    {
        public const string NotBeNegative = "Argument must not be negative.";
        public const string NotBeNull = "Argument must not be null.";
        public const string BufferTooShort = "Buffer length is too short.";
        public const string WriteNotAvailable = "Write operation not available.";
        public const string ChildVersionCreated = "Child version already created.";
        public const string MaxVersionsCreated = "Maximum versions already created.";
        public const string MaxEntriesCreated = "Maximum entries already created.";
        public const string FileNotFound = "File not found.";
        public const string DirectoryNotFound = "Directory not found.";
        public const string EntryAlreadyExists = "Entry already exists.";
        public const string InvalidEntryName = "Invalid entry name.";
    }
}