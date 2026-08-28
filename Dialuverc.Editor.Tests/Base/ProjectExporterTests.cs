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

            MemoryStream result = ProjectExporter.CreateZip(toExport.AsEnumerable());

            using (ZipArchive readArchive = new ZipArchive(result))
            {
                Assert.That(readArchive.Entries, Has.Count.EqualTo(toExport.Length));

                for (int i = 0; i < toExport.Length; i++)
                {
                    Assert.That(readArchive.Entries[i].Name, Is.EqualTo(toExport[i].ExportName));

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

        class TestExportableObject : IExportable
        {
            readonly string _exportName;
            public string ExportName => _exportName;

            public readonly string Content;

            public TestExportableObject(string exportName, string content)
            {
                _exportName = exportName;
                
                Content = content;
            }

            public string SerializeForExport() => Content;
        }
    }
}
