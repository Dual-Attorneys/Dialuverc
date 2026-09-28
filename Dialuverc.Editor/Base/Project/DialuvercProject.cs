using Dialuverc.Editor.Base.IO;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Dialuverc.Editor.Base.Project
{
    public class DialuvercProject
    {
        public string ProjectFolderPath { get; private set; }
        public string ContentFolderPath { get; private set; }
        public string BackupsFolderPath { get; private set; }

        readonly IEnumerable<IProjectTrackable> _projectTrackables;

        DialuvercProjectInfo _projectInfo = new DialuvercProjectInfo();

        public string Name
        {
            get => _projectInfo.Name;
            set => _projectInfo.Name = value;
        }

        public DialuvercProject(string folderPath, IEnumerable<IProjectTrackable> projectTrackables)
        {
            ArgumentNullException.ThrowIfNull(projectTrackables);

            UpdatePathsFromProjectFolder(folderPath);

            _projectTrackables = projectTrackables;
        }

        /// <summary>
        /// Moves this project's folder to the passed <paramref name="newFolderPath"/>.<br/>
        /// Relative paths (e.g. <see cref="ContentFolderPath"/>, <see cref="BackupsFolderPath"/>) are updated accordingly.
        /// </summary>
        public void MoveProject(string newFolderPath)
        {
            if (ProjectFolderPath is not null &&
                !ProjectFolderPath.Equals(newFolderPath, StringComparison.OrdinalIgnoreCase))
                Directory.Move(ProjectFolderPath, newFolderPath);

            UpdatePathsFromProjectFolder(newFolderPath);

            Directory.CreateDirectory(ProjectFolderPath);
        }

        // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/attributes/nullable-analysis#helper-methods-membernotnull-and-membernotnullwhen
        [MemberNotNull(nameof(ProjectFolderPath),
            nameof(ContentFolderPath),
            nameof(BackupsFolderPath))]
        void UpdatePathsFromProjectFolder(string newFolderPath)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(newFolderPath);

            if (!Path.IsPathFullyQualified(newFolderPath))
                throw new ArgumentException($"'{newFolderPath}' must be an absolute path", nameof(newFolderPath));

            ProjectFolderPath = newFolderPath;
            ContentFolderPath = Path.Combine(newFolderPath, "Content");
            BackupsFolderPath = Path.Combine(newFolderPath, "Backups");
        }

        IEnumerable<IImportable> EnumerateImportables()
        {
            foreach (IProjectTrackable trackable in _projectTrackables)
            {
                foreach (IImportable importable in trackable.GetEditorImportables())
                {
                    yield return importable;
                }
            }
        }

        // TODO: Proper error handling.
        public bool SaveProject()
        {
            string? tempProjectInfoPath = null;
            string? tempContentFolderPath = null;

            string? pathUsedForBackup = null;

            try
            {
                Directory.CreateDirectory(ProjectFolderPath);

                tempProjectInfoPath = Path.Combine(ProjectFolderPath, ".dialuverc.tmp");

                // IImportable can't currently be used for this as they require writing/reading to/from the same file (".dialuverc.tmp" <-> ".dialuverc").
                // This can be fixed by making the saving process run entirely inside a temp project folder (if needed).
                using (FileStream projectInfoStream = File.Open(tempProjectInfoPath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    JsonSerializer.Serialize(projectInfoStream, _projectInfo);
                }

                tempContentFolderPath = Path.Combine(ProjectFolderPath, $"TempContent{Guid.NewGuid()}");

                pathUsedForBackup = MoveCurrentSaveToBackups();

                ProjectExporter.ExportToFolder(EnumerateImportables(), tempContentFolderPath);

                // If Content exists, it's empty (MoveCurrentSaveToBackups would have moved it if it had anything inside).
                if (Directory.Exists(ContentFolderPath))
                    Directory.Delete(ContentFolderPath);

                string projectInfoPath = Path.Combine(ProjectFolderPath, ".dialuverc");

                if (File.Exists(projectInfoPath))
                    File.Delete(projectInfoPath);

                File.Move(tempProjectInfoPath, projectInfoPath);

                Directory.Move(tempContentFolderPath, ContentFolderPath);

                foreach (IProjectTrackable trackable in _projectTrackables)
                {
                    trackable.SetCurrentStateAsSaved();
                }

                return true;
            }
            catch
            {
                if (File.Exists(tempProjectInfoPath))
                    File.Delete(tempProjectInfoPath);

                if (Directory.Exists(tempContentFolderPath))
                    Directory.Delete(tempContentFolderPath, true);

                if (Directory.Exists(pathUsedForBackup))
                    Directory.Move(pathUsedForBackup, ContentFolderPath);

                return false;
            }
        }

        string? MoveCurrentSaveToBackups()
        {
            if (!Directory.Exists(ContentFolderPath))
                return null;

            if (!Directory.EnumerateFileSystemEntries(ContentFolderPath).Any())
                return null;

            Directory.CreateDirectory(BackupsFolderPath);

            string usableBackupPath = Path.Combine(BackupsFolderPath, $"Backup{Guid.NewGuid()}");
            Directory.CreateDirectory(usableBackupPath);

            string usableContentBackupPath = Path.Combine(usableBackupPath, "Content");

            Directory.Move(ContentFolderPath, usableContentBackupPath);

            return usableContentBackupPath;
        }
    }
}
