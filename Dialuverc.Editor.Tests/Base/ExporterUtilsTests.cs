using Dialuverc.Editor.Base.IO;
using System.IO.Compression;

namespace Dialuverc.Editor.Tests.Base
{
    internal class ExporterUtilsTests
    {
        const string _tempStorageName = nameof(ExporterUtilsTests);

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
                ExportableUtils.CreateZip(toExport.AsEnumerable(), memoryStream, true);

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

            using (TemporaryFileStorage storage = new TemporaryFileStorage(_tempStorageName))
            {
                ExportableUtils.ExportToFolder(toExport.AsEnumerable(), storage.AbsoluteFolderPath);

                for (int i = 0; i < toExport.Length; i++)
                {
                    string filePath = Path.Combine(storage.AbsoluteFolderPath, toExport[i].ExportPath);

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
                ExportableUtils.CreateZip(importables, memoryStream, true);

                foreach (TestImportableObject importable in importables)
                {
                    Assert.That(importable.ImportedContent, Is.Null);
                }

                ExportableUtils.ImportFromZip(importables, memoryStream);

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

            using (TemporaryFileStorage storage = new TemporaryFileStorage(_tempStorageName))
            {
                ExportableUtils.ExportToFolder(importables, storage.AbsoluteFolderPath);

                foreach (TestImportableObject importable in importables)
                {
                    Assert.That(importable.ImportedContent, Is.Null);
                }

                ExportableUtils.ImportFromFolder(importables, storage.AbsoluteFolderPath);

                foreach (TestImportableObject importable in importables)
                {
                    Assert.That(importable.ImportedContent, Is.EqualTo(importable.Content));
                }
            }
        }

        [TestCase("folder/file.txt", "folder", ExportableUtils.AllowedDirectoryTraversal.None)]
        [TestCase("folder/file.txt", "folder/", ExportableUtils.AllowedDirectoryTraversal.None)]
        [TestCase("folder/../file.txt", "folder", ExportableUtils.AllowedDirectoryTraversal.Parent)]
        [TestCase("folder/../file.txt", "folder/", ExportableUtils.AllowedDirectoryTraversal.Parent)]
        [TestCase("folder/subfolder/file.txt", "folder", ExportableUtils.AllowedDirectoryTraversal.Parent)]
        [TestCase("folder/subfolder/file.txt", "folder/", ExportableUtils.AllowedDirectoryTraversal.Parent)]
        [TestCase("folder", "folder", ExportableUtils.AllowedDirectoryTraversal.None)]
        [TestCase("folder/", "folder", ExportableUtils.AllowedDirectoryTraversal.None)]
        [TestCase("folder", "folder/", ExportableUtils.AllowedDirectoryTraversal.None)]
        [TestCase("folder", "folder", ExportableUtils.AllowedDirectoryTraversal.Parent)]
        [TestCase("folder/", "folder", ExportableUtils.AllowedDirectoryTraversal.Parent)]
        [TestCase("folder", "folder/", ExportableUtils.AllowedDirectoryTraversal.Parent)]
        public void PathTraversalCasesSuccess(string targetPath, string referenceFolder, ExportableUtils.AllowedDirectoryTraversal allowedTraversal)
        {
            Assert.That(ExportableUtils.PathDirectoryTraversalIsAllowed(targetPath, referenceFolder, allowedTraversal), Is.True);
        }

        [TestCase("folder/../file.txt", "folder", ExportableUtils.AllowedDirectoryTraversal.None)]
        [TestCase("folder/../file.txt", "folder/", ExportableUtils.AllowedDirectoryTraversal.None)]
        [TestCase("folder/../../file.txt", "folder", ExportableUtils.AllowedDirectoryTraversal.Parent)]
        [TestCase("folder/../../file.txt", "folder/", ExportableUtils.AllowedDirectoryTraversal.Parent)]
        public void PathTraversalCasesFail(string targetPath, string referenceFolder, ExportableUtils.AllowedDirectoryTraversal allowedTraversal)
        {
            Assert.That(ExportableUtils.PathDirectoryTraversalIsAllowed(targetPath, referenceFolder, allowedTraversal), Is.False);
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
