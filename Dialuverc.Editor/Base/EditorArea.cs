using Dialuverc.Editor.Base.IO;
using Dialuverc.Editor.Base.Project;
using Dialuverc.Editor.Base.StateTracking;
using Dialuverc.Editor.Base.Verifier;

namespace Dialuverc.Editor.Base
{
    /// <summary>
    /// An area of the editor, similar in concept to standard document editing software.
    /// <para>Allows the user to work on a specific part of the system and provides both local and project-level functionality.</para>
    /// </summary>
    /// <typeparam name="T">The object that represents this area's state.</typeparam>
    // Interface separation here is intentional:
    // the consumer of this should be interested in only one of those at any time as they represent different domains.
    public abstract class EditorArea<T> : StateHistoryTracker<T>, IProjectTrackable, IVerifiable
    {
        T _lastSavedState = default!;

        // Do not check for null here as default state *could* be null.
        public bool HasUnsavedChanges => SavedStates.Count > 0 &&
            !CheckStateEquality(_lastSavedState, SavedStates[CurrentState]);

        protected override void MakeSureBaseStateIsSaved()
        {
            base.MakeSureBaseStateIsSaved();

            SetCurrentStateAsSaved();
        }

        public void ClearStatesHistory()
        {
            _lastSavedState = default!;

            OnClearStatesHistory();

            base.Clear();
        }

        // Since saving is done using IImportables, we do not necessarily know who is doing the saving or when it happens.
        // We let the object doing the saving tell us when changes are safe.
        public void SetCurrentStateAsSaved()
        {
            if (SavedStates.Count == 0)
                return;

            _lastSavedState = SavedStates[CurrentState];
        }

        protected abstract void OnClearStatesHistory();

        public virtual IReadOnlyList<Problem> Verify() { return Array.Empty<Problem>(); }

        public virtual IEnumerable<IExportable> GetGameExportables() { return Array.Empty<IExportable>(); }
        public virtual IEnumerable<IImportable> GetEditorImportables() { return Array.Empty<IImportable>(); }
    }
}
