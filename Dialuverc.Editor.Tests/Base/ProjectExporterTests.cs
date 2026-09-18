using Dialuverc.Editor.Base.IO;
using System.IO.Compression;

namespace Dialuverc.Editor.Tests.Base
{
    internal class ProjectExporterTests
    {
        [Test]
        public void ZipArchiveCreation()
        {
            TestExportableObject[] toExport = new TestExportableObject[]
            {
                new TestExportableObject("file1", "content1"),
                new TestExportableObject("file2", "content2"),
                new TestExportableObject("file3", "content3"),
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
            TestExportableObject[] toExport = new TestExportableObject[]
            {
                new TestExportableObject("file1", "content1"),
                new TestExportableObject("file2", "content2"),
                new TestExportableObject("folder/file3", "content3"),
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
            TestExportableObject first = new TestExportableObject("firstFile", "firstContent");
            TestExportableObject second = new TestExportableObject("secondFile", "secondContent");
            TestExportableObject third = new TestExportableObject("folder/thirdFile", "thirdContent");

            TestExportableObject[] exportables = new TestExportableObject[] { first, second, third };

            using (MemoryStream memoryStream = new MemoryStream())
            {
                ProjectExporter.CreateZip(exportables, memoryStream, true);

                foreach (TestExportableObject exportable in exportables)
                {
                    Assert.That(exportable.ImportedContent, Is.Null);
                }

                ProjectExporter.ImportFromZip(exportables, memoryStream);

                foreach (TestExportableObject exportable in exportables)
                {
                    Assert.That(exportable.ImportedContent, Is.EqualTo(exportable.Content));
                }
            }
        }

        [Test]
        public void ImportFromFolder()
        {
            TestExportableObject first = new TestExportableObject("firstFile", "firstContent");
            TestExportableObject second = new TestExportableObject("secondFile", "secondContent");
            TestExportableObject third = new TestExportableObject("folder/thirdFile", "thirdContent");

            TestExportableObject[] exportables = new TestExportableObject[] { first, second, third };

            using (TemporaryFileStorage storage = new TemporaryFileStorage(
                Path.Combine(Path.GetTempPath(), $"Dialuverc{nameof(ProjectExporterTests)}{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}")))
            {
                ProjectExporter.ExportToFolder(exportables, storage.FolderPath);

                foreach (TestExportableObject exportable in exportables)
                {
                    Assert.That(exportable.ImportedContent, Is.Null);
                }

                ProjectExporter.ImportFromFolder(exportables, storage.FolderPath);

                foreach (TestExportableObject exportable in exportables)
                {
                    Assert.That(exportable.ImportedContent, Is.EqualTo(exportable.Content));
                }
            }
        }

        class TestExportableObject : IExportable
        {
            public const string ImportFailedContent = "Failed";

            readonly string _exportName;
            public string ExportPath => _exportName;

            public readonly string Content;

            public string? ImportedContent { get; private set; }

            public TestExportableObject(string exportName, string content)
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
