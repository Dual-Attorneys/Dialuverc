using Dialuverc.Editor.Base.IO;
using Dialuverc.Editor.Base.Project;

namespace Dialuverc.Editor.Tests.Base
{
    internal class DialuvercProjectTests
    {
        [Test]
        public void SaveSuccessCreatesBackup()
        {
            TestingImportable[] importables = new TestingImportable[]
            {
                new TestingImportable("file1")
            };

            // Only saving creates backups, exporting doesn't.
            TestingImportable[] exportables = Array.Empty<TestingImportable>();

            IProjectTrackable[] trackables = new IProjectTrackable[]
            {
                new TestingProjectTrackable(importables, exportables),
            };

            string firstContent = "Hello";
            string secondContent = "World";

            using (TemporaryFileStorage tempStorage = new TemporaryFileStorage(nameof(DialuvercProjectTests)))
            {
                DialuvercProject project = new DialuvercProject(
                    Path.Combine(tempStorage.AbsoluteFolderPath, "DialuvercProject"),
                    trackables);

                string filePath = Path.Combine(project.ContentFolderPath, importables[0].ExportPath);

                importables[0].Content = firstContent;

                Assert.That(project.SaveProject(), Is.True);

                Assert.That(Directory.Exists(project.BackupsFolderPath), Is.False);
                Assert.That(Directory.Exists(project.ContentFolderPath), Is.True);

                Assert.That(File.Exists(filePath), Is.True);
                Assert.That(File.ReadAllText(filePath), Is.EqualTo(firstContent));

                importables[0].Content = secondContent;

                Assert.That(project.SaveProject(), Is.True);

                Assert.That(Directory.Exists(project.BackupsFolderPath), Is.True);

                string[] backupSubdirectories = Directory.GetDirectories(project.BackupsFolderPath);

                Assert.That(backupSubdirectories.Length, Is.EqualTo(1));

                string[] thisBackupSubdirectories = Directory.GetDirectories(backupSubdirectories[0]);

                Assert.That(thisBackupSubdirectories.Length, Is.EqualTo(1));

                string contentBackupSubdirectoryName = thisBackupSubdirectories[0].AsSpan()
                    .Slice(backupSubdirectories[0].Length)
                    .Trim(Path.DirectorySeparatorChar)
                    .ToString();

                Assert.That(contentBackupSubdirectoryName, Is.EqualTo("Content"));

                string[] contentBackupSubdirectoryFiles = Directory.GetFiles(thisBackupSubdirectories[0]);

                Assert.That(contentBackupSubdirectoryFiles.Length, Is.EqualTo(1));
                Assert.That(File.ReadAllText(contentBackupSubdirectoryFiles[0]), Is.EqualTo(firstContent));

                Assert.That(Directory.Exists(project.ContentFolderPath), Is.True);

                Assert.That(File.Exists(filePath), Is.True);
                Assert.That(File.ReadAllText(filePath), Is.EqualTo(importables[0].Content));
            }
        }

        [Test]
        public void SaveFailRestoresLatestBackup()
        {
            TestingImportable[] importables = new TestingImportable[]
            {
                new TestingImportable("file1")
            };

            // Only saving creates backups, exporting doesn't.
            TestingImportable[] exportables = Array.Empty<TestingImportable>();

            IProjectTrackable[] trackables = new IProjectTrackable[]
            {
                new TestingProjectTrackable(importables, exportables),
            };

            string firstContent = "Hello";
            string secondContent = "World";

            using (TemporaryFileStorage tempStorage = new TemporaryFileStorage(nameof(DialuvercProjectTests)))
            {
                DialuvercProject project = new DialuvercProject(
                    Path.Combine(tempStorage.AbsoluteFolderPath, "DialuvercProject"),
                    trackables);

                string filePath = Path.Combine(project.ContentFolderPath, importables[0].ExportPath);

                importables[0].Content = firstContent;

                Assert.That(project.SaveProject(), Is.True);
                Assert.That(File.ReadAllText(filePath), Is.EqualTo(firstContent));

                importables[0].Content = secondContent;
                importables[0].ThrowOn = ImportableThrowOn.Serialize;

                Assert.That(project.SaveProject(), Is.False);
                Assert.That(File.ReadAllText(filePath), Is.EqualTo(firstContent));
            }
        }

        private class TestingProjectTrackable : IProjectTrackable
        {
            public bool HasUnsavedChanges { get; private set; }

            readonly IEnumerable<IImportable> _importables;
            readonly IEnumerable<IExportable> _exportables;

            public TestingProjectTrackable(IEnumerable<IImportable> importables, IEnumerable<IExportable> exportables)
            {
                _importables = importables;
                _exportables = exportables;
            }

            // This is usually needed by DialuvercProject, but not for tests.
            public void ClearStatesHistory() => throw new NotImplementedException();

            public IEnumerable<IImportable> GetEditorImportables() => _importables;

            public IEnumerable<IExportable> GetGameExportables() => _exportables;

            public void SetCurrentStateAsSaved() => HasUnsavedChanges = false;
        }

        private class TestingImportable : IImportable
        {
            readonly string _exportPath;
            public string ExportPath => _exportPath;

            public ImportableThrowOn ThrowOn { get; set; } = ImportableThrowOn.None;

            public string? Content { get; set; }

            public TestingImportable(string exportPath)
            {
                _exportPath = exportPath;
            }

            public void DeserializeForImport(Stream stream)
            {
                if (ThrowOn == ImportableThrowOn.Both || ThrowOn == ImportableThrowOn.Deserialize)
                    throw new Exception($"Importing threw an exception because {nameof(ThrowOn)} is {ThrowOn}");

                using (StreamReader reader = new StreamReader(stream))
                {
                    Content = reader.ReadToEnd();
                }
            }

            public void SerializeForExport(Stream stream)
            {
                if (ThrowOn == ImportableThrowOn.Both || ThrowOn == ImportableThrowOn.Serialize)
                    throw new Exception($"Exporting threw an exception because {nameof(ThrowOn)} is {ThrowOn}");

                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(Content);
                }
            }
        }

        private enum ImportableThrowOn
        {
            None,
            Deserialize,
            Serialize,
            Both,
        }
    }
}
