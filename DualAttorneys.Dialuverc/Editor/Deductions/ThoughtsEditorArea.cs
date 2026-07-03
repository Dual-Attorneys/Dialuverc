using Dialuverc.Editor.Base;
using Dialuverc.Editor.Base.Verifier;
using DualAttorneys.Dialuverc.Deductions;
using System.Collections.Immutable;
using System.Text.Json;

namespace DualAttorneys.Dialuverc.Editor.Deductions
{
    public class ThoughtsEditorArea : EditorArea<ThoughtsEditorState>
    {
        ImmutableList<EditorThought> _thoughts = ImmutableList<EditorThought>.Empty;
        public IReadOnlyList<EditorThought> Thoughts => _thoughts;

        // Used to compare references so that selection events are not invoked
        // if no changes were made or we're deselecting twice.
        EditorThought? _lastSelectedThought = null;

        /// <summary>
        /// Invoked when a <see cref="EditorThought"/> is selected or deselected.
        /// <para>Deselecting passes a <see langword="null"/> <see cref="EditorThought"/>.</para>
        /// </summary>
        public event Action<EditorThought?>? OnThoughtSelectionChanged;

        // Note: While we are using records for EditorThoughts, we'll keep (runtime) Thoughts readonly.
        // They're editable using a single Edit method that can change everything at once.
        // This assumes their structure is unlikely to change and will always need few parameters.

        public Guid AddThought(string nameKey, string descriptionKey, CharacterSides side)
        {
            Guid newGuid = Guid.NewGuid();

            Thought thought = new Thought(newGuid, nameKey, descriptionKey, side);

            EditorThought editorThought = new EditorThought(thought);

            BeginChange();

            _thoughts = _thoughts.Add(editorThought);

            EndChange();

            return newGuid;
        }

        public Guid InsertThought(int index, string nameKey, string descriptionKey, CharacterSides side)
        {
            Guid newGuid = Guid.NewGuid();

            Thought thought = new Thought(newGuid, nameKey, descriptionKey, side);

            EditorThought editorThought = new EditorThought(thought);

            BeginChange();

            _thoughts = _thoughts.Insert(index, editorThought);

            EndChange();

            return newGuid;
        }

        public bool RemoveThought(Guid guid)
        {
            int foundThought = _thoughts.FindIndex(thought => thought.RuntimeThought.Guid == guid);

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
            int index = _thoughts.FindIndex(thought => thought.RuntimeThought.Guid == guid);

            // An edit changes state in a way that failure doesn't match.
            if (index < 0)
                throw new KeyNotFoundException($"No thoughts with id '{guid}'");

            EditorThought currentThought = _thoughts[index];

            Thought modifiedThought = new Thought(guid, nameKey, descriptionKey, side);

            if (currentThought.RuntimeThought.HasSameValues(modifiedThought))
                return;

            EditorThought modifiedEditorThought = _thoughts[index] with { RuntimeThought = modifiedThought };

            BeginChange();

            _thoughts = _thoughts.SetItem(index, modifiedEditorThought);

            EndChange();
        }

        public void MoveThought(Guid guid, int newIndex)
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
        public void SelectThought(Guid? guid)
        {
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

            if (ReferenceEquals(_lastSelectedThought, foundThought))
                return;

            _lastSelectedThought = foundThought;

            OnThoughtSelectionChanged?.Invoke(foundThought);
        }

        public void SetEditorNote(Guid guid, string editorNote)
        {
            int index = _thoughts.FindIndex(thought => thought.RuntimeThought.Guid == guid);

            if (index < 0)
                throw new KeyNotFoundException($"No thoughts with id '{guid}'");

            EditorThought currentThought = _thoughts[index];

            if (currentThought.EditorNote == editorNote)
                return;

            EditorThought modifiedThought = currentThought with { EditorNote = editorNote };

            BeginChange();

            _thoughts = _thoughts.SetItem(index, modifiedThought);

            EndChange();
        }

        #region EditorArea

        protected override ThoughtsEditorState GetStateToSave()
        {
            Guid? selection = null;

            if (_lastSelectedThought is not null)
                selection = _lastSelectedThought.RuntimeThought.Guid;

            return new ThoughtsEditorState(_thoughts, selection);
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
