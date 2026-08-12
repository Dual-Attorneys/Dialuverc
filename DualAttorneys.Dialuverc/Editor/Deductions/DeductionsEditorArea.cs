using Dialuverc.Editor.Base;
using Dialuverc.Editor.Base.Modes;
using DualAttorneys.Dialuverc.Deductions;
using System.Collections.Immutable;

using static Dialuverc.Editor.Base.Modes.EditorModeManager;

namespace DualAttorneys.Dialuverc.Editor.Deductions
{
    /// <summary>
    /// This <see cref="EditorArea{T}"/> allows progressive creation and editing of <see cref="EditorDeduction"/>s.
    /// </summary>
    public class DeductionsEditorArea : EditorArea<DeductionsEditorState>
    {
        Guid? _selectionGuid;

        ImmutableList<EditorDeduction> _deductions = ImmutableList<EditorDeduction>.Empty;
        public IReadOnlyList<EditorDeduction> Deductions => _deductions;

        /// <summary>
        /// Invoked when an <see cref="EditorDeduction"/> is selected or deselected.
        /// <para>Deselecting passes a <see langword="null"/> <see cref="EditorDeduction"/>.</para>
        /// </summary>
        public event Action<EditorDeduction?>? OnDeductionSelectionChanged;

        readonly EditorScratchpadManager<EditorDeduction> _scratchpadManager;
        public EditorModeManager ScratchpadManager => _scratchpadManager;

        public DeductionsEditorArea()
        {
            _scratchpadManager = new EditorScratchpadManager<EditorDeduction>();

            _scratchpadManager.AddBuilder = CreateDefaultEditorDeduction();
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

            _scratchpadManager.EditBuilder = foundDeduction;
            _selectionGuid = guid;

            OnDeductionSelectionChanged?.Invoke(foundDeduction);

            _scratchpadManager.ChangeMode(Mode.Edit);
        }

        public void SetAlias(string alias)
        {
            BeginChange();

            _scratchpadManager.ActiveBuilder = _scratchpadManager.ActiveBuilder with { Alias = alias };

            EndChange();
        }

        public void SetEditorNote(string editorNote)
        {
            BeginChange();

            _scratchpadManager.ActiveBuilder = _scratchpadManager.ActiveBuilder with { EditorNote = editorNote };

            EndChange();
        }

        public void AddOutputThought(ThoughtGuid guid)
        {
            // TODO: Maybe check for duplicates?

            BeginChange();

            _scratchpadManager.ActiveBuilder = _scratchpadManager.ActiveBuilder with
            {
                Outputs = _scratchpadManager.ActiveBuilder.Outputs with
                {
                    Thoughts = _scratchpadManager.ActiveBuilder.Outputs.Thoughts.Add(guid)
                }
            };

            EndChange();
        }

        public void RemoveOutputThought(ThoughtGuid guid)
        {
            ImmutableArray<ThoughtGuid> newArray = _scratchpadManager.ActiveBuilder.Outputs.Thoughts.Remove(guid);

            // Don't save history if we try removing a non-existing thought.
            // Failure results in the same state as success anyway.
            if (_scratchpadManager.ActiveBuilder.Outputs.Thoughts == newArray)
                return;

            BeginChange();

            _scratchpadManager.ActiveBuilder = _scratchpadManager.ActiveBuilder with
            {
                Outputs = _scratchpadManager.ActiveBuilder.Outputs with
                {
                    Thoughts = newArray
                }
            };

            EndChange();
        }

        /// <summary>
        /// Applies all changes done so far on the active builder to the <see cref="Deductions"/> list.
        /// <para>If <see cref="EditorModeManager.CurrentMode"/> is <see cref="Mode.Add"/>, its builder is also reset.</para>
        /// </summary>
        public Guid FinishBuilding()
        {
            if (_scratchpadManager.CurrentMode == Mode.Edit)
            {
                if (_scratchpadManager.EditBuilder is null)
                    throw new InvalidOperationException($"Can't call {nameof(FinishBuilding)} in {Mode.Edit} mode with null {_scratchpadManager.EditBuilder}");

                int index = _deductions.FindIndex(d => d.Guid == _scratchpadManager.EditBuilder.Guid);

                if (index < 0)
                    throw new InvalidOperationException($"No deduction with id '{_scratchpadManager.EditBuilder.Guid}'");

                BeginChange();

                _deductions = _deductions.SetItem(index, _scratchpadManager.EditBuilder!);

                EndChange();

                return _scratchpadManager.EditBuilder.Guid;
            }

            Guid addedGuid = _scratchpadManager.AddBuilder.Guid;

            BeginChange();

            _deductions = _deductions.Add(_scratchpadManager.AddBuilder);

            _scratchpadManager.AddBuilder = CreateDefaultEditorDeduction();

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

        EditorDeduction CreateDefaultEditorDeduction() => new EditorDeduction(
            Guid.NewGuid(),
            new ThoughtCombination(null, null, null),
            new DeductionOutputs());

        #region EditorArea

        protected override DeductionsEditorState GetStateToSave() 
            => new DeductionsEditorState(_deductions, _scratchpadManager.AddBuilder, _scratchpadManager.EditBuilder, _scratchpadManager.CurrentMode, _selectionGuid);

        // Changing mode or selecting a deduction are not state changes by themselves.
        protected override bool CheckStateEquality(DeductionsEditorState a, DeductionsEditorState b) 
            => a.Deductions == b.Deductions && 
            a.AddBuilder == b.AddBuilder && 
            a.EditBuilder == b.EditBuilder;

        protected override void ApplyRestoredState(DeductionsEditorState newState)
        {
            _deductions = newState.Deductions;
            _scratchpadManager.AddBuilder = newState.AddBuilder;
            _scratchpadManager.EditBuilder = newState.EditBuilder;

            // If a state was saved at all, something has changed and the UI needs to update.
            // SelectDeduction would not invoke the event in cases where Guids are equal.
            _selectionGuid = newState.DeductionSelection;
            OnDeductionSelectionChanged?.Invoke(newState.EditBuilder);

            _scratchpadManager.ChangeMode(newState.Mode);
        }

        public override string SerializeForExport()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
