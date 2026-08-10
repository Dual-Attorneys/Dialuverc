using Dialuverc.Editor.Base;
using DualAttorneys.Dialuverc.Deductions;
using System.Collections.Immutable;

namespace DualAttorneys.Dialuverc.Editor.Deductions
{
    /// <summary>
    /// This <see cref="EditorArea{T}"/> allows progressive creation and editing of <see cref="EditorDeduction"/>s.
    /// </summary>
    public class DeductionsEditorArea : ModeEditorArea<EditorDeduction, DeductionsEditorState>
    {
        Guid? _selectionGuid;

        ImmutableList<EditorDeduction> _deductions = ImmutableList<EditorDeduction>.Empty;
        public IReadOnlyList<EditorDeduction> Deductions => _deductions;

        /// <summary>
        /// Invoked when an <see cref="EditorDeduction"/> is selected or deselected.
        /// <para>Deselecting passes a <see langword="null"/> <see cref="EditorDeduction"/>.</para>
        /// </summary>
        public event Action<EditorDeduction?>? OnDeductionSelectionChanged;

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

            ActiveBuilder = ActiveBuilder with { Alias = alias };

            EndChange();
        }

        public void SetEditorNote(string editorNote)
        {
            BeginChange();

            ActiveBuilder = ActiveBuilder with { EditorNote = editorNote };

            EndChange();
        }

        public void AddOutputThought(ThoughtGuid guid)
        {
            // TODO: Maybe check for duplicates?

            BeginChange();

            ActiveBuilder = ActiveBuilder with
            {
                Outputs = ActiveBuilder.Outputs with
                {
                    Thoughts = ActiveBuilder.Outputs.Thoughts.Add(guid)
                }
            };

            EndChange();
        }

        public void RemoveOutputThought(ThoughtGuid guid)
        {
            ImmutableArray<ThoughtGuid> newArray = ActiveBuilder.Outputs.Thoughts.Remove(guid);

            // Don't save history if we try removing a non-existing thought.
            // Failure results in the same state as success anyway.
            if (ActiveBuilder.Outputs.Thoughts == newArray)
                return;

            BeginChange();

            ActiveBuilder = ActiveBuilder with
            {
                Outputs = ActiveBuilder.Outputs with
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

            _addingBuilder = CreateDefaultBuilder();

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

        protected override EditorDeduction CreateDefaultBuilder() => new EditorDeduction(
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

            // If a state was saved at all, something has changed and the UI needs to update.
            // SelectChange would not invoke the event in cases where Guids are equal.
            _selectionGuid = newState.DeductionSelection;
            OnDeductionSelectionChanged?.Invoke(newState.EditBuilder);

            ChangeMode(newState.Mode);
        }

        public override string SerializeForExport()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
