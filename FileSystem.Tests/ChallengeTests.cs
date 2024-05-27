using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileSystem.Library;
using FileSystem.Library.Interfaces;
using NUnit.Framework;

namespace FileSystem.Tests
{
    [TestFixture]
    public class RequiredTests
    {
        static IVirtualFileSystem NewFileSystem => VirtualFileSystemFactory.Create();

        static Stream NewVersionStream(IVirtualFileSystem fs) => fs.Root.CreateFile("test").CurrentVersion.GetStream();

        const long Gigabyte = 1L * 1024 * 1024 * 1024;
        const long Terabyte = Gigabyte * 1024;

        static readonly byte[] TestBuffer = Encoding.ASCII.GetBytes("0123456789");

        [Test]
        public void TestFactoryCreatesNewVirtualFileSystem()
        {
            var fs = NewFileSystem;
            Assert.That(fs, Is.Not.Null);
        }

        [Test]
        public void TestFileSystemReturnsRootDirectory()
        {
            var fs = NewFileSystem;
            var root = fs.Root;
            Assert.That(root, Is.Not.Null);
            Assert.That(root, Is.SameAs(fs.Root));
        }

        [Test]
        public void TestFileSystemCreatesDirectoriesAndFiles()
        {
            var fs = NewFileSystem;
            var root = fs.Root;

            var d1 = root.CreateDirectory("d1");
            var d2 = root.CreateDirectory("d2");

            Assert.That(d1, Is.Not.Null);
            Assert.That(d2, Is.Not.Null);

            Assert.That(d1.Name, Is.EqualTo("d1"));
            Assert.That(d1.Path, Is.EqualTo("/d1"));
            Assert.That(d2.Name, Is.EqualTo("d2"));
            Assert.That(d2.Path, Is.EqualTo("/d2"));

            var f1 = root.CreateFile("f1");
            var f2 = root.CreateFile("f2");

            Assert.That(f1, Is.Not.Null);
            Assert.That(f2, Is.Not.Null);

            Assert.That(f1.Name, Is.EqualTo("f1"));
            Assert.That(f1.Path, Is.EqualTo("/f1"));
            Assert.That(f2.Name, Is.EqualTo("f2"));
            Assert.That(f2.Path, Is.EqualTo("/f2"));

            var entries = root.GetEntries();
            Assert.That(entries, Is.EquivalentTo(new IVirtualFileSystemEntry[] { d1, d2, f1, f2 }));
            Assert.That(entries.Select(e => e.Path),
                Is.EquivalentTo(new[] { "/d1", "/d2", "/f1", "/f2" }));
        }

        [Test]
        public void TestFileSystemDirectoryAndFileHierarchy()
        {
            var fs = NewFileSystem;
            var root = fs.Root;

            var d1 = root.CreateDirectory("d1");
            var d2 = d1.CreateDirectory("d2");
            var d3 = d2.CreateDirectory("d3");

            Assert.That(d1, Is.Not.Null);
            Assert.That(d2, Is.Not.Null);
            Assert.That(d3, Is.Not.Null);

            Assert.That(d1.Name, Is.EqualTo("d1"));
            Assert.That(d2.Name, Is.EqualTo("d2"));
            Assert.That(d3.Name, Is.EqualTo("d3"));

            Assert.That(d1.Path, Is.EqualTo("/d1"));
            Assert.That(d2.Path, Is.EqualTo("/d1/d2"));
            Assert.That(d3.Path, Is.EqualTo("/d1/d2/d3"));

            var f1 = d3.CreateFile("f1");
            var f2 = d2.CreateFile("f2");

            Assert.That(f1, Is.Not.Null);
            Assert.That(f2, Is.Not.Null);

            Assert.That(f1.Path, Is.EqualTo("/d1/d2/d3/f1"));
            Assert.That(f2.Path, Is.EqualTo("/d1/d2/f2"));

            Assert.That(root.GetEntries().Select(e => e.Path),
                Is.EquivalentTo(new[] { "/d1" }));
            Assert.That(d1.GetEntries().Select(e => e.Path),
                Is.EquivalentTo(new[] { "/d1/d2" }));
            Assert.That(d2.GetEntries().Select(e => e.Path),
                Is.EquivalentTo(new[] { "/d1/d2/d3", "/d1/d2/f2" }));
            Assert.That(d3.GetEntries().Select(e => e.Path),
                Is.EquivalentTo(new[] { "/d1/d2/d3/f1" }));

            var allEntries = fs.Root.Enumerate().ToArray();
            Assert.That(allEntries, Is.EquivalentTo(
                new IVirtualFileSystemEntry[] { d1, d2, d3, f2, f1 }));
            Assert.That(allEntries.Select(e => e.Path), Is.EquivalentTo(
                new[] { "/d1", "/d1/d2", "/d1/d2/d3", "/d1/d2/f2", "/d1/d2/d3/f1" }));
        }

        [Test]
        public void TestFileSystemReturnsDirectoryByPath()
        {
            var fs = NewFileSystem;
            var root = fs.Root;

            var d1 = root.CreateDirectory("d1");
            var d2 = d1.CreateDirectory("d2");
            var d3 = d2.CreateDirectory("d3");

            Assert.That(fs.GetDirectory(d1.Path), Is.SameAs(d1));
            Assert.That(fs.GetDirectory(d2.Path), Is.SameAs(d2));
            Assert.That(fs.GetDirectory(d3.Path), Is.SameAs(d3));
        }

        [Test]
        public void TestFileSystemReturnsFileByPath()
        {
            var fs = NewFileSystem;
            var root = fs.Root;

            var d1 = root.CreateDirectory("d1");
            var f1 = d1.CreateFile("f1");
            var f2 = d1.CreateFile("f2");

            Assert.That(fs.GetFile(f1.Path), Is.SameAs(f1));
            Assert.That(fs.GetFile(f2.Path), Is.SameAs(f2));
        }

        [Test]
        public void TestFileSystemSupportsVeryDeepDirectoryHierarchy()
        {
            var fs = NewFileSystem;
            var root = fs.Root;

            const int count = 10000;

            var directory = root;
            for (int i = 0; i < count; i++)
            {
                directory = directory.CreateDirectory(i.ToString());
            }

            Assert.That(root.Enumerate().ToArray(), Has.Length.EqualTo(count));
        }

        [Test]
        public void TestDirectoryEnumeratesEntries()
        {
            var fs = NewFileSystem;
            var root = fs.Root;

            Assert.That(root.Enumerate(), Is.EquivalentTo(new IVirtualFileSystemEntry[0]));

            var d1 = root.CreateDirectory("d1");
            var d2 = d1.CreateDirectory("d2");
            var f1 = d1.CreateFile("f1");
            var f2 = d2.CreateFile("f2");

            Assert.That(d1.Enumerate(),
                Is.EquivalentTo(new IVirtualFileSystemEntry[] { d2, f1, f2 }));
            Assert.That(d1.Enumerate().Select(e => e.Path),
                Is.EquivalentTo(new[] { "/d1/d2", "/d1/f1", "/d1/d2/f2" }));
        }

        [Test]
        public void TestDirectoryReturnsEntries()
        {
            var fs = NewFileSystem;
            var root = fs.Root;

            var d1 = root.CreateDirectory("d1");
            var d2 = d1.CreateDirectory("d2");
            var f1 = d1.CreateFile("f1");
            d2.CreateFile("f2");

            Assert.That(d1.GetEntries(),
                Is.EquivalentTo(new IVirtualFileSystemEntry[] { d2, f1 }));
            Assert.That(d1.GetEntries().Select(e => e.Path),
                Is.EquivalentTo(new[] { "/d1/d2", "/d1/f1" }));
        }

        [Test]
        public void TestNewDirectoryOrFileRaisesEvent()
        {
            var fs = NewFileSystem;
            var root = fs.Root;

            var events = new List<VirtualFileSystemEventArgs>();
            fs.FileSystemChanged += (s, e) => events.Add(e);

            var d1 = root.CreateDirectory("d1");
            var f1 = d1.CreateFile("f1");

            Assert.That(events[0].Entry, Is.EqualTo(d1));
            Assert.That(events[0].EventType, Is.EqualTo(VirtualFileSystemEventType.CreatedDirectory));
            Assert.That(events[1].Entry, Is.EqualTo(f1));
            Assert.That(events[1].EventType, Is.EqualTo(VirtualFileSystemEventType.CreatedFile));

            // since CreateFile automatically creates a new version, CreatedVersion event should be also registered
            Assert.That(events[2].Entry, Is.EqualTo(f1.CurrentVersion));
            Assert.That(events[2].EventType, Is.EqualTo(VirtualFileSystemEventType.CreatedVersion));
        }

        [Test]
        public void TestNewVersionRaisesEvent()
        {
            var fs = NewFileSystem;
            var root = fs.Root;

            var events = new List<VirtualFileSystemEventArgs>();

            fs.FileSystemChanged += (s, e) => events.Add(e);

            var f1 = root.CreateFile("f1");
            var v1 = f1.CurrentVersion;
            var v2 = v1.CreateVersion();
            var v3 = v2.CreateVersion();

            Assert.That(events[0].Entry, Is.EqualTo(f1));
            Assert.That(events[0].EventType, Is.EqualTo(VirtualFileSystemEventType.CreatedFile));
            Assert.That(events[1].Entry, Is.EqualTo(v1));
            Assert.That(events[1].EventType, Is.EqualTo(VirtualFileSystemEventType.CreatedVersion));
            Assert.That(events[2].Entry, Is.EqualTo(v2));
            Assert.That(events[2].EventType, Is.EqualTo(VirtualFileSystemEventType.CreatedVersion));
            Assert.That(events[3].Entry, Is.EqualTo(v3));
            Assert.That(events[3].EventType, Is.EqualTo(VirtualFileSystemEventType.CreatedVersion));
        }

        [Test]
        public void TestArgumentExceptionIsThrownWhenNameContainsSlashCharacter()
        {
            var fs = NewFileSystem;
            var root = fs.Root;
            Assert.Throws<ArgumentException>(() => root.CreateDirectory("a/directory"));
            Assert.Throws<ArgumentException>(() => root.CreateFile("a/file"));
        }

        [Test]
        public void TestArgumentExceptionIsThrownWhenDirectoryWithSameNameExists()
        {
            var fs = NewFileSystem;
            var root = fs.Root;

            var d1 = root.CreateDirectory("d1");
            Assert.That(d1.Path, Is.EqualTo("/d1"));

            Assert.Throws<ArgumentException>(() => root.CreateDirectory(d1.Name));
        }

        [Test]
        public void TestArgumentExceptionIsThrownWhenFileWithSameNameExists()
        {
            var fs = NewFileSystem;
            var root = fs.Root;

            var f1 = root.CreateFile("f1");
            Assert.That(f1.Path, Is.EqualTo("/f1"));

            Assert.Throws<ArgumentException>(() => root.CreateFile(f1.Name));
        }

        [Test]
        public void TestInvalidOperationExceptionIsThrownWhenDirectoryHasTooManyEntries()
        {
            var options = VirtualFileSystemOptions.Default;
            options.MaximumEntriesPerDirectory = 2;
            var fs = VirtualFileSystemFactory.Create(options);
            var root = fs.Root;

            root.CreateFile("f1");
            root.CreateFile("f2");

            Assert.Throws<InvalidOperationException>(() => root.CreateFile("f3"));
            Assert.Throws<InvalidOperationException>(() => root.CreateDirectory("d3"));
        }

        [Test]
        public void TestInvalidOperationExceptionIsThrownWhenFileHasTooManyVersions()
        {
            var options = VirtualFileSystemOptions.Default;
            options.MaximumVersionsPerFile = 3;
            var fs = VirtualFileSystemFactory.Create(options);
            var root = fs.Root;

            var f1 = root.CreateFile("f1");
            var v1 = f1.CurrentVersion.CreateVersion();
            var v2 = v1.CreateVersion();

            Assert.Throws<InvalidOperationException>(() => v2.CreateVersion());
        }

        [Test]
        public void TestInvalidOperationExceptionIsThrownWhenVersionAlreadyHasChildVersion()
        {
            var fs = NewFileSystem;
            var root = fs.Root;

            var f1 = root.CreateFile("f1");

            var v1 = f1.CurrentVersion.CreateVersion();
            v1.CreateVersion();

            Assert.Throws<InvalidOperationException>(() => v1.CreateVersion());
        }

        [Test]
        public void TestFileNotFoundExceptionIsThrownWhenFileNotFound()
        {
            var fs = NewFileSystem;
            Assert.Throws<FileNotFoundException>(() => fs.GetFile("unknown"));
        }

        [Test]
        public void TestDirectoryNotFoundExceptionIsThrownWhenDirectoryNotFound()
        {
            var fs = NewFileSystem;
            Assert.Throws<DirectoryNotFoundException>(() => fs.GetDirectory("unknown"));
        }

        [Test]
        public void TestFileCreatesAndReturnVersionsInTheOrderOfCreation()
        {
            var fs = NewFileSystem;
            var root = fs.Root;

            var f1 = root.CreateFile("f1");

            Assert.That(f1.Versions, Has.Count.EqualTo(1));
            Assert.That(f1.Versions.First(), Is.SameAs(f1.CurrentVersion));

            var v1 = f1.CurrentVersion;
            var v2 = f1.CurrentVersion.CreateVersion();
            var v3 = v2.CreateVersion();
            var v4 = v3.CreateVersion();

            Assert.That(f1.Versions, Has.Count.EqualTo(4));
            Assert.That(f1.Versions.First(), Is.SameAs(v1));
            Assert.That(f1.Versions.Skip(1).First(), Is.SameAs(v2));
            Assert.That(f1.Versions.Skip(2).First(), Is.SameAs(v3));
            Assert.That(f1.Versions.Skip(3).First(), Is.SameAs(v4));

            Assert.That(f1.CurrentVersion, Is.SameAs(v4));
        }

        [Test]
        public void TestFileSupportAtLeast1000Versions()
        {
            var options = VirtualFileSystemOptions.Default;
            const int versionCount = 1000;
            options.MaximumVersionsPerFile = versionCount + 1;

            var fs = VirtualFileSystemFactory.Create(options);
            var file = fs.Root.CreateFile("f1");
            var version = file.CurrentVersion;

            for (int i = 1; i <= versionCount; i++)
            {
                version = version.CreateVersion();
                using (var stream = version.GetStream())
                {
                    stream.SetLength(i * 1024L * 1024);
                    stream.Position = i;
                    stream.Write(TestBuffer, 0, TestBuffer.Length);
                }
            }

            Assert.That(file.Versions, Has.Count.EqualTo(versionCount + 1));
            Assert.That(file.CurrentVersion, Is.SameAs(version));

            for (int i = versionCount; i >= 1; i--)
            {
                using (var stream = version.GetStream())
                {
                    stream.Position = i;
                    Assert.That(stream.ReadByte(), Is.EqualTo(TestBuffer[0]));
                    version = version.ParentVersion;
                }
            }
        }

        [Test]
        public void TestVersionHasInitialZeroState()
        {
            var fs = NewFileSystem;
            var file = fs.Root.CreateFile("test");

            var v1 = file.CurrentVersion;
            var v2 = v1.CreateVersion();

            Assert.That(v1.Length, Is.Zero);
            Assert.That(v2.Length, Is.Zero);
            Assert.That(v1.Owner, Is.SameAs(file));
            Assert.That(v2.Owner, Is.SameAs(file));
            Assert.That(v1.ParentVersion, Is.Null);
            Assert.That(v2.ParentVersion, Is.SameAs(v1));

            foreach (var streamFunc in new Func<Stream>[] { () => v1.GetStream(), () => v2.GetStream() })
            {
                using (var stream = streamFunc())
                {
                    Assert.That(stream, Is.Not.Null);
                    Assert.That(stream.CanRead);
                    Assert.That(stream.CanSeek);
                    Assert.That(stream.CanWrite);
                    Assert.That(stream.CanTimeout, Is.False);
                    Assert.That(stream.Position, Is.Zero);
                    Assert.That(stream.Length, Is.Zero);
                    // flush is not expected to do anything,
                    // but it should not throw either
                    Assert.DoesNotThrow(() => stream.Flush());
                }
            }
        }

        [Test]
        public void TestVersionGetStreamReturnsDifferentStreams()
        {
            var fs = NewFileSystem;
            var file = fs.Root.CreateFile("test");

            var v1 = file.CurrentVersion;

            using (var stream1 = v1.GetStream())
            using (var stream2 = v1.GetStream())
            {
                Assert.That(stream1, Is.Not.SameAs(stream2));
            }
        }

        [Test]
        public void TestEndOfStreamExceptionIsThrownWhenStreamPositionIsInvalid()
        {
            var fs = NewFileSystem;
            using (var stream = NewVersionStream(fs))
            {
                Assert.Throws<EndOfStreamException>(() => { stream.Position = -1; });
                Assert.Throws<EndOfStreamException>(() => { stream.Position = 1; });
                Assert.Throws<EndOfStreamException>(() => { stream.Write(TestBuffer, 0, 4); });
                Assert.Throws<EndOfStreamException>(() => stream.Seek(0, SeekOrigin.Begin));
                Assert.Throws<EndOfStreamException>(() => stream.Seek(0, SeekOrigin.End));
                Assert.Throws<EndOfStreamException>(() => stream.Seek(-1, SeekOrigin.Begin));
            }
        }

        [Test]
        public void TestObjectDisposedExceptionIsThrownWhenVersionIsDisposed()
        {
            var fs = NewFileSystem;
            var root = fs.Root;

            var f1 = root.CreateFile("f1");
            var v1 = f1.CurrentVersion;
            var stream = v1.GetStream();

            stream.Dispose();

            Assert.Throws<ObjectDisposedException>(() => stream.SetLength(10));
            Assert.Throws<ObjectDisposedException>(() => stream.Seek(10, SeekOrigin.Begin));
            Assert.Throws<ObjectDisposedException>(() => stream.WriteByte(0));
            Assert.Throws<ObjectDisposedException>(() => stream.ReadByte());
        }

        [Test]
        public void TestVersionStreamDisposeCanBeCalledTwice()
        {
            var fs = NewFileSystem;
            var root = fs.Root;

            var f1 = root.CreateFile("f1");
            var v1 = f1.CurrentVersion;
            var stream = v1.GetStream();

            stream.Dispose();
            Assert.DoesNotThrow(() => stream.Dispose());
        }

        [Test]
        public void TestVersionGetStreamCallAfterDispose()
        {
            var fs = NewFileSystem;
            var root = fs.Root;

            var f1 = root.CreateFile("f1");
            var v1 = f1.CurrentVersion;
            using (var stream = v1.GetStream())
            {
                stream.SetLength(1);
                stream.WriteByte(0x11);
                stream.Dispose();

                using (var stream1A = v1.GetStream())
                {
                    Assert.That(stream1A.Length, Is.EqualTo(1));
                    stream1A.Position = 0;
                    Assert.That(stream1A.ReadByte(), Is.EqualTo(0x11));
                }
            }
        }

        [Test]
        public void TestVersionLengthUpdatedOnStreamSetLength()
        {
            var fs = NewFileSystem;
            var file = fs.Root.CreateFile("file");
            var v1 = file.CurrentVersion;

            using (var stream = v1.GetStream())
            {
                Assert.That(v1.Length, Is.Zero);
                Assert.That(stream.Length, Is.Zero);

                stream.SetLength(1);
                Assert.That(v1.Length, Is.EqualTo(1));
                Assert.That(stream.Length, Is.EqualTo(1));

                stream.SetLength(100);
                Assert.That(v1.Length, Is.EqualTo(100));
                Assert.That(stream.Length, Is.EqualTo(100));
            }
        }

        [Test]
        public void TestVersionStreamBecomesSeekableWhenSetLengthCalled()
        {
            var fs = NewFileSystem;
            using (var stream = NewVersionStream(fs))
            {
                Assert.That(stream.Position, Is.Zero);
                Assert.That(stream.Length, Is.Zero);

                Assert.Throws<EndOfStreamException>(() => stream.Position = 0);

                stream.SetLength(4);

                Assert.That(stream.Length, Is.EqualTo(4));
                Assert.That(stream.Position, Is.Zero);

                stream.Position = 2;
                Assert.That(stream.Position, Is.EqualTo(2));

                stream.Seek(1, SeekOrigin.Begin);
                Assert.That(stream.Position, Is.EqualTo(1));

                stream.Seek(1, SeekOrigin.Current);
                Assert.That(stream.Position, Is.EqualTo(2));

                stream.Seek(-1, SeekOrigin.End);
                Assert.That(stream.Position, Is.EqualTo(stream.Length - 1));
            }
        }

        [Test]
        public void TestVersionStreamSupportsCopying()
        {
            var fs = NewFileSystem;
            using (var stream = NewVersionStream(fs))
            {
                stream.SetLength(TestBuffer.Length);
                stream.Write(TestBuffer, 0, TestBuffer.Length);
                Assert.That(stream.Position, Is.EqualTo(TestBuffer.Length));

                stream.Position = 0;

                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    Assert.That(memoryStream.ToArray(), Is.EqualTo(TestBuffer));
                }

                using (var memoryStream = new MemoryStream(TestBuffer))
                {
                    stream.Position = 0;
                    memoryStream.CopyTo(stream);
                    Assert.That(TestBuffer, Is.EqualTo(memoryStream.ToArray()));
                }
            }
        }

        [Test]
        public void TestVersionChildVersionAndParentVersionPropertyLogic()
        {
            var fs = NewFileSystem;
            var file = fs.Root.CreateFile("test");

            var v1 = file.CurrentVersion;
            using (var stream1 = v1.GetStream())
            {
                stream1.SetLength(1);
            }

            // first file version should neither have parent nor child version
            Assert.That(v1.ParentVersion, Is.Null);
            Assert.That(v1.ChildVersion, Is.Null);

            var v2 = v1.CreateVersion();

            Assert.That(v1.ParentVersion, Is.Null);
            Assert.That(v1.ChildVersion, Is.SameAs(v2));

            Assert.That(v2.ParentVersion, Is.SameAs(v1));
            Assert.That(v2.ChildVersion, Is.Null);

            var v3 = v2.CreateVersion();

            Assert.That(v1.ParentVersion, Is.Null);
            Assert.That(v1.ChildVersion, Is.SameAs(v2));

            Assert.That(v2.ParentVersion, Is.SameAs(v1));
            Assert.That(v2.ChildVersion, Is.SameAs(v3));

            Assert.That(v3.ParentVersion, Is.SameAs(v2));
            Assert.That(v3.ChildVersion, Is.Null);
        }

        [Test]
        public void TestInvalidOperationExceptionIsThrownWhenWriteIsCalledForParentVersionStream()
        {
            var fs = NewFileSystem;
            var file = fs.Root.CreateFile("test");

            var v1 = file.CurrentVersion;
            IVirtualVersion v2;

            using (var stream1 = v1.GetStream())
            {
                stream1.SetLength(4);
                stream1.WriteByte(0x11);

                v2 = v1.CreateVersion();

                // parent stream must become immutable after CreateVersion call
                Assert.Throws<InvalidOperationException>(() =>
                {
                    stream1.WriteByte(0x11);
                });
                Assert.Throws<InvalidOperationException>(() =>
                {
                    stream1.SetLength(100);
                });
                Assert.Throws<InvalidOperationException>(() =>
                {
                    using (var stream1A = v1.GetStream())
                        stream1A.WriteByte(0x11);
                });
                Assert.Throws<InvalidOperationException>(() =>
                {
                    using (var stream1A = v1.GetStream())
                        stream1A.SetLength(100);
                });
            }

            // but child stream should be mutable
            using (var stream2 = v2.GetStream())
            {
                stream2.SetLength(4);
                stream2.WriteByte(0x22);
            }
        }

        [Test]
        public async Task TestVersionStreamSupportsAsyncCopying()
        {
            var fs = NewFileSystem;
            using (var stream = NewVersionStream(fs))
            {
                stream.SetLength(TestBuffer.Length);
                stream.Write(TestBuffer, 0, TestBuffer.Length);
                Assert.That(stream.Position, Is.EqualTo(TestBuffer.Length));

                stream.Position = 0;

                using (var memoryStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream);
                    Assert.That(memoryStream.ToArray(), Is.EqualTo(TestBuffer));
                }

                using (var memoryStream = new MemoryStream(TestBuffer))
                {
                    stream.Position = 0;
                    await memoryStream.CopyToAsync(stream);
                    Assert.That(TestBuffer, Is.EqualTo(memoryStream.ToArray()));
                }
            }
        }

        [Test]
        public void TestVersionStreamSupportsGigabyteCopying()
        {
            var fs = NewFileSystem;
            using (var stream = NewVersionStream(fs))
            {
                stream.SetLength(Gigabyte);
                Assert.That(stream.Length, Is.EqualTo(Gigabyte));

                stream.Position = Gigabyte - 1;
                stream.WriteByte(0xFF);

                stream.Position = 0;

                var buffer = new byte[64 * 1024];
                int read, totalRead = 0;
                while ((read = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    totalRead += read;
                }

                Assert.That(totalRead, Is.EqualTo(Gigabyte));
                Assert.That(buffer.Last(), Is.EqualTo(0xFF));
            }
        }

        [Test]
        public void TestVersionStreamSupportsTerabyteLength()
        {
            var fs = NewFileSystem;
            using (var stream = NewVersionStream(fs))
            {
                stream.SetLength(Terabyte);
                Assert.That(stream.Length, Is.EqualTo(Terabyte));

                stream.Position = Terabyte - 1;
                stream.WriteByte(0xFF);

                Assert.That(stream.Position, Is.EqualTo(Terabyte));

                stream.Seek(-1, SeekOrigin.End);
                Assert.That(stream.ReadByte(), Is.EqualTo(0xFF));

                stream.Seek(-1L * 1024 * 1024 * 1024, SeekOrigin.End);
                using (var nullStream = Stream.Null)
                {
                    stream.CopyTo(nullStream);
                }
                Assert.That(stream.Position, Is.EqualTo(Terabyte));
            }
        }

        [Test]
        public void TestVersionStreamSupportsOffsetAndCountArgumentsInReadWriteOperations()
        {
            var fs = NewFileSystem;
            using (var stream = NewVersionStream(fs))
            {
                var buffer = new byte[8];

                stream.SetLength(16);
                stream.Write(TestBuffer, 4, 5);
                stream.Position = 0;
                stream.Read(buffer, 2, 3);

                Assert.That(buffer, Is.EqualTo(
                    new byte[] {0x00, 0x00, (byte)'4', (byte)'5', (byte)'6', 0x00, 0x00, 0x00}));
            }
        }

        [Test]
        public void TestVersionStreamReadReturnsZeroesForUninitializedRegions()
        {
            var fs = NewFileSystem;
            using (var stream = NewVersionStream(fs))
            {
                stream.SetLength(3);
                stream.Position = 1;
                stream.WriteByte(0x10);

                stream.Position = 0;
                Assert.That(stream.ReadByte(), Is.Zero);
                Assert.That(stream.ReadByte(), Is.EqualTo(0x10));
                Assert.That(stream.ReadByte(), Is.Zero);

                var buffer = new byte[] {0xDE, 0xAD, 0xBE};
                stream.Position = 0;
                stream.Read(buffer, 0, buffer.Length);
                Assert.That(buffer, Is.EqualTo(new[]{0x00, 0x10, 0x00}));
            }
        }

        [Test]
        public void TestVersionStreamResetsWrittenDataWhenLengthIsTruncated()
        {
            var fs = NewFileSystem;
            using (var stream = NewVersionStream(fs))
            {
                stream.SetLength(TestBuffer.Length * 2);
                stream.Write(TestBuffer, 0, TestBuffer.Length);
                stream.Write(TestBuffer, 0, TestBuffer.Length);

                int truncatedLength = TestBuffer.Length - 2;
                stream.SetLength(truncatedLength);
                stream.Position = 0;

                var buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                Assert.That(buffer, Is.EqualTo(
                    TestBuffer.Take(truncatedLength).ToArray()));

                stream.SetLength(TestBuffer.Length * 2);
                buffer = new byte[stream.Length];
                stream.Position = 0;
                stream.Read(buffer, 0, buffer.Length);
                Assert.That(buffer, Is.EqualTo(
                    TestBuffer.Take(truncatedLength)
                        .Concat(Enumerable.Range(0, (int)stream.Length - truncatedLength).Select(i => (byte)0x00))
                        .ToArray()));

                stream.SetLength(0);
                stream.SetLength(2);
                stream.Position = 0;
                Assert.That(stream.ReadByte(), Is.Zero);
                Assert.That(stream.ReadByte(), Is.Zero);
            }
        }

        [Test]
        public void TestVersionStreamSupportsLayers()
        {
            var fs = NewFileSystem;
            var file = fs.Root.CreateFile("test");

            var v1 = file.CurrentVersion;
            using (var stream1 = v1.GetStream())
            {
                // v1: ?? 10 11 12 ?? ?? ?? ?? 13 ??
                stream1.SetLength(10);
                stream1.Position = 1;
                stream1.WriteByte(0x10);
                stream1.WriteByte(0x11);
                stream1.WriteByte(0x12);
                stream1.Position = 8;
                stream1.WriteByte(0x13);
                stream1.Position = 0;
            }

            var v2 = v1.CreateVersion();
            using (var stream2 = v2.GetStream())
            {
                // v2: ?? ?? 20 21 ?? 22 23 ??
                stream2.SetLength(8);
                stream2.Position = 2;
                stream2.WriteByte(0x20);
                stream2.WriteByte(0x21);
                stream2.Position = 5;
                stream2.WriteByte(0x22);
                stream2.WriteByte(0x23);
                stream2.Position = 0;
            }

            var v3 = v2.CreateVersion();
            using (var stream3 = v3.GetStream())
            {
                // v3: ?? ?? ?? 30 31 ?? ?? 32 ?? ?? ..
                stream3.SetLength(100);
                stream3.Position = 3;
                stream3.WriteByte(0x30);
                stream3.WriteByte(0x31);
                stream3.Position = 7;
                stream3.WriteByte(0x32);
                stream3.Position = 0;
            }

            using (var stream1 = v1.GetStream())
            using (var stream2 = v2.GetStream())
            using (var stream3 = v3.GetStream())
            {
                var buffer = new byte[10];

                // the result v1 stream should be:
                // v1:  ?? 10 11 12 ?? ?? ?? ?? 13 ??
                // ----------------------------------
                // *v1: 00 10 11 12 00 00 00 00 13 00

                Assert.That(stream1.Read(buffer, 0, buffer.Length), Is.EqualTo(stream1.Length));
                Assert.That(buffer, Is.EqualTo(new byte[]
                {
                    0x00, 0x10, 0x11, 0x12, 0x00, 0x00, 0x00, 0x00, 0x13, 0x00
                }));

                // the result v2 stream should be:
                // v1:  ?? 10 11 12 ?? ?? ?? ?? 13 ??
                // v2:  ?? ?? 20 21 ?? 22 23 ??
                // ----------------------------------
                // *v2: 00 10 20 21 00 22 23 00

                Assert.That(stream2.Read(buffer, 0, buffer.Length), Is.EqualTo(stream2.Length));
                Assert.That(buffer.Take((int)stream2.Length).ToArray(), Is.EqualTo(new byte[]
                {
                    0x00, 0x10, 0x20, 0x21, 0x00, 0x22, 0x23, 0x00
                }));

                // the result v3 stream should be:
                // v1:  ?? 10 11 12 ?? ?? ?? ?? 13 ??
                // v2:  ?? ?? 20 21 ?? 22 23 ??
                // v3:  ?? ?? ?? 30 31 ?? ?? 32 ?? ?? ..
                // ----------------------------------
                // *v3: 00 10 20 30 31 22 23 32 13 00

                Assert.That(stream3.Read(buffer, 0, buffer.Length), Is.EqualTo(buffer.Length));
                Assert.That(buffer, Is.EqualTo(new byte[]
                {
                    0x00, 0x10, 0x20, 0x30, 0x31, 0x22, 0x23, 0x32, 0x13, 0x00
                }));
            }
        }

        [Test]
        public void TestVersionStreamSupportsOverlappedReadWriteOperations()
        {
            var fs = NewFileSystem;
            using (var stream = NewVersionStream(fs))
            {
                var buffer = new byte[10];
                stream.SetLength(10);

                // write bytes so that the data stream looks like:
                // stream: 10 ?? 12 ?? 14 ?? ?? ?? ?? ??

                stream.WriteByte(0x10);
                stream.Position++;
                stream.WriteByte(0x12);
                stream.Position++;
                stream.WriteByte(0x14);

                // overwrite data from 2nd position:
                // stream: 10 ?? 30 31 32 33 34 ?? ?? ??
                stream.Position = 2;
                stream.Write(TestBuffer, 0, 5);

                // overwrite data from 6nd position:
                // stream: 10 ?? 30 31 32 33 17 ?? ?? ??
                stream.Position = 6;
                stream.WriteByte(0x17);

                // finally, read 10 bytes from 0 position, the correct result would be:
                // stream: 10 00 30 31 32 33 17 00 00 00
                stream.Position = 0;

                Assert.That(stream.Read(buffer, 0, buffer.Length), Is.EqualTo(stream.Length));
                Assert.That(buffer, Is.EqualTo(
                    new byte[]
                    {
                        0x10, 0x00, (byte)'0', (byte)'1', (byte)'2', (byte)'3', 0x17, 0x00, 0x00, 0x00
                    }));

                stream.SetLength(Terabyte);

                for (long i = 0; i < 1000; i++)
                {
                    stream.Position = i * Gigabyte;
                    stream.Write(TestBuffer, 0, TestBuffer.Length);
                }

                for (long i = 0; i < 1000; i++)
                {
                    stream.Position = i * Gigabyte;
                    stream.Read(buffer, 0, buffer.Length);
                    Assert.That(buffer, Is.EqualTo(TestBuffer));
                }
            }
        }

        [Test]
        public void TestVersionStreamSupportsMultipleOverlappedWriteOperations()
        {
            var fs = NewFileSystem;
            using (var stream = NewVersionStream(fs))
            {
                const int chunkCount = 100000;
                stream.SetLength(chunkCount * 2);

                for (int i = 0; i < chunkCount; i++)
                {
                    stream.Position = i;
                    stream.Write(TestBuffer, 0, TestBuffer.Length);
                }

                stream.Seek(-TestBuffer.Length, SeekOrigin.Current);

                var buffer = new byte[TestBuffer.Length];
                stream.Read(buffer, 0, buffer.Length);
                Assert.That(buffer, Is.EqualTo(TestBuffer));

                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(buffer, 0, buffer.Length);
                Assert.That(buffer, Is.EqualTo(
                    Enumerable.Range(0, TestBuffer.Length)
                        .Select(x => TestBuffer[0]).ToArray()));
            }
        }

        [Test]
        public void TestCaseSensitiveDirectoryEntries()
        {
            var fs = NewFileSystem;
            fs.Root.CreateDirectory("D1");
            fs.Root.CreateDirectory("d1");
            fs.Root.CreateFile("f1");
            fs.Root.CreateFile("F2");
            Assert.AreEqual(4, fs.Root.GetEntries().Count);
        }
    }
}
