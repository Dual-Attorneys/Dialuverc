using DualAttorneys.Dialuverc.Deductions;
using System.Collections.Immutable;

namespace DualAttorneys.Dialuverc.Editor.Deductions
{
    internal class DeductionsEditorArea
    {
        // This area uses 2 builders, 1 for adding, 1 for editing.
        // Deductions need to be built progressively rather than completely at once (the needed params are too complex).
        // The builders allow us to save history for both and only modify the deductions list when done.
        // Note that UI is responsible for appropriately updating (e.g. on undo/redo, when done building and so on).
        Mode _currentMode = Mode.Add;

        /// <summary>
        /// Builder to be used for Add/Insert operations.
        /// </summary>
        EditorDeduction _addingBuilder = CreateDefaultDeduction();

        /// <summary>
        /// Builder to be used for any operation that modifies elements that already exist in the list.
        /// </summary>
        EditorDeduction? _editingBuilder;

        // Since the values and reference the editing builder holds can change, use this as a stable identifier.
        Guid? _selectionGuid;

        /// <summary>
        /// The appropriate deduction builder based on the <see cref="_currentMode"/>.<br/>
        /// Guaranteed to never be <see langword="null"/>.
        /// </summary>
        EditorDeduction _activeBuilder
        {
            get
            {
                switch (_currentMode)
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
                switch (_currentMode)
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
        /// Sets which deduction builder to use based on <paramref name="newMode"/>.<br/>
        /// All changes happen on the currently active builder.
        /// </summary>
        public void ChangeMode(Mode newMode) => _currentMode = newMode;

        /// <summary>
        /// Selects the <see cref="EditorDeduction"/> corresponding to the given <paramref name="guid"/>.
        /// <para>Pass <see langword="null"/> to deselect.</para>
        /// </summary>
        // Note: Currently, selecting doesn't change mode to Edit. Do we want that?
        public void SelectDeduction(Guid? guid)
        {
            if (_selectionGuid == guid)
                return;

            EditorDeduction? foundDeduction;

            if (!guid.HasValue)
            {
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

            // TODO: Selection event.
        }

        public void SetAlias(string alias)
        {
            _activeBuilder = _activeBuilder with { Alias = alias };
        }

        public void SetEditorNote(string editorNote)
        {
            _activeBuilder = _activeBuilder with { EditorNote = editorNote };
        }

        public void AddOutputThought(ThoughtGuid guid)
        {
            // TODO: Maybe check for duplicates?

            _activeBuilder = _activeBuilder with
            {
                Outputs = _activeBuilder.Outputs with
                {
                    Thoughts = _activeBuilder.Outputs.Thoughts.Add(guid)
                }
            };
        }

        public void RemoveOutputThought(ThoughtGuid guid)
        {
            ImmutableArray<ThoughtGuid> newArray = _activeBuilder.Outputs.Thoughts.Remove(guid);

            // Don't save history if we try removing a non-existing thought.
            // Failure results in the same state as success anyway.
            if (_activeBuilder.Outputs.Thoughts == newArray)
                return;

            _activeBuilder = _activeBuilder with
            {
                Outputs = _activeBuilder.Outputs with
                {
                    Thoughts = newArray
                }
            };
        }

        /// <summary>
        /// Applies all changes done so far to the active builder to the <see cref="Deductions"/> list.
        /// </summary>
        public void FinishBuilding()
        {
            if (_currentMode == Mode.Edit)
            {
                if (_editingBuilder is null)
                    throw new InvalidOperationException($"Can't call {nameof(FinishBuilding)} in {Mode.Edit} mode with null {_editingBuilder}");

                int index = _deductions.FindIndex(d => d.Guid == _editingBuilder.Guid);

                _deductions = _deductions.SetItem(index, _editingBuilder!);
                return;
            }

            _deductions = _deductions.Add(_addingBuilder);

            _addingBuilder = CreateDefaultDeduction();
        }

        static EditorDeduction CreateDefaultDeduction() => new EditorDeduction(
            Guid.NewGuid(),
            new ThoughtCombination(null, null, null),
            new DeductionOutputs());

        public enum Mode
        {
            Add,
            Edit
        }
    }
}
