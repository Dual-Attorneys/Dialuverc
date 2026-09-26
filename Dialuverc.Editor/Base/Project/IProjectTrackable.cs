using Dialuverc.Editor.Base.IO;

namespace Dialuverc.Editor.Base.Project
{
    /// <summary>
    /// Represents an object on which project-level state history and persistence-related operations can be done.
    /// </summary>
    public interface IProjectTrackable
    {
        /// <summary>
        /// Whether this object has unsaved changes, making destructive operations dangerous.
        /// </summary>
        public bool HasUnsavedChanges { get; }

        public void ClearStatesHistory();

        /// <summary>
        /// Returns all <see cref="IExportable"/>s whose target is a game.
        /// </summary>
        public IEnumerable<IExportable> GetGameExportables();

        /// <summary>
        /// Returns all <see cref="IImportable"/>s usable to save and load work done in the editor.
        /// </summary>
        public IEnumerable<IImportable> GetEditorImportables();

        /// <summary>
        /// Sets this object's changes (since last save) as saved, making destructive operations safe.
        /// </summary>
        public void SetCurrentStateAsSaved();
    }
}
