using Dialuverc.Editor.Base;
using DualAttorneys.Dialuverc.Deductions;
using System.Collections.Immutable;

namespace DualAttorneys.Dialuverc.Editor.Deductions
{
    /// <summary>
    /// This <see cref="EditorArea{T}"/> allows progressive creation and editing of <see cref="EditorDeduction"/>s.
    /// <para>
    /// Creation and editing happen (possibly at the same time) respectively in <see cref="Mode.Add"/> and <see cref="Mode.Edit"/> mode.<br/>
    /// Each mode uses its own builder (with undo/redo support).
    /// </para>
    /// </summary>
    /// <remarks>The UI is responsible for appropriately initializing and updating based on this.</remarks>
    public class DeductionsEditorArea : EditorArea<DeductionsEditorState>
    {
        // Add is currently assumed to be the default mode.
        public Mode CurrentMode { get; private set; } = Mode.Add;

        /// <summary>
        /// Builder to be used for Add/Insert operations (new and possibly empty deductions).
        /// </summary>
        EditorDeduction _addingBuilder = CreateDefaultDeduction();

        /// <summary>
        /// Builder to be used for any operation that modifies deductions that already exist in the list.
        /// </summary>
        EditorDeduction? _editingBuilder;

        Guid? _selectionGuid;

        /// <summary>
        /// The appropriate deduction builder based on the <see cref="CurrentMode"/>.<br/>
        /// Guaranteed to never be <see langword="null"/>.
        /// </summary>
        EditorDeduction _activeBuilder
        {
            get
            {
                switch (CurrentMode)
                {
                    case Mode.Add:
                        return _addingBuilder;

                    case Mode.Edit:

                        if (_editingBuilder is null)
                            throw new InvalidOperationException($"Can't get {nameof(_activeBuilder)} in {Mode.Edit} mode with null {nameof(_editingBuilder)}");

                        return _editingBuilder;

                    default: throw new NotImplementedException();
                }
            }

            set
            {
                switch (CurrentMode)
                {
                    case Mode.Add:

                        _addingBuilder = value!;
                        break;

                    case Mode.Edit:

                        _editingBuilder = value;
                        break;

                    default: throw new NotImplementedException();
                }
            }
        }

        ImmutableList<EditorDeduction> _deductions = ImmutableList<EditorDeduction>.Empty;
        public IReadOnlyList<EditorDeduction> Deductions => _deductions;

        /// <summary>
        /// Invoked when an <see cref="EditorDeduction"/> is selected or deselected.
        /// <para>Deselecting passes a <see langword="null"/> <see cref="EditorDeduction"/>.</para>
        /// </summary>
        public event Action<EditorDeduction?>? OnDeductionSelectionChanged;

        public event Action<Mode>? OnModeChanged;

        /// <summary>
        /// Sets which deduction builder to use based on the passed <paramref name="newMode"/>.<br/>
        /// All changes happen on the currently active builder.
        /// <para></para>
        /// </summary>
        public void ChangeMode(Mode newMode)
        {
            if (newMode == CurrentMode)
                return;

            CurrentMode = newMode;

            OnModeChanged?.Invoke(CurrentMode);
        }

        /// <summary>
        /// Selects the <see cref="EditorDeduction"/> corresponding to the given <paramref name="guid"/>.
        /// <para>Pass <see langword="null"/> to deselect.</para>
        /// </summary>
        public void SelectDeduction(Guid? guid)
        {
            if (_selectionGuid == guid)
                return;

            EditorDeduction? foundDeduction;

            if (!guid.HasValue)
            {
                // TODO: Maybe make deselecting keep the current mode instead of changing to Edit?
                foundDeduction = null;
            }
            else
            {
                foundDeduction = _deductions.FirstOrDefault(t => t.Guid == guid);

                if (foundDeduction is null)
                    throw new InvalidOperationException($"No deduction with id '{guid}'");
            }

            _editingBuilder = foundDeduction;
            _selectionGuid = guid;

            OnDeductionSelectionChanged?.Invoke(foundDeduction);

            ChangeMode(Mode.Edit);
        }

        public void SetAlias(string alias)
        {
            BeginChange();

            _activeBuilder = _activeBuilder with { Alias = alias };

            EndChange();
        }

        public void SetEditorNote(string editorNote)
        {
            BeginChange();

            _activeBuilder = _activeBuilder with { EditorNote = editorNote };

            EndChange();
        }

        public void AddOutputThought(ThoughtGuid guid)
        {
            // TODO: Maybe check for duplicates?

            BeginChange();

            _activeBuilder = _activeBuilder with
            {
                Outputs = _activeBuilder.Outputs with
                {
                    Thoughts = _activeBuilder.Outputs.Thoughts.Add(guid)
                }
            };

            EndChange();
        }

        public void RemoveOutputThought(ThoughtGuid guid)
        {
            ImmutableArray<ThoughtGuid> newArray = _activeBuilder.Outputs.Thoughts.Remove(guid);

            // Don't save history if we try removing a non-existing thought.
            // Failure results in the same state as success anyway.
            if (_activeBuilder.Outputs.Thoughts == newArray)
                return;

            BeginChange();

            _activeBuilder = _activeBuilder with
            {
                Outputs = _activeBuilder.Outputs with
                {
                    Thoughts = newArray
                }
            };

            EndChange();
        }

        /// <summary>
        /// Applies all changes done so far on the active builder to the <see cref="Deductions"/> list.
        /// <para>If <see cref="CurrentMode"/> is <see cref="Mode.Add"/>, its builder is also reset.</para>
        /// </summary>
        public Guid FinishBuilding()
        {
            if (CurrentMode == Mode.Edit)
            {
                if (_editingBuilder is null)
                    throw new InvalidOperationException($"Can't call {nameof(FinishBuilding)} in {Mode.Edit} mode with null {_editingBuilder}");

                int index = _deductions.FindIndex(d => d.Guid == _editingBuilder.Guid);

                if (index < 0)
                    throw new InvalidOperationException($"No deduction with id '{_editingBuilder.Guid}'");

                BeginChange();

                _deductions = _deductions.SetItem(index, _editingBuilder!);

                EndChange();

                return _editingBuilder.Guid;
            }

            Guid addedGuid = _addingBuilder.Guid;

            BeginChange();

            _deductions = _deductions.Add(_addingBuilder);

            _addingBuilder = CreateDefaultDeduction();

            EndChange();

            return addedGuid;
        }

        public bool RemoveDeduction(Guid guid)
        {
            int index = _deductions.FindIndex(d => d.Guid == guid);

            if (index < 0)
                return false;

            BeginChange();

            _deductions = _deductions.RemoveAt(index);

            if (guid == _selectionGuid)
                SelectDeduction(null);

            EndChange();

            return true;
        }

        static EditorDeduction CreateDefaultDeduction() => new EditorDeduction(
            Guid.NewGuid(),
            new ThoughtCombination(null, null, null),
            new DeductionOutputs());

        #region EditorArea

        protected override DeductionsEditorState GetStateToSave() 
            => new DeductionsEditorState(_deductions, _addingBuilder, _editingBuilder, CurrentMode, _selectionGuid);

        // Changing mode or selecting a deduction are not state changes by themselves.
        protected override bool CheckStateEquality(DeductionsEditorState a, DeductionsEditorState b) 
            => a.Deductions == b.Deductions && 
            a.AddBuilder == b.AddBuilder && 
            a.EditBuilder == b.EditBuilder;

        protected override void ApplyRestoredState(DeductionsEditorState newState)
        {
            _deductions = newState.Deductions;
            _addingBuilder = newState.AddBuilder;
            _editingBuilder = newState.EditBuilder;
            
            SelectDeduction(newState.DeductionSelection);

            // Currently, selecting always changes mode to Edit.
            // In case the previous mode was Add, change it again to Add.
            // Not the happiest with this solution since ChangeMode is called twice.
            ChangeMode(newState.Mode);
        }

        public override string SerializeForExport()
        {
            throw new NotImplementedException();
        }

        #endregion

        public enum Mode
        {
            Add,
            Edit
        }
    }
}
