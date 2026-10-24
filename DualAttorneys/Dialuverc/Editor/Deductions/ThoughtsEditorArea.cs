using Dialuverc.Editor.Base;
using Dialuverc.Editor.Base.IO;
using Dialuverc.Editor.Base.Verifier;
using Dialuverc.Editor.Base.Localization;
using DualAttorneys.Dialuverc.Deductions;
using System.Collections.Immutable;
using System.Text.Json;
using Dialuverc.Editor.Base.Observability;

namespace DualAttorneys.Dialuverc.Editor.Deductions
{
    public partial class ThoughtsEditorArea : EditorArea<ThoughtsEditorState>, IHasLocalizableKeysFlatFile
    {
        ImmutableList<EditorThought> _thoughts = ImmutableList<EditorThought>.Empty;
        public IReadOnlyList<EditorThought> Thoughts => _thoughts;

        ThoughtGuid? _selectionGuid = null;

        /// <summary>
        /// Invoked when a <see cref="EditorThought"/> is selected or deselected while already in <see cref="AddEditEnum.Edit"/> mode.
        /// <para>Deselecting passes a <see langword="null"/> <see cref="EditorThought"/>.</para>
        /// </summary>
        public event Action<EditorThought?>? OnThoughtSelectionChanged;

        readonly EditorScratchpadManager<EditorThought, AddEditEnum> _scratchpadManager;
        public EnumSelection<AddEditEnum> ScratchpadSelection => _scratchpadManager.Selection;
        public EditorThought? ActiveScratchpad => _scratchpadManager.ActiveScratchpad;

        readonly JsonImportable<ImmutableList<EditorThought>> _editorThoughtsImportable;

        readonly IImportable[] _cachedImportables;

        public ThoughtsEditorArea()
        {
            _scratchpadManager = new EditorScratchpadManager<EditorThought, AddEditEnum>();

            _scratchpadManager[AddEditEnum.Add] = CreateDefaultEditorThought();

            _editorThoughtsImportable = new JsonImportable<ImmutableList<EditorThought>>(nameof(ThoughtsEditorArea), GetEditorThoughtsForImportable, OnEditorThoughtsImported, new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = true,
            });

            _cachedImportables = new IImportable[]
            {
                _editorThoughtsImportable,
            };
        }

        public void SetNameKey(string nameKey)
        {
            BeginChange();

            _scratchpadManager.ActiveScratchpad = _scratchpadManager.ActiveScratchpad! with
            {
                RuntimeThought = _scratchpadManager.ActiveScratchpad.RuntimeThought with
                {
                    NameKey = nameKey,
                }
            };

            EndChange();
        }

        public void SetDescriptionKey(string descriptionKey)
        {
            BeginChange();

            _scratchpadManager.ActiveScratchpad = _scratchpadManager.ActiveScratchpad! with
            {
                RuntimeThought = _scratchpadManager.ActiveScratchpad.RuntimeThought with
                {
                    DescriptionKey = descriptionKey,
                }
            };

            EndChange();
        }

        public void SetSide(CharacterSides side)
        {
            BeginChange();

            _scratchpadManager.ActiveScratchpad = _scratchpadManager.ActiveScratchpad! with
            {
                RuntimeThought = _scratchpadManager.ActiveScratchpad.RuntimeThought with
                {
                    Side = side
                }
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
        /// <para>If <see cref="ScratchpadSelection.CurrentSelection"/> is <see cref="AddEditEnum.Add"/>, its builder is also reset.</para>
        /// </summary>
        public ThoughtGuid FinishBuilding()
        {
            switch (ScratchpadSelection.CurrentSelection)
            {
                case AddEditEnum.Edit:

                    if (_scratchpadManager.ActiveScratchpad is null)
                        throw new InvalidOperationException($"Can't call {nameof(FinishBuilding)} in {AddEditEnum.Edit} mode with null {nameof(_scratchpadManager.ActiveScratchpad)}");

                    int index = _thoughts.FindIndex(d => d.RuntimeThought.Guid == _scratchpadManager.ActiveScratchpad.RuntimeThought.Guid);

                    if (index < 0)
                        throw new InvalidOperationException($"No thought with id '{_scratchpadManager.ActiveScratchpad.RuntimeThought.Guid}'");

                    // List is checked by reference. Prevent stacking state saving when there's no actual changes.
                    if (_thoughts[index] == _scratchpadManager.ActiveScratchpad)
                        return _thoughts[index].RuntimeThought.Guid;

                    BeginChange();

                    _thoughts = _thoughts.SetItem(index, _scratchpadManager.ActiveScratchpad!);

                    EndChange();

                    return _scratchpadManager.ActiveScratchpad.RuntimeThought.Guid;

                case AddEditEnum.Add:

                    if (_scratchpadManager.ActiveScratchpad is null)
                        throw new InvalidOperationException($"Can't call {nameof(FinishBuilding)} in {AddEditEnum.Add} mode with null {nameof(_scratchpadManager.ActiveScratchpad)}");

                    ThoughtGuid addedGuid = _scratchpadManager.ActiveScratchpad.RuntimeThought.Guid;

                    BeginChange();

                    _thoughts = _thoughts.Add(_scratchpadManager.ActiveScratchpad);

                    _scratchpadManager.ActiveScratchpad = CreateDefaultEditorThought();

                    EndChange();

                    return addedGuid;

                default:
                    throw new InvalidOperationException($"{ScratchpadSelection.CurrentSelection} is not supported");
            }
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

            _scratchpadManager[AddEditEnum.Edit] = foundThought;
            _selectionGuid = guid;

            if (!ScratchpadSelection.ChangeSelection(AddEditEnum.Edit))
                OnThoughtSelectionChanged?.Invoke(foundThought);
        }

        EditorThought CreateDefaultEditorThought() => new EditorThought(new Thought(
            new ThoughtGuid(),
            string.Empty,
            string.Empty,
            CharacterSides.Any));

        #region Exportables/Importables

        public override IEnumerable<IImportable> GetEditorImportables() => _cachedImportables;

        ImmutableList<EditorThought> GetEditorThoughtsForImportable() => _thoughts;

        void OnEditorThoughtsImported(ImmutableList<EditorThought>? imported)
        {
            // TODO: Proper error handling.
            // This probably should not be done in a transaction.

            if (imported is null)
                return;

            BeginChange();

            _thoughts = imported;

            EndChange();
        }

        #endregion

        #region EditorArea

        protected override ThoughtsEditorState GetStateToSave()
        {
            return new ThoughtsEditorState(_thoughts, _scratchpadManager[AddEditEnum.Add], _scratchpadManager[AddEditEnum.Edit], ScratchpadSelection.CurrentSelection, _selectionGuid);
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
            _scratchpadManager[AddEditEnum.Add] = newState.AddBuilder;
            _scratchpadManager[AddEditEnum.Edit] = newState.EditBuilder;
            _selectionGuid = newState.ThoughtSelection;

            ScratchpadSelection.ChangeSelection(newState.Mode, invokeEvent: false);
        }

        public override IReadOnlyList<Problem> Verify() => ThoughtsEditorVerifier.Run(Thoughts);

        // This is what was on 'ThoughtsEditorPage' in Dual-Attorneys.
        // IHasLocalizableKeysFlatFile requires this to be implemented
        // so I thought that it should've been moved here.
        // Note: we are already in the so called "_editorArea" so it's just Thoughts
        public IReadOnlySet<string> GetLocalizableKeysFlatFile()
        {
            HashSet<string> keys = new HashSet<string>();

            foreach (EditorThought editorThought in Thoughts)
            {
                keys.Add(editorThought.RuntimeThought.NameKey);
                keys.Add(editorThought.RuntimeThought.DescriptionKey);
            }

            return keys;
        }

        #endregion
    }
}
