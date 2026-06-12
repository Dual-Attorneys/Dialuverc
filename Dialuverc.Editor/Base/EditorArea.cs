namespace Dialuverc.Editor.Base
{
    /// <summary>
    /// Represents one of multiple editor areas, each allowing to work on a specific part of the narrative system.
    /// <para>Provides basic undo/redo functionality.</para>
    /// </summary>
    public abstract class EditorArea : TransactionalObject
    {
        protected virtual int MaxStates => 50;

        List<string> _savedStates = new List<string>();

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

            string text = SerializeCurrentEditorState();

            if (_savedStates.Count > 0 && _savedStates[_currentState] == text)
                return;

            if (_currentState < _savedStates.Count - 1)
                _savedStates.RemoveRange(_currentState + 1, _savedStates.Count - _currentState - 1);

            if (_savedStates.Count >= MaxStates)
                _savedStates.RemoveAt(0);

            _savedStates.Add(text);

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

            ApplyEditorState(_savedStates[_currentState], _savedStates[index]);

            _currentState = index;

            _isRestoringPreviousState = false;
        }

        protected abstract string SerializeCurrentEditorState();

        // Pass both states. In case we'll need it in the future. Costs nothing anyway.
        protected abstract void ApplyEditorState(string previousState, string newState);

        public abstract string SerializeForExport();

        public enum RestoreDirection
        {
            Previous = -1,
            Next = 1
        }

        #region Testing

        /// <summary>
        /// Moves <see cref="_currentState"/> to the latest saved state WITHOUT applying it.
        /// </summary>
        protected void PointToLatestState() => _currentState = _savedStates.Count - 1;

        protected IReadOnlyList<string> SavedStates => _savedStates;

        #endregion
    }
}
