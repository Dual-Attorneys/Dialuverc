namespace Dialuverc.Editor.Base.StateTracking
{
    public abstract class StateHistoryTracker<T> : TransactionalObject, IStateHistoryTracker
    {
        protected virtual int MaxStates => 50;

        readonly List<T> _savedStates = new List<T>();
        protected IReadOnlyList<T> SavedStates => _savedStates;

        protected int CurrentState { get; private set; }

        protected bool IsRestoringPreviousState { get; private set; }

        public bool CanUndo => _savedStates.Count > 0 && CurrentState > 0;
        public bool CanRedo => CurrentState < _savedStates.Count - 1;

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

        protected virtual void MakeSureBaseStateIsSaved()
        {
            if (_savedStates.Count != 0)
                return;

            ForceCommit();
        }

        protected override void Commit()
        {
            if (IsRestoringPreviousState)
                return;

            T stateToSave = GetStateToSave();

            if (stateToSave is null)
                throw new InvalidOperationException($"Can't save a null state");

            if (_savedStates.Count > 0 && CheckStateEquality(_savedStates[CurrentState], stateToSave))
                return;

            if (CurrentState < _savedStates.Count - 1)
                _savedStates.RemoveRange(CurrentState + 1, _savedStates.Count - CurrentState - 1);

            if (_savedStates.Count >= MaxStates)
                _savedStates.RemoveAt(0);

            _savedStates.Add(stateToSave);

            CurrentState = _savedStates.Count - 1;

            // This will get called twice on the first commit (MakeSureBaseStateIsSaved forces a Commit).
            OnStateChanged?.Invoke();
        }

        public void RestorePreviousState(RestoreDirection direction)
        {
            if (TransactionPending)
                return;

            if (_savedStates.Count == 0)
                return;

            int index = CurrentState + (int)direction;

            if (index < 0 || index >= _savedStates.Count)
                return;

            IsRestoringPreviousState = true;

            ApplyRestoredState(_savedStates[index]);

            CurrentState = index;

            IsRestoringPreviousState = false;

            OnStateChanged?.Invoke();
        }

        protected abstract T GetStateToSave();

        // Since we don't know what T is,
        // force whoever is writing the code to think about how equality between states is determined.
        protected abstract bool CheckStateEquality(T a, T b);

        protected abstract void ApplyRestoredState(T newState);

        protected void Clear()
        {
            if (IsRestoringPreviousState || TransactionPending)
                return;

            _savedStates.Clear();

            CurrentState = default;
        }
    }
}
