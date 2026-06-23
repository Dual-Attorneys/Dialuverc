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
            if (_savedStates.Count == 0)
                ForceCommit();
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
        }

        protected abstract T GetStateToSave();

        // Since we don't know what T is,
        // force whoever is writing the code to think about how equality between states is determined.
        protected abstract bool CheckStateEquality(T a, T b);

        protected abstract void ApplyRestoredState(T newState);

        public abstract string SerializeForExport();

        #region Testing

        /// <summary>
        /// Moves <see cref="_currentState"/> to the latest saved state WITHOUT applying it.
        /// </summary>
        protected void PointToLatestState() => _currentState = _savedStates.Count - 1;

        protected IReadOnlyList<T> SavedStates => _savedStates;

        #endregion
    }
}
