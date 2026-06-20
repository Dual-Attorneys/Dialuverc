using Dialuverc.Deductions;
using Dialuverc.Editor.Base;
using System.Text.Json;

namespace Dialuverc.Editor.Deductions
{
    public class ThoughtsEditorArea : EditorArea<byte[]>
    {
        List<Thought> _thoughts = new List<Thought>();
        public IReadOnlyList<Thought> Thoughts => _thoughts;

        public Guid AddThought(string nameKey, string descriptionKey, CharacterSides side)
        {
            Guid newGuid = Guid.NewGuid();

            Thought thought = new Thought(newGuid, nameKey, descriptionKey, side);

            BeginChange();

            _thoughts.Add(thought);

            EndChange();

            return newGuid;
        }

        public Guid InsertThought(int index, string nameKey, string descriptionKey, CharacterSides side)
        {
            Guid newGuid = Guid.NewGuid();

            Thought thought = new Thought(newGuid, nameKey, descriptionKey, side);

            BeginChange();

            _thoughts.Insert(index, thought);

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

            _thoughts.RemoveAt(foundThought);

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

            _thoughts[index] = new Thought(guid, nameKey, descriptionKey, side);

            EndChange();
        }

        public void MoveThought(Guid guid, int newIndex)
        {
            int oldIndex = _thoughts.FindIndex(thought => thought.Guid == guid);

            if (oldIndex < 0)
                throw new KeyNotFoundException($"No thoughts with id '{guid}'");

            Thought thoughtToMove = _thoughts[oldIndex];

            BeginChange();

            _thoughts.RemoveAt(oldIndex);

            _thoughts.Insert(newIndex, thoughtToMove);

            EndChange();
        }

        protected override byte[] GetStateToSave()
        {
            return JsonSerializer.SerializeToUtf8Bytes(_thoughts);
        }

        protected override bool CheckStateEquality(byte[] a, byte[] b)
        {
            return a.SequenceEqual(b);
        }

        protected override void ApplyRestoredState(byte[] newState)
        {
            List<Thought> thoughtsToRestore = JsonSerializer.Deserialize<List<Thought>>(newState)!;

            _thoughts = thoughtsToRestore;
        }

        public override string SerializeForExport()
        {
            // While we want to use as little space as possible while serializing editor state,
            // we prefer to have exports be as readable as possible.
            return JsonSerializer.Serialize(Thoughts, new JsonSerializerOptions() { WriteIndented = true });
        }
    }
}
