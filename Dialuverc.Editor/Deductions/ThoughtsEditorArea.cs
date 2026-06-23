using Dialuverc.Deductions;
using Dialuverc.Editor.Base;
using System.Collections.Immutable;
using System.Text.Json;

namespace Dialuverc.Editor.Deductions
{
    public class ThoughtsEditorArea : EditorArea<ThoughtsEditorState>
    {
        ImmutableList<Thought> _thoughts = ImmutableList<Thought>.Empty;
        public IReadOnlyList<Thought> Thoughts => _thoughts;

        // Used to compare Thought references so that selection events are not invoked
        // if no changes were made or we're deselecting twice.
        Thought? _lastSelectedThought = null;

        /// <summary>
        /// Invoked when a <see cref="Thought"/> is selected or deselected.
        /// <para>Deselecting passes a <see langword="null"/> <see cref="Thought"/>.</para>
        /// </summary>
        public event Action<Thought?>? OnThoughtSelectionChanged;

        public Guid AddThought(string nameKey, string descriptionKey, CharacterSides side)
        {
            Guid newGuid = Guid.NewGuid();

            Thought thought = new Thought(newGuid, nameKey, descriptionKey, side);

            BeginChange();

            _thoughts = _thoughts.Add(thought);

            EndChange();

            return newGuid;
        }

        public Guid InsertThought(int index, string nameKey, string descriptionKey, CharacterSides side)
        {
            Guid newGuid = Guid.NewGuid();

            Thought thought = new Thought(newGuid, nameKey, descriptionKey, side);

            BeginChange();

            _thoughts = _thoughts.Insert(index, thought);

            EndChange();

            return newGuid;
        }

        public bool RemoveThought(Guid guid)
        {
            int foundThought = _thoughts.FindIndex(thought => thought.Guid == guid);

            // Don't throw if we remove a thought that doesn't exist
            // since it not existing results in the same state as it being removed.
            if (foundThought < 0)
                return false;

            BeginChange();

            _thoughts = _thoughts.RemoveAt(foundThought);

            EndChange();

            return true;
        }

        public void EditThought(Guid guid, string nameKey, string descriptionKey, CharacterSides side)
        {
            int index = _thoughts.FindIndex(thought => thought.Guid == guid);

            // An edit changes state in a way that failure doesn't match.
            if (index < 0)
                throw new KeyNotFoundException($"No thoughts with id '{guid}'");

            BeginChange();

            _thoughts = _thoughts.SetItem(index, new Thought(guid, nameKey, descriptionKey, side));

            EndChange();
        }

        public void MoveThought(Guid guid, int newIndex)
        {
            int oldIndex = _thoughts.FindIndex(thought => thought.Guid == guid);

            if (oldIndex < 0)
                throw new KeyNotFoundException($"No thoughts with id '{guid}'");

            Thought thoughtToMove = _thoughts[oldIndex];

            BeginChange();

            _thoughts = _thoughts.RemoveAt(oldIndex).Insert(newIndex, thoughtToMove);

            EndChange();
        }

        /// <summary>
        /// Selects the <see cref="Thought"/> corresponding to the given <paramref name="guid"/>.
        /// <para>Pass <see cref="Guid.Empty"/> to deselect.</para>
        /// </summary>
        public void SelectThought(Guid guid)
        {
            Thought? foundThought;

            if (guid == Guid.Empty)
            {
                foundThought = null;
            }
            else
            {
                foundThought = Thoughts.FirstOrDefault(t => t.Guid == guid);

                if (foundThought is null)
                    throw new InvalidOperationException($"No thought with id '{guid}'");
            }

            if (ReferenceEquals(_lastSelectedThought, foundThought))
                return;

            _lastSelectedThought = foundThought;

            OnThoughtSelectionChanged?.Invoke(foundThought);
        }

        #region EditorArea

        protected override ThoughtsEditorState GetStateToSave()
        {
            Guid toSave = Guid.Empty;

            if (_lastSelectedThought is not null)
                toSave = _lastSelectedThought.Guid;

            return new ThoughtsEditorState(_thoughts, toSave);
        }

        protected override bool CheckStateEquality(ThoughtsEditorState a, ThoughtsEditorState b)
        {
            // Selecting a thought is not a state change.
            return a.Thoughts == b.Thoughts;
        }

        protected override void ApplyRestoredState(ThoughtsEditorState newState)
        {
            _thoughts = newState.Thoughts;

            SelectThought(newState.ThoughtSelection);
        }

        public override string SerializeForExport()
        {
            // While we want to use as little space as possible while serializing editor state,
            // we prefer to have exports be as readable as possible.
            return JsonSerializer.Serialize(Thoughts, new JsonSerializerOptions() 
            { 
                WriteIndented = true,
                IncludeFields = true,
            });
        }

        #endregion
    }
}
