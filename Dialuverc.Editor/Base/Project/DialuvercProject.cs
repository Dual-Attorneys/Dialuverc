using Dialuverc.Editor.Base.IO;
using System.Diagnostics.CodeAnalysis;

namespace Dialuverc.Editor.Base.Project
{
    public class DialuvercProject
    {
        public string ProjectFolderPath { get; private set; }
        public string ContentFolderPath { get; private set; }
        public string BackupsFolderPath { get; private set; }

        readonly IEnumerable<IProjectTrackable> _projectTrackables;

        public DialuvercProject(string folderPath, IEnumerable<IProjectTrackable> projectTrackables)
        {
            ArgumentNullException.ThrowIfNull(projectTrackables);

            MoveProject(folderPath);

            _projectTrackables = projectTrackables;
        }

        /// <summary>
        /// Moves this project's folder to the passed <paramref name="newFolderPath"/>.<br/>
        /// Relative paths (e.g. <see cref="ContentFolderPath"/>, <see cref="BackupsFolderPath"/>) are updated accordingly.
        /// </summary>
        // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/attributes/nullable-analysis#helper-methods-membernotnull-and-membernotnullwhen
        [MemberNotNull(nameof(ProjectFolderPath),
            nameof(ContentFolderPath),
            nameof(BackupsFolderPath))]
        public void MoveProject(string newFolderPath)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(newFolderPath);

            if (!Path.IsPathFullyQualified(newFolderPath))
                throw new ArgumentException($"'{newFolderPath}' must be an absolute path", nameof(newFolderPath));

            // We may be initializing the paths without actually moving the folder.
            if (ProjectFolderPath is not null &&
                !ProjectFolderPath.Equals(newFolderPath, StringComparison.OrdinalIgnoreCase))
                Directory.Move(ProjectFolderPath, newFolderPath);

            ProjectFolderPath = newFolderPath;
            ContentFolderPath = Path.Combine(newFolderPath, "Content");
            BackupsFolderPath = Path.Combine(newFolderPath, "Backups");

            // No-op if the folder already exists.
            Directory.CreateDirectory(ProjectFolderPath);
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
            string? tempFolderPath = null;

            try
            {
                tempFolderPath = Path.Combine(ProjectFolderPath, $"TempContent{Guid.NewGuid()}");
                Directory.CreateDirectory(tempFolderPath);

                MoveCurrentSaveToBackups();

                ProjectExporter.ExportToFolder(EnumerateImportables(), tempFolderPath);

                // If Content exists, it's empty (MoveCurrentSaveToBackups would have moved it if it had anything inside).
                if (Directory.Exists(ContentFolderPath))
                    Directory.Delete(ContentFolderPath);

                Directory.Move(tempFolderPath, ContentFolderPath);

                foreach (IProjectTrackable trackable in _projectTrackables)
                {
                    trackable.SetCurrentStateAsSaved();
                }

                return true;
            }
            catch
            {
                if (Directory.Exists(tempFolderPath))
                    Directory.Delete(tempFolderPath, true);

                return false;
            }
        }

        void MoveCurrentSaveToBackups()
        {
            if (!Directory.Exists(ContentFolderPath))
                return;

            if (!Directory.EnumerateFileSystemEntries(ContentFolderPath).Any())
                return;

            Directory.CreateDirectory(BackupsFolderPath);

            string usableBackupPath = Path.Combine(BackupsFolderPath, $"Backup{Guid.NewGuid()}");
            Directory.CreateDirectory(usableBackupPath);

            string usableContentBackupPath = Path.Combine(usableBackupPath, "Content");

            Directory.Move(ContentFolderPath, usableContentBackupPath);
        }
    }
}
