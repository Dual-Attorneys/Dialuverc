using Dialuverc.Editor.Base;
using Dialuverc.Editor.Base.Modes;
using Dialuverc.Editor.Base.Verifier;
using DualAttorneys.Dialuverc.Deductions;
using System.Collections.Immutable;
using System.Text.Json;

using static Dialuverc.Editor.Base.Modes.EditorModeManager;

namespace DualAttorneys.Dialuverc.Editor.Deductions
{
    public class ThoughtsEditorArea : EditorArea<ThoughtsEditorState>
    {
        ImmutableList<EditorThought> _thoughts = ImmutableList<EditorThought>.Empty;
        public IReadOnlyList<EditorThought> Thoughts => _thoughts;

        ThoughtGuid? _selectionGuid = null;

        /// <summary>
        /// Invoked when a <see cref="EditorThought"/> is selected or deselected while already in <see cref="Mode.Edit"/> mode.
        /// <para>Deselecting passes a <see langword="null"/> <see cref="EditorThought"/>.</para>
        /// </summary>
        public event Action<EditorThought?>? OnThoughtSelectionChanged;

        readonly EditorScratchpadManager<EditorThought> _scratchpadManager;
        public EditorModeManager ScratchpadManager => _scratchpadManager;
        public EditorThought? ActiveScratchpad => _scratchpadManager.ActiveScratchpad;

        // Note: While we are using records for EditorThoughts, we'll keep (runtime) Thoughts readonly.
        // This assumes their structure is unlikely to change and will always need few parameters.

        public override string ExportName => nameof(ThoughtsEditorArea);

        public ThoughtsEditorArea()
        {
            _scratchpadManager = new EditorScratchpadManager<EditorThought>();

            _scratchpadManager.AddScratchpad = CreateDefaultEditorThought();
        }

        public void SetNameKey(string nameKey)
        {
            Thought currentRuntimeThought = _scratchpadManager.ActiveScratchpad!.RuntimeThought;

            BeginChange();

            _scratchpadManager.ActiveScratchpad = _scratchpadManager.ActiveScratchpad with
            {
                RuntimeThought = new Thought(
                    currentRuntimeThought.Guid,
                    nameKey,
                    currentRuntimeThought.DescriptionKey,
                    currentRuntimeThought.Side)
            };

            EndChange();
        }

        public void SetDescriptionKey(string descriptionKey)
        {
            Thought currentRuntimeThought = _scratchpadManager.ActiveScratchpad!.RuntimeThought;

            BeginChange();

            _scratchpadManager.ActiveScratchpad = _scratchpadManager.ActiveScratchpad with
            {
                RuntimeThought = new Thought(
                    currentRuntimeThought.Guid,
                    currentRuntimeThought.NameKey,
                    descriptionKey,
                    currentRuntimeThought.Side)
            };

            EndChange();
        }

        public void SetSide(CharacterSides side)
        {
            Thought currentRuntimeThought = _scratchpadManager.ActiveScratchpad!.RuntimeThought;

            BeginChange();

            _scratchpadManager.ActiveScratchpad = _scratchpadManager.ActiveScratchpad with
            {
                RuntimeThought = new Thought(
                    currentRuntimeThought.Guid,
                    currentRuntimeThought.NameKey,
                    currentRuntimeThought.DescriptionKey,
                    side)
            };

            EndChange();
        }

        public void SetEditorNote(string editorNote)
        {
            BeginChange();

            _scratchpadManager.ActiveScratchpad = _scratchpadManager.ActiveScratchpad! with { EditorNote = editorNote };

            EndChange();
        }

        /// <summary>
        /// Applies all changes done so far on the active builder to the <see cref="Thoughts"/> list.
        /// <para>If <see cref="EditorModeManager.CurrentMode"/> is <see cref="Mode.Add"/>, its builder is also reset.</para>
        /// </summary>
        public ThoughtGuid FinishBuilding()
        {
            if (_scratchpadManager.CurrentMode == Mode.Edit)
            {
                if (_scratchpadManager.EditScratchpad is null)
                    throw new InvalidOperationException($"Can't call {nameof(FinishBuilding)} in {Mode.Edit} mode with null {_scratchpadManager.EditScratchpad}");

                int index = _thoughts.FindIndex(d => d.RuntimeThought.Guid == _scratchpadManager.EditScratchpad.RuntimeThought.Guid);

                if (index < 0)
                    throw new InvalidOperationException($"No thought with id '{_scratchpadManager.EditScratchpad.RuntimeThought.Guid}'");

                BeginChange();

                _thoughts = _thoughts.SetItem(index, _scratchpadManager.EditScratchpad!);

                EndChange();

                return _scratchpadManager.EditScratchpad.RuntimeThought.Guid;
            }

            if (_scratchpadManager.AddScratchpad is null)
                throw new InvalidOperationException($"Can't call {nameof(FinishBuilding)} in {Mode.Add} mode with null {_scratchpadManager.AddScratchpad}");

            ThoughtGuid addedGuid = _scratchpadManager.AddScratchpad.RuntimeThought.Guid;

            BeginChange();

            _thoughts = _thoughts.Add(_scratchpadManager.AddScratchpad);

            _scratchpadManager.AddScratchpad = CreateDefaultEditorThought();

            EndChange();

            return addedGuid;
        }

        public bool RemoveThought(ThoughtGuid guid)
        {
            int foundThought = _thoughts.FindIndex(thought => thought.RuntimeThought.Guid == guid);
            
            // Don't throw if we remove a thought that doesn't exist
            // since it not existing results in the same state as it being removed.
            if (foundThought < 0)
                return false;

            BeginChange();

            _thoughts = _thoughts.RemoveAt(foundThought);

            if (guid == _selectionGuid)
                SelectThought(null);

            EndChange();

            return true;
        }

        public void MoveThought(ThoughtGuid guid, int newIndex)
        {
            int oldIndex = _thoughts.FindIndex(thought => thought.RuntimeThought.Guid == guid);

            if (oldIndex < 0)
                throw new KeyNotFoundException($"No thoughts with id '{guid}'");

            if (oldIndex == newIndex)
                return;

            EditorThought thoughtToMove = _thoughts[oldIndex];

            BeginChange();

            _thoughts = _thoughts.RemoveAt(oldIndex).Insert(newIndex, thoughtToMove);

            EndChange();
        }

        /// <summary>
        /// Selects the <see cref="Thought"/> corresponding to the given <paramref name="guid"/>.
        /// <para>Pass <see langword="null"/> to deselect.</para>
        /// </summary>
        public void SelectThought(ThoughtGuid? guid)
        {
            if (_selectionGuid == guid)
                return;

            EditorThought? foundThought;

            if (!guid.HasValue)
            {
                foundThought = null;
            }
            else
            {
                foundThought = Thoughts.FirstOrDefault(t => t.RuntimeThought.Guid == guid);

                if (foundThought is null)
                    throw new InvalidOperationException($"No thought with id '{guid}'");
            }

            _scratchpadManager.EditScratchpad = foundThought;
            _selectionGuid = guid;

            if (!_scratchpadManager.ChangeMode(Mode.Edit))
                OnThoughtSelectionChanged?.Invoke(foundThought);
        }

        EditorThought CreateDefaultEditorThought() => new EditorThought(new Thought(
            new ThoughtGuid(),
            string.Empty,
            string.Empty,
            CharacterSides.Any));

        #region EditorArea

        protected override ThoughtsEditorState GetStateToSave()
        {
            return new ThoughtsEditorState(_thoughts, _scratchpadManager.AddScratchpad, _scratchpadManager.EditScratchpad, _scratchpadManager.CurrentMode, _selectionGuid);
        }

        protected override bool CheckStateEquality(ThoughtsEditorState a, ThoughtsEditorState b)
        {
            // Selecting a thought or changing modes is not a state change.
            return a.Thoughts == b.Thoughts &&
                a.AddBuilder == b.AddBuilder &&
                a.EditBuilder == b.EditBuilder;
        }

        protected override void ApplyRestoredState(ThoughtsEditorState newState)
        {
            _thoughts = newState.Thoughts;
            _scratchpadManager.AddScratchpad = newState.AddBuilder;
            _scratchpadManager.EditScratchpad = newState.EditBuilder;
            _selectionGuid = newState.ThoughtSelection;

            _scratchpadManager.ChangeMode(newState.Mode, invokeEvent: false);
        }

        public override string SerializeForExport()
        {
            // While we want to use as little space as possible while serializing editor state,
            // we prefer to have exports be as readable as possible.
            return JsonSerializer.Serialize(Thoughts.Select(et => et.RuntimeThought), new JsonSerializerOptions()
            {
                WriteIndented = true,
                IncludeFields = true,
            });
        }

        public override IReadOnlyList<Problem> Verify() => ThoughtsEditorVerifier.Run(Thoughts);

        #endregion
    }
}
