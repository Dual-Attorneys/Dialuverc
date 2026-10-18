using Dialuverc.Editor.Base.IO;
using System.Diagnostics.CodeAnalysis;

namespace Dialuverc.Editor.Base.Project
{
    public class DialuvercProject
    {
        public string ProjectFolderPath { get; private set; }
        public string ContentFolderPath { get; private set; }
        public string BackupsFolderPath { get; private set; }

        readonly IEnumerable<IEditorProjectTrackable> _projectTrackables;

        public DialuvercProject(string folderPath, IEnumerable<IEditorProjectTrackable> projectTrackables)
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
            foreach (IEditorProjectTrackable trackable in _projectTrackables)
            {
                foreach (IImportable importable in trackable.GetEditorImportables())
                {
                    yield return importable;
                }
            }
        }

        public bool CheckHasUnsavedChanges()
        {
            foreach (IEditorProjectTrackable trackable in _projectTrackables)
            {
                if (trackable.HasUnsavedChanges)
                    return true;
            }

            return false;
        }

        // TODO: Proper error handling.
        public void SaveProject()
        {
            string? tempContentFolderPath = null;

            string? pathUsedForBackup = null;

            try
            {
                Directory.CreateDirectory(ProjectFolderPath);

                tempContentFolderPath = Path.Combine(ProjectFolderPath, $"TempContent{Guid.NewGuid()}");

                pathUsedForBackup = MoveCurrentSaveToBackups();

                ExportableUtils.ExportToFolder(EnumerateImportables(), tempContentFolderPath);

                // If Content exists, it's empty (MoveCurrentSaveToBackups would have moved it if it had anything inside).
                if (Directory.Exists(ContentFolderPath))
                    Directory.Delete(ContentFolderPath);

                Directory.Move(tempContentFolderPath, ContentFolderPath);

                foreach (IEditorProjectTrackable trackable in _projectTrackables)
                {
                    trackable.SetCurrentStateAsSaved();
                }
            }
            catch
            {
                if (Directory.Exists(tempContentFolderPath))
                    Directory.Delete(tempContentFolderPath, true);

                if (Directory.Exists(pathUsedForBackup))
                    Directory.Move(pathUsedForBackup, ContentFolderPath);

                // Surface any exceptions thrown during saving.
                throw;
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

        public void LoadProject()
        {
            ExportableUtils.ImportFromFolder(EnumerateImportables(), ContentFolderPath);

            foreach (IEditorProjectTrackable trackable in _projectTrackables)
            {
                trackable.ClearStatesHistory();
            }
        }
    }
}
