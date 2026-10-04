namespace Dialuverc.Editor.Base.StateTracking
{
    /// <summary>
    /// An abstract component which keeps track of up to <see cref="MaxStates"/> states of type <typeparamref name="T"/>,<br/>
    /// and provides undo/redo capabilities.
    /// <para><b>Note</b>: <typeparamref name="T"/> should be either immutable or handled like it is (applies to collections as well)!</para>
    /// </summary>
    /// <typeparam name="T">The type of state to track.</typeparam>
    // Heavily inspired by osu-lazer's EditorChangeHandler.
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

        /// <summary>
        /// Runs on the very first transaction when there's no history.<br/>
        /// Makes sure a base state to undo towards exists.
        /// </summary>
        // Making this virtual, allows inheriting classes to operate on the base state.
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

            OnStateChanged?.Invoke();
        }
    }
}
