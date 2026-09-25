using Dialuverc.Editor.Base.IO;
using System.IO.Compression;

namespace Dialuverc.Editor.Tests.Base
{
    internal class ProjectExporterTests
    {
        [Test]
        public void ZipArchiveCreation()
        {
            TestImportableObject[] toExport = new TestImportableObject[]
            {
                new TestImportableObject("file1", "content1"),
                new TestImportableObject("file2", "content2"),
                new TestImportableObject("file3", "content3"),
            };

            using (MemoryStream memoryStream = new MemoryStream())
            {
                ProjectExporter.CreateZip(toExport.AsEnumerable(), memoryStream, true);

                using (ZipArchive readArchive = new ZipArchive(memoryStream))
                {
                    Assert.That(readArchive.Entries, Has.Count.EqualTo(toExport.Length));

                    for (int i = 0; i < toExport.Length; i++)
                    {
                        Assert.That(readArchive.Entries[i].Name, Is.EqualTo(toExport[i].ExportPath));

                        using (Stream stream = readArchive.Entries[i].Open())
                        {
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                Assert.That(reader.ReadToEnd(), Is.EqualTo(toExport[i].Content));
                            }
                        }
                    }
                }
            }
        }

        [Test]
        public void ExportToFolder()
        {
            TestImportableObject[] toExport = new TestImportableObject[]
            {
                new TestImportableObject("file1", "content1"),
                new TestImportableObject("file2", "content2"),
                new TestImportableObject("folder/file3", "content3"),
            };

            using (TemporaryFileStorage storage = new TemporaryFileStorage(
                Path.Combine(Path.GetTempPath(), $"Dialuverc{nameof(ProjectExporterTests)}{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}")))
            {
                ProjectExporter.ExportToFolder(toExport.AsEnumerable(), storage.FolderPath);

                for (int i = 0; i < toExport.Length; i++)
                {
                    string filePath = Path.Combine(storage.FolderPath, toExport[i].ExportPath);

                    Assert.That(File.Exists(filePath), Is.True);
                    Assert.That(File.ReadAllText(filePath), Is.EqualTo(toExport[i].Content));
                }
            }
        }

        [Test]
        public void ImportFromZip()
        {
            TestImportableObject first = new TestImportableObject("firstFile", "firstContent");
            TestImportableObject second = new TestImportableObject("secondFile", "secondContent");
            TestImportableObject third = new TestImportableObject("folder/thirdFile", "thirdContent");

            TestImportableObject[] importables = new TestImportableObject[] { first, second, third };

            using (MemoryStream memoryStream = new MemoryStream())
            {
                ProjectExporter.CreateZip(importables, memoryStream, true);

                foreach (TestImportableObject importable in importables)
                {
                    Assert.That(importable.ImportedContent, Is.Null);
                }

                ProjectExporter.ImportFromZip(importables, memoryStream);

                foreach (TestImportableObject importable in importables)
                {
                    Assert.That(importable.ImportedContent, Is.EqualTo(importable.Content));
                }
            }
        }

        [Test]
        public void ImportFromFolder()
        {
            TestImportableObject first = new TestImportableObject("firstFile", "firstContent");
            TestImportableObject second = new TestImportableObject("secondFile", "secondContent");
            TestImportableObject third = new TestImportableObject("folder/thirdFile", "thirdContent");

            TestImportableObject[] importables = new TestImportableObject[] { first, second, third };

            using (TemporaryFileStorage storage = new TemporaryFileStorage(
                Path.Combine(Path.GetTempPath(), $"Dialuverc{nameof(ProjectExporterTests)}{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}")))
            {
                ProjectExporter.ExportToFolder(importables, storage.FolderPath);

                foreach (TestImportableObject importable in importables)
                {
                    Assert.That(importable.ImportedContent, Is.Null);
                }

                ProjectExporter.ImportFromFolder(importables, storage.FolderPath);

                foreach (TestImportableObject importable in importables)
                {
                    Assert.That(importable.ImportedContent, Is.EqualTo(importable.Content));
                }
            }
        }

        class TestImportableObject : IImportable
        {
            public const string ImportFailedContent = "Failed";

            readonly string _exportName;
            public string ExportPath => _exportName;

            public readonly string Content;

            public string? ImportedContent { get; private set; }

            public TestImportableObject(string exportName, string content)
            {
                _exportName = exportName;

                Content = content;
            }

            public void SerializeForExport(Stream stream)
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(Content);
                }
            }

            public void DeserializeForImport(Stream stream)
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string result = reader.ReadToEnd();

                    if (string.IsNullOrWhiteSpace(result))
                        result = ImportFailedContent;

                    ImportedContent = result;
                }
            }
        }
    }
}
