using Dialuverc.Editor.Base.IO;
using Dialuverc.Editor.Base.Project;
using Dialuverc.Editor.Base.StateTracking;
using Dialuverc.Editor.Base.Verifier;

namespace Dialuverc.Editor.Base
{
    /// <summary>
    /// An implementation of <see cref="IEditorArea"/>.
    /// <para>Note: <typeparamref name="T"/> must be immutable or at least handled like it is.<br/>
    /// This means that state, even if possible to mutate, should not be mutated after being saved.<br/>
    /// This also applies to items inside collections.</para>
    /// </summary>
    /// <typeparam name="T">The object that represents the editor state.</typeparam>
    public abstract class EditorArea<T> : StateHistoryTracker<T>, IProjectTrackable, IVerifiable
    {
        T _lastSavedState = default!;
        public bool HasUnsavedChanges => SavedStates.Count > 0 &&
            !CheckStateEquality(_lastSavedState, SavedStates[CurrentState]);

        protected override void MakeSureBaseStateIsSaved()
        {
            base.MakeSureBaseStateIsSaved();

            SetCurrentStateAsSaved();
        }

        public void ClearStatesHistory()
        {
            base.Clear();

            _lastSavedState = default!;
        }

        // Since saving is done using IImportables, we do not necessarily know who is doing the saving or when it happens.
        // We let the object doing the saving tell us when changes are safe.
        public void SetCurrentStateAsSaved()
        {
            if (SavedStates.Count == 0)
                return;

            _lastSavedState = SavedStates[CurrentState];
        }

        public virtual IReadOnlyList<Problem> Verify() { return Array.Empty<Problem>(); }

        public virtual IEnumerable<IExportable> GetGameExportables() { return Array.Empty<IExportable>(); }
        public virtual IEnumerable<IImportable> GetEditorImportables() { return Array.Empty<IImportable>(); }
    }
}
