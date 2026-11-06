using Dialuverc.Editor.Base;
using Dialuverc.Editor.Base.IO;
using Dialuverc.Editor.Base.Verifier;
using DualAttorneys.Dialuverc.Deductions;
using System.Collections.Immutable;
using System.Text.Json;
using DualAttorneys.Dialuverc.Editor.Deductions.Scratchpads;
using Dialuverc.Editor.Base.Scratchpads;
using Dialuverc.Editor.Base.Localization;
using Dialuverc.Editor.Base.Observability;

namespace DualAttorneys.Dialuverc.Editor.Deductions
{
    public partial class ThoughtsEditorArea : EditorArea<ThoughtsEditorState>
    {
        ImmutableList<EditorThought> _thoughts = ImmutableList<EditorThought>.Empty;
        public IReadOnlyList<EditorThought> Thoughts => _thoughts;

        ThoughtGuid? _selectionGuid = null;

        /// <summary>
        /// Invoked when a <see cref="EditorThought"/> is selected or deselected while already in <see cref="AddEditEnum.Edit"/> mode.
        /// <para>Deselecting passes a <see langword="null"/> <see cref="EditorThought"/>.</para>
        /// </summary>
        public event Action<EditorThought?>? OnThoughtSelectionChanged;

        readonly public ObservableValue<AddEditEnum> ScratchpadSelection;
        readonly AddEditScratchpads<ThoughtsEditorFields> _scratchpadsManager;
        public IScratchpadsManager<ThoughtsEditorFields> ScratchpadsManager => _scratchpadsManager;

        readonly LocalizationStore _localizationManager;
        public IReadOnlyLocalizationStore LocalizationManager => _localizationManager;

        // Records are immutable, making changes to this requires creating a new instance.
        readonly ThoughtsEditorFields _cachedDefaultScratchpad = new ThoughtsEditorFields();

        readonly JsonImportable<ImmutableList<EditorThought>> _editorThoughtsImportable;

        readonly IImportable[] _cachedImportables;

        public ThoughtsEditorArea(IReadOnlyObservableValue<string> localeChoice)
        {
            ScratchpadSelection = new ObservableValue<AddEditEnum>(AddEditEnum.Add);

            _scratchpadsManager = new AddEditScratchpads<ThoughtsEditorFields>(ScratchpadSelection);

            _scratchpadsManager.AddScratchpad = _cachedDefaultScratchpad;

            _localizationManager = new LocalizationStore(localeChoice, "thoughts");

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
            ArgumentNullException.ThrowIfNull(nameKey, nameof(nameKey));

            if (_scratchpadsManager.ActiveScratchpad is null)
                throw new InvalidOperationException("Can't set name key of a null scratchpad");

            if (nameKey == _scratchpadsManager.ActiveScratchpad.NameKey)
                return;

            BeginChange();

            _scratchpadsManager.ActiveScratchpad =
                _scratchpadsManager.ActiveScratchpad with { NameKey = nameKey };

            EndChange();
        }

        public void SetDescriptionKey(string descriptionKey)
        {
            ArgumentNullException.ThrowIfNull(descriptionKey, nameof(descriptionKey));

            if (_scratchpadsManager.ActiveScratchpad is null)
                throw new InvalidOperationException($"Can't set description key of a null scratchpad");

            if (descriptionKey == _scratchpadsManager.ActiveScratchpad.DescriptionKey)
                return;

            BeginChange();

            _scratchpadsManager.ActiveScratchpad =
                _scratchpadsManager.ActiveScratchpad with { DescriptionKey = descriptionKey };

            EndChange();
        }

        public void SetSide(CharacterSides side)
        {
            if (_scratchpadsManager.ActiveScratchpad is null)
                throw new InvalidOperationException($"Can't set side of a null scratchpad");

            if (side == _scratchpadsManager.ActiveScratchpad.Side)
                return;

            BeginChange();

            _scratchpadsManager.ActiveScratchpad =
                _scratchpadsManager.ActiveScratchpad with { Side = side };

            EndChange();
        }

        public void SetEditorNote(string editorNote)
        {
            ArgumentNullException.ThrowIfNull(editorNote, nameof(editorNote));

            if (_scratchpadsManager.ActiveScratchpad is null)
                throw new InvalidOperationException($"Can't set editor note of a null scratchpad");

            if (editorNote == _scratchpadsManager.ActiveScratchpad.EditorNote)
                return;

            BeginChange();

            _scratchpadsManager.ActiveScratchpad =
                _scratchpadsManager.ActiveScratchpad with { EditorNote = editorNote };

            EndChange();
        }

        public void SetNameLocalization(string nameLocalization)
        {
            ArgumentNullException.ThrowIfNull(nameLocalization, nameof(nameLocalization));

            if (_scratchpadsManager.ActiveScratchpad is null)
                throw new InvalidOperationException($"Can't set name localization of a null scratchpad");

            BeginChange();

            _scratchpadsManager.ActiveScratchpad =
                _scratchpadsManager.ActiveScratchpad with { NameLocalization = nameLocalization };

            EndChange();
        }

        public void SetDescriptionLocalization(string descriptionLocalization)
        {
            ArgumentNullException.ThrowIfNull(descriptionLocalization, nameof(descriptionLocalization));

            if (_scratchpadsManager.ActiveScratchpad is null)
                throw new InvalidOperationException($"Can't set description localization of a null scratchpad'");

            BeginChange();

            _scratchpadsManager.ActiveScratchpad =
                _scratchpadsManager.ActiveScratchpad with { DescriptionLocalization = descriptionLocalization };

            EndChange();
        }

        /// <summary>
        /// Applies all changes done so far on the <see cref="_scratchpadsManager.ActiveScratchpad"/> to the <see cref="Thoughts"/> list.
        /// <para>If <see cref="ScratchpadSelection.CurrentSelection"/> is <see cref="AddEditEnum.Add"/>, its corresponding scratchpad is also reset.</para>
        /// </summary>
        public ThoughtGuid FinishBuilding()
        {
            AddEditEnum currentSelection = _scratchpadsManager.Selection.Value;

            switch (currentSelection)
            {
                case AddEditEnum.Edit:

                    if (_selectionGuid is null)
                        throw new InvalidOperationException($"Can't commit changes to list with null Guid selection");

                    if (_scratchpadsManager.EditScratchpad is null)
                        throw new InvalidOperationException($"Can't commit changes to list with null editing scratchpad");

                    int index = _thoughts.FindIndex(d => d.RuntimeThought.Guid == _selectionGuid);

                    if (index < 0)
                        throw new InvalidOperationException($"No thought with id '{_selectionGuid}'");

                    // List is checked by reference. Prevent stacking state saving when there's no actual changes.
                    if (!_scratchpadsManager.EditScratchpad.HasChanges(_thoughts[index]))
                        return _thoughts[index].RuntimeThought.Guid;

                    BeginChange();

                    _thoughts = _thoughts.SetItem(index, _scratchpadsManager.EditScratchpad.CreateFromCurrentValues(_selectionGuid.Value));

                    AddOrSetValuesForCurrentLocale(_scratchpadsManager.EditScratchpad);

                    EndChange();

                    return _selectionGuid.Value;

                case AddEditEnum.Add:

                    if (_scratchpadsManager.AddScratchpad is null)
                        throw new InvalidOperationException($"Can't commit new thought to list with null adding scratchpad");

                    // To enforce the assumption of keys being defined from a single source at most.
                    // TODO: This error path may need better internal handling.
                    // Verifier should take care of surfacing the error to the user.
                    if (_localizationManager.TryGetValue(_scratchpadsManager.AddScratchpad.NameKey, out _))
                        throw new InvalidOperationException($"Name key \"{_scratchpadsManager.AddScratchpad.NameKey}\" is already defined in a different thought");

                    if (_localizationManager.TryGetValue(_scratchpadsManager.AddScratchpad.DescriptionKey, out _))
                        throw new InvalidOperationException($"Description key \"{_scratchpadsManager.AddScratchpad.DescriptionKey}\" is already defined in a different thought");

                    ThoughtGuid addedGuid = new ThoughtGuid();

                    BeginChange();

                    // Before wiping the adding scratchpad.
                    AddOrSetValuesForCurrentLocale(_scratchpadsManager.AddScratchpad);

                    _thoughts = _thoughts.Add(_scratchpadsManager.AddScratchpad.CreateFromCurrentValues(addedGuid));

                    _scratchpadsManager.AddScratchpad = _cachedDefaultScratchpad;

                    EndChange();

                    return addedGuid;

                default:
                    throw new InvalidOperationException($"{currentSelection} is not a supported mode");
            }
        }

        void AddOrSetValuesForCurrentLocale(ThoughtsEditorFields source)
        {
            _localizationManager.AddOrSetValue(source.NameKey, source.NameLocalization);
            _localizationManager.AddOrSetValue(source.DescriptionKey, source.DescriptionLocalization);
        }

        public bool RemoveThought(ThoughtGuid guid)
        {
            int foundThoughtIndex = _thoughts.FindIndex(thought => thought.RuntimeThought.Guid == guid);

            // Don't throw if we remove a thought that doesn't exist
            // since it not existing results in the same state as it being removed.
            if (foundThoughtIndex < 0)
                return false;

            BeginChange();

            EditorThought foundThought = _thoughts[foundThoughtIndex];

            _localizationManager.RemoveKeyForAllLocales(foundThought.RuntimeThought.NameKey);
            _localizationManager.RemoveKeyForAllLocales(foundThought.RuntimeThought.DescriptionKey);

            _thoughts = _thoughts.RemoveAt(foundThoughtIndex);

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

                _scratchpadsManager.EditScratchpad = null;
            }
            else
            {
                foundThought = Thoughts.FirstOrDefault(t => t.RuntimeThought.Guid == guid);

                if (foundThought is null)
                    throw new InvalidOperationException($"No thought with id '{guid}'");

                if (_scratchpadsManager.EditScratchpad is null)
                    _scratchpadsManager.EditScratchpad = _cachedDefaultScratchpad;

                _scratchpadsManager.EditScratchpad =
                    _scratchpadsManager.EditScratchpad with
                    {
                        NameKey = foundThought.RuntimeThought.NameKey,
                        DescriptionKey = foundThought.RuntimeThought.DescriptionKey,

                        Side = foundThought.RuntimeThought.Side,

                        EditorNote = foundThought.EditorNote
                    };
            }

            _selectionGuid = guid;

            if (!ScratchpadSelection.ChangeValue(AddEditEnum.Edit))
                OnThoughtSelectionChanged?.Invoke(foundThought);
        }

        #region Exportables/Importables

        public override IEnumerable<IImportable> GetEditorImportables() => _cachedImportables;

        ImmutableList<EditorThought> GetEditorThoughtsForImportable() => _thoughts;

        void OnEditorThoughtsImported(ImmutableList<EditorThought>? imported)
        {
            // TODO: Proper error handling.
            if (imported is null)
                return;

            // This probably should not be done in a transaction.
            BeginChange();

            _thoughts = imported;

            EndChange();
        }

        #endregion

        #region EditorArea

        protected override ThoughtsEditorState GetStateToSave()
        {
            return new ThoughtsEditorState(_thoughts, _scratchpadsManager, _selectionGuid, _localizationManager);
        }

        protected override bool CheckStateEquality(ThoughtsEditorState a, ThoughtsEditorState b)
        {
            // Selecting a thought or changing modes is not a state change.
            return a.Thoughts == b.Thoughts &&
                a.AddScratchpad == b.AddScratchpad &&
                a.EditScratchpad == b.EditScratchpad &&
                a.LocalizationLookup == b.LocalizationLookup;
        }

        protected override void ApplyRestoredState(ThoughtsEditorState newState)
        {
            _thoughts = newState.Thoughts;

            _scratchpadsManager.AddScratchpad = newState.AddScratchpad;
            _scratchpadsManager.EditScratchpad = newState.EditScratchpad;

            _localizationManager.Lookup = newState.LocalizationLookup;

            _selectionGuid = newState.ThoughtSelection;

            ScratchpadSelection.ChangeValue(newState.Mode, invokeEvent: false);
        }

        public override IReadOnlyList<Problem> Verify() => ThoughtsEditorVerifier.Run(_scratchpadsManager, _selectionGuid, _thoughts, _localizationManager);

        #endregion
    }
}
