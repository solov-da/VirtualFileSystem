# C#/.NET Challenge

## Given

- A simple .NET solution containing two projects:
    - **FileSystem.Library** - a class library describing required interfaces and some stub classes prepared for you to implement.
    - **FileSystem.Tests** - ready to run NUnit tests defining expected behavior of the virtual file system implemented in the **FileSystem.Library** project.

## Required

- Your main goal is to implement a virtual file system that will eventually pass all tests defined in the **FileSystem.Tests** project

- The virtual file system is represented using the following public interfaces:
    - IVirtualFileSystem
      - Describes a root file system object containing hierarchy structure of directories, files and their versions.
      - A file system always has the root directory ("/") returned by the **Root** property.
      - All directories, files and versions are virtual and stored solely in memory.
      - **GetDirectory** returns a given directory by its absolute path.
      - **GetFile** returns a given file by its absolute path.
      - **FileSystemChanged** event raises when a new directory, file or version is created.
      - **IVirtualFileSystem** is created using static factory  **VirtualFileSystemFactory.Create**. The factory accepts **VirtualFileSystemOptions** object that allows to set two options: **MaximumEntriesPerDirectory** and **MaximumVersionsPerFile**.
    - IVirtualDirectory
      - Describes a single directory.
      - Every directory may contain 0 or more sub-directories and 0 or more files.
      - **CreateDirectory** creates a new sub-directory.
      - **CreateFile** creates a new file.
      - **GetEntries** returns direct children entries.
      - **Enumerate** returns all children entries recursively.
      - All created file system entries are immutable, it is not possible to delete or rename directories and files.
    - IVirtualFile
      - Describes a single file which is merely a container for versions.
      - Every file has at least 1 version (automatically created along with the file itself).
      - **CurrentVersion** property returns the latest file version.
      - **Versions** property returns all created versions in the order of their creation.
    - IVirtualFileSystemEntry
      - A base interface for both IVirtualDirectory and IVirtualFile, describing a single file system entry.
      - **Name** returns directory or file name that was passed into the **CreateDirectory** or **CreateFile** methods. File system entry names are case sensitive.
      - **Path** returns the absolute path to a directory or file with the slash character ('/') used as the path delimiter. For example: "/dir1/dir2".
    - IVirtualVersion
      - Describes a single file version.
      - **Owner** property keeps reference to the owner file.
      - **GetStream** returns a standard .NET **Stream** object used to access binary data (read and write), associated with the version.
      - Version stream stores only written chunks of data and returns zero bytes for uninitialized chunks (a chunk of data is a byte buffer of 1 or more bytes in length).
      - All versions have independent length, initially set to 0.
      - Writing to the same stream position overwrites any previously written data to this version (parent versions never change).
      - When stream length is truncated, all previously written data farther than the new length should be deleted (become uninitialized again).
      - **CreateVersion** returns a new child version based on the current version. The current version automatically becomes its parent version.
      - As soon as child version is created, the parent version instantly becomes immutable (writing to the parent stream throws *InvalidOperationException*).
      - **ChildVersion** and **ParentVersion** properties keep reference to the child and parent versions respectively.
      - The first version of a file does not have parent version, so **ParentVersion** value is always null.
      - The latest version does not have child version, so **ChildVersion** value is always null.
      - The version history is always linear. For example, if `v3` is created by calling `v3 = v2.CreateVersion()` and `v2` is created by calling `v2 = v1.CreateVersion()`, the version history would look like this:
        ```csharp
        null (no parent version available)
          |_ v1
             |_ v2
                |_ v3
                   |_ null (no child version available)
        ```
      - Every child version inherits parent version data. For example, consider the following code:
        ```csharp
        var file = fs.Root.CreateFile("test");
        // v1 is the first file version
        var v1 = file.CurrentVersion;
        var stream1 = v1.GetStream();
        // write data to stream1 (which represents v1)
        stream1.SetLength(4);
        stream1.Position = 0;
        stream1.WriteByte(0x10);
        stream1.WriteByte(0x11);
        // now v1 has: [10, 11, ??, ??]
        // ?? - depicts uninitialized chunk of data

        // create v2 based on v1 (v1 becomes immutable)
        var v2 = v1.CreateVersion();
        var stream2 = v2.GetStream();
        // write data to stream2 (which represents v2, based on v1)
        stream2.SetLength(4);
        stream2.Position = 1;
        stream2.WriteByte(0x20);
        stream2.WriteByte(0x21);
        // now v2 has: [??, 20, 21, ??]
        // ?? - depicts uninitialized chunk of data

        // read data from v1 into a buffer:
        var buffer = new byte[4];
        stream1.Position = 0;
        stream1.Read(buffer, 0, buffer.Length);
        //     v1:  [10, 11, ??, ??]
        // -------------------------
        // buffer: [10, 11, 00, 00]
        // for ?? chunks the stream returns 00 bytes

        // read data from v2 into a buffer:
        stream2.Position = 0;
        stream2.Read(buffer, 0, buffer.Length);
        //     v1:  [10, 11, ??, ??]
        //     v2:  [??, 20, 21, ??]
        // -------------------------
        // buffer: [10, 20, 21, 00]
        // for ?? chunks the stream returns 00 bytes
        ```
        Basically, the stream's *Read** methods have the following logic:
          - the stream checks if it has its own data for the current position
          - if no data available, it recursively checks if any of the parent version streams have the data for the current position
          - if there is no data for the current position - the stream returns zero bytes (0x00)
- The implementation is not expected to be thread safe, so you don't have to implement additional thread safety logic.
- You are free to edit any code in the **FileSystem.Library** project.
- You must not edit existing NUnit tests, however, you are free to write your own tests.
- The total execution time of all tests must be less than 30 seconds (based on a generic 2+ core 2Ghz+ CPU and 4GB RAM).
- Since the main goal is to pass all **FileSystem.Tests** tests, please concentrate on that first and do code optimization and polishing later.