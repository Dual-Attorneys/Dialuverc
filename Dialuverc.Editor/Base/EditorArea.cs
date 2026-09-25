using Dialuverc.Editor.Base.IO;
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
    public abstract class EditorArea<T> : TransactionalObject, IEditorArea
    {
        protected virtual int MaxStates => 50;

        readonly List<T> _savedStates = new List<T>();

        int _currentState;

        bool _isRestoringPreviousState;

        public bool CanUndo => _savedStates.Count > 0 && _currentState > 0;
        public bool CanRedo => _currentState < _savedStates.Count - 1;

        T _lastSavedState = default(T)!;
        public bool HasUnsavedChanges => _savedStates.Count > 0 &&
            _lastSavedState is not null &&
            !CheckStateEquality(_lastSavedState, _savedStates[_currentState]);

        /// <summary>
        /// Invoked when state is saved or restored.
        /// </summary>
        public event Action? OnStateChanged;

        /// <summary>
        /// Begins a transaction and makes sure a base state to undo towards exists.
        /// </summary>
        public override void BeginChange()
        {
            MakeSureBaseStateIsSaved();

            base.BeginChange();
        }

        void MakeSureBaseStateIsSaved()
        {
            if (_savedStates.Count != 0)
                return;

            ForceCommit();

            // When MakeSureBaseStateIsSaved runs, we're either on an empty EditorArea (which means there's nothing to save),
            // or we've just restored state for the first time from an external source in some way.
            // In the last case, we can assume state comes from a persistent storage, meaning it is already "saved".
            SetCurrentStateAsSaved();
        }

        protected override void Commit()
        {
            if (_isRestoringPreviousState)
                return;

            T stateToSave = GetStateToSave();

            if (stateToSave is null)
                throw new InvalidOperationException($"Can't save a null state");

            if (_savedStates.Count > 0 && CheckStateEquality(_savedStates[_currentState], stateToSave))
                return;

            if (_currentState < _savedStates.Count - 1)
                _savedStates.RemoveRange(_currentState + 1, _savedStates.Count - _currentState - 1);

            if (_savedStates.Count >= MaxStates)
                _savedStates.RemoveAt(0);

            _savedStates.Add(stateToSave);

            _currentState = _savedStates.Count - 1;

            // This will get called twice on the first commit (MakeSureBaseStateIsSaved forces a Commit).
            OnStateChanged?.Invoke();
        }

        public void RestorePreviousState(RestoreDirection direction)
        {
            if (TransactionPending)
                return;

            if (_savedStates.Count == 0)
                return;

            int index = _currentState + (int)direction;

            if (index < 0 || index >= _savedStates.Count)
                return;

            _isRestoringPreviousState = true;

            ApplyRestoredState(_savedStates[index]);

            _currentState = index;

            _isRestoringPreviousState = false;

            OnStateChanged?.Invoke();
        }

        protected abstract T GetStateToSave();

        // Since we don't know what T is,
        // force whoever is writing the code to think about how equality between states is determined.
        protected abstract bool CheckStateEquality(T a, T b);

        protected abstract void ApplyRestoredState(T newState);

        // Since saving is done using IImportables, we do not necessarily know who is doing the saving or when it happens.
        // We let the object doing the saving tell us when changes are safe.
        public void SetCurrentStateAsSaved()
        {
            if (_savedStates.Count == 0)
                return;

            _lastSavedState = _savedStates[_currentState];
        }

        public virtual IReadOnlyList<Problem> Verify() { return Array.Empty<Problem>(); }

        public virtual IEnumerable<IExportable> GetGameExportables() { return Array.Empty<IExportable>(); }
        public virtual IEnumerable<IImportable> GetEditorImportables() { return Array.Empty<IImportable>(); }

        #region Testing

        protected int CurrentStateIndex => _currentState;

        protected IReadOnlyList<T> SavedStates => _savedStates;

        #endregion
    }
}
