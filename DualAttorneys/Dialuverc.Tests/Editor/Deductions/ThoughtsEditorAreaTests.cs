using DualAttorneys.Dialuverc.Deductions;
using DualAttorneys.Dialuverc.Editor.Deductions;
using Dialuverc.Editor.Base.IO;

using Dialuverc.Editor.Base.StateTracking;
using DualAttorneys.Dialuverc.Editor.Deductions.Scratchpads;

namespace DualAttorneys.Dialuverc.Tests.Editor.Deductions
{
    internal class ThoughtsEditorAreaTests
    {
        TestThoughtsEditorArea _area;

        [SetUp]
        public void SetUp()
        {
            _area = new TestThoughtsEditorArea();
        }

        [Test]
        // Should be enough to test both Add and Edit scratchpads, assuming ActiveScratchpad works properly.
        public void SetProperties()
        {
            _area.ScratchpadsManager.Selection.ChangeSelection(AddEditEnum.Add);

            Assert.That(_area.ScratchpadsManager.ActiveScratchpad!.NameKey, Is.Empty);
            Assert.That(_area.ScratchpadsManager.ActiveScratchpad.DescriptionKey, Is.Empty);
            Assert.That(_area.ScratchpadsManager.ActiveScratchpad.Side, Is.EqualTo(CharacterSides.Any));
            Assert.That(_area.ScratchpadsManager.ActiveScratchpad.EditorNote, Is.Empty);

            _area.SetNameKey("nameKey");
            Assert.That(_area.ScratchpadsManager.ActiveScratchpad.NameKey, Is.EqualTo("nameKey"));

            _area.SetDescriptionKey("descriptionKey");
            Assert.That(_area.ScratchpadsManager.ActiveScratchpad.DescriptionKey, Is.EqualTo("descriptionKey"));

            _area.SetSide(CharacterSides.Tychon);
            Assert.That(_area.ScratchpadsManager.ActiveScratchpad.Side, Is.EqualTo(CharacterSides.Tychon));

            _area.SetEditorNote("editorNote");
            Assert.That(_area.ScratchpadsManager.ActiveScratchpad.EditorNote, Is.EqualTo("editorNote"));
        }

        [Test]
        public void SetPropertiesHistory()
        {
            _area.ScratchpadsManager.Selection.ChangeSelection(AddEditEnum.Add);

            _area.SetNameKey("nameKey");
            _area.SetDescriptionKey("descriptionKey");
            _area.SetSide(CharacterSides.Tychon);
            _area.SetEditorNote("editorNote");

            _area.RestorePreviousState(RestoreDirection.Previous);
            Assert.That(_area.ScratchpadsManager.ActiveScratchpad!.EditorNote, Is.Empty);

            _area.RestorePreviousState(RestoreDirection.Previous);
            Assert.That(_area.ScratchpadsManager.ActiveScratchpad.Side, Is.EqualTo(CharacterSides.Any));

            _area.RestorePreviousState(RestoreDirection.Previous);
            Assert.That(_area.ScratchpadsManager.ActiveScratchpad.DescriptionKey, Is.Empty);

            _area.RestorePreviousState(RestoreDirection.Previous);
            Assert.That(_area.ScratchpadsManager.ActiveScratchpad.NameKey, Is.Empty);

            _area.RestorePreviousState(RestoreDirection.Next);
            Assert.That(_area.ScratchpadsManager.ActiveScratchpad.NameKey, Is.EqualTo("nameKey"));

            _area.RestorePreviousState(RestoreDirection.Next);
            Assert.That(_area.ScratchpadsManager.ActiveScratchpad.DescriptionKey, Is.EqualTo("descriptionKey"));

            _area.RestorePreviousState(RestoreDirection.Next);
            Assert.That(_area.ScratchpadsManager.ActiveScratchpad.Side, Is.EqualTo(CharacterSides.Tychon));

            _area.RestorePreviousState(RestoreDirection.Next);
            Assert.That(_area.ScratchpadsManager.ActiveScratchpad.EditorNote, Is.EqualTo("editorNote"));
        }

        [Test]
        public void ChangingPropertyWithSameValueDoesNotSave()
        {
            _area.ScratchpadsManager.Selection.ChangeSelection(AddEditEnum.Add);

            _area.SetNameKey("nameKey");
            _area.SetDescriptionKey(_area.ScratchpadsManager.ActiveScratchpad!.DescriptionKey);

            _area.RestorePreviousState(RestoreDirection.Previous);

            Assert.That(_area.CanUndo, Is.False);
        }

        [Test]
        public void FinishBuildingWithNoChangesIsNoOp()
        {
            _area.ScratchpadsManager.Selection.ChangeSelection(AddEditEnum.Add);

            _area.SetNameKey("nameKey");
            _area.SetDescriptionKey("descriptionKey");

            ThoughtGuid toEdit = _area.FinishBuilding();

            _area.SelectThought(toEdit);

            _area.SetNameKey("editedNameKey");

            _area.FinishBuilding();
            _area.FinishBuilding();

            Assert.That(_area.CurrentStateIndex, Is.EqualTo(5));
        }

        [Test]
        public void AddThoughtToList()
        {
            Assert.That(_area.Thoughts, Is.Empty);

            _area.ScratchpadsManager.Selection.ChangeSelection(AddEditEnum.Add);

            PopulateSampleThought();

            ThoughtsEditorFields lastFieldsState = _area.ScratchpadsManager.ActiveScratchpad!;

            _area.FinishBuilding();

            Assert.That(_area.Thoughts, Has.Count.EqualTo(1));
            Assert.That(lastFieldsState.HasChanges(_area.Thoughts[0]), Is.False);

            Assert.That(_area.ScratchpadsManager.ActiveScratchpad, Is.Not.EqualTo(lastFieldsState));
        }

        [Test]
        public void AddThoughtToListHistory()
        {
            _area.ScratchpadsManager.Selection.ChangeSelection(AddEditEnum.Add);

            PopulateSampleThought();

            ThoughtsEditorFields lastFieldsState = _area.ScratchpadsManager.ActiveScratchpad!;

            _area.FinishBuilding();

            _area.RestorePreviousState(RestoreDirection.Previous);

            Assert.That(_area.Thoughts, Is.Empty);
            Assert.That(_area.ScratchpadsManager.ActiveScratchpad, Is.EqualTo(lastFieldsState));

            _area.RestorePreviousState(RestoreDirection.Next);

            Assert.That(_area.Thoughts, Has.Count.EqualTo(1));
            Assert.That(lastFieldsState.HasChanges(_area.Thoughts[0]), Is.False);
        }

        [Test]
        public void GuidsAreDifferent()
        {
            ThoughtGuid guid1 = AddSampleThoughtToList(0);
            ThoughtGuid guid2 = AddSampleThoughtToList(1);

            Assert.That(guid1, Is.Not.EqualTo(guid2));
        }

        [Test]
        public void RemoveThoughtFromList()
        {
            ThoughtGuid guid = AddSampleThoughtToList();

            Assert.That(_area.RemoveThought(guid), Is.True);
            Assert.That(_area.RemoveThought(guid), Is.False);

            Assert.That(_area.Thoughts, Is.Empty);
        }

        [Test]
        public void RemoveThoughtFromListHistory()
        {
            ThoughtGuid guid = AddSampleThoughtToList();

            _area.RemoveThought(guid);
            _area.RestorePreviousState(RestoreDirection.Previous);

            Assert.That(_area.Thoughts, Has.Count.EqualTo(1));
            Assert.That(_area.Thoughts[0].RuntimeThought.Guid, Is.EqualTo(guid));

            _area.RestorePreviousState(RestoreDirection.Next);

            Assert.That(_area.Thoughts, Is.Empty);
        }

        [Test]
        public void MoveThought()
        {
            ThoughtGuid guid1 = AddSampleThoughtToList(0);
            ThoughtGuid guid2 = AddSampleThoughtToList(1);

            _area.MoveThought(guid2, 0);

            Assert.That(_area.Thoughts[0].RuntimeThought.Guid, Is.EqualTo(guid2));
            Assert.That(_area.Thoughts[1].RuntimeThought.Guid, Is.EqualTo(guid1));
        }

        [Test]
        public void MoveThoughtHistory()
        {
            ThoughtGuid guid1 = AddSampleThoughtToList(0);
            ThoughtGuid guid2 = AddSampleThoughtToList(1);

            _area.MoveThought(guid2, 0);
            _area.RestorePreviousState(RestoreDirection.Previous);

            Assert.That(_area.Thoughts[0].RuntimeThought.Guid, Is.EqualTo(guid1));
            Assert.That(_area.Thoughts[1].RuntimeThought.Guid, Is.EqualTo(guid2));

            _area.RestorePreviousState(RestoreDirection.Next);

            Assert.That(_area.Thoughts[0].RuntimeThought.Guid, Is.EqualTo(guid2));
            Assert.That(_area.Thoughts[1].RuntimeThought.Guid, Is.EqualTo(guid1));
        }

        // Moving to same index is effectively untestable as it is a no-op path which has the same result as it running.

        [Test]
        public void SelectInvokesCorrectEvent()
        {
            _area.ScratchpadsManager.Selection.ChangeSelection(AddEditEnum.Add);

            int changeModeInvokations = 0;
            int changeSelectInvokations = 0;
            EditorThought? currentSelection = null;

            _area.ScratchpadsManager.Selection.SelectionChanged += (_) => { changeModeInvokations++; };
            _area.OnThoughtSelectionChanged += (selection) =>
            {
                changeSelectInvokations++;
                currentSelection = selection;
            };

            ThoughtGuid guid1 = AddSampleThoughtToList(0);
            ThoughtGuid guid2 = AddSampleThoughtToList(1);

            _area.SelectThought(guid1);

            Assert.That(_area.ScratchpadsManager.Selection.CurrentSelection, Is.EqualTo(AddEditEnum.Edit));
            Assert.That(_area.ScratchpadsManager.ActiveScratchpad!.HasChanges(_area.Thoughts[0]), Is.False);
            Assert.That(changeModeInvokations, Is.EqualTo(1));
            Assert.That(changeSelectInvokations, Is.Zero);
            // When in Add, only OnModeChanged is invoked, which leaves currentSelection null.

            _area.SelectThought(guid2);

            Assert.That(_area.ScratchpadsManager.Selection.CurrentSelection, Is.EqualTo(AddEditEnum.Edit));
            Assert.That(changeModeInvokations, Is.EqualTo(1));
            Assert.That(changeSelectInvokations, Is.EqualTo(1));
            Assert.That(currentSelection, Is.EqualTo(_area.Thoughts[1]));
            Assert.That(_area.ScratchpadsManager.ActiveScratchpad.HasChanges(currentSelection), Is.False);
        }

        [Test]
        public void SelectSameIsNoOp()
        {
            ThoughtGuid guid = AddSampleThoughtToList();

            _area.ScratchpadsManager.Selection.ChangeSelection(AddEditEnum.Edit);

            int eventInvokations = 0;

            _area.OnThoughtSelectionChanged += (selection) => { eventInvokations++; };

            _area.SelectThought(guid);
            _area.SelectThought(guid);

            Assert.That(eventInvokations, Is.EqualTo(1));
        }

        [Test]
        public void RemoveSelection()
        {
            EditorThought? currentSelection = null;

            _area.OnThoughtSelectionChanged += (selection) => { currentSelection = selection; };

            ThoughtGuid guid = AddSampleThoughtToList();

            _area.ScratchpadsManager.Selection.ChangeSelection(AddEditEnum.Edit);

            _area.SelectThought(guid);

            Assert.That(currentSelection, Is.EqualTo(_area.Thoughts[0]));

            _area.RemoveThought(guid);

            Assert.That(currentSelection, Is.Null);
            Assert.That(_area.ScratchpadsManager.ActiveScratchpad, Is.Null);
        }

        [Test]
        public void Deselect()
        {
            ThoughtGuid guid = AddSampleThoughtToList();

            _area.ScratchpadsManager.Selection.ChangeSelection(AddEditEnum.Edit);

            int eventInvokations = 0;
            EditorThought? currentSelection = null;

            _area.OnThoughtSelectionChanged += (selection) =>
            {
                eventInvokations++;
                currentSelection = selection;
            };

            _area.SelectThought(guid);
            _area.SelectThought(null);

            Assert.That(currentSelection, Is.Null);
            Assert.That(_area.ScratchpadsManager.ActiveScratchpad, Is.Null);
        }

        [Test]
        public void EditThoughtInList()
        {
            ThoughtGuid guid = AddSampleThoughtToList();

            _area.SelectThought(guid);
            _area.SetNameKey("editedNameKey");
            _area.SetDescriptionKey("editedDescriptionKey");
            _area.SetSide(CharacterSides.Forger);
            _area.SetEditorNote("editedEditorNote");
            _area.FinishBuilding();

            Assert.That(_area.Thoughts[0].RuntimeThought.NameKey, Is.EqualTo("editedNameKey"));
            Assert.That(_area.Thoughts[0].RuntimeThought.DescriptionKey, Is.EqualTo("editedDescriptionKey"));
            Assert.That(_area.Thoughts[0].RuntimeThought.Side, Is.EqualTo(CharacterSides.Forger));
            Assert.That(_area.Thoughts[0].EditorNote, Is.EqualTo("editedEditorNote"));

            Assert.That(_area.ScratchpadsManager.ActiveScratchpad!.HasChanges(_area.Thoughts[0]), Is.False);
        }

        [Test]
        public void EditThoughtInListHistory()
        {
            ThoughtGuid guid = AddSampleThoughtToList();

            _area.SelectThought(guid);
            _area.SetNameKey("editedNameKey");
            _area.SetDescriptionKey("editedDescriptionKey");
            _area.SetSide(CharacterSides.Forger);
            _area.SetEditorNote("editedEditorNote");
            _area.FinishBuilding();
            _area.RestorePreviousState(RestoreDirection.Previous);

            Assert.That(_area.ScratchpadsManager.ActiveScratchpad!.HasChanges(_area.Thoughts[0]), Is.True);

            _area.RestorePreviousState(RestoreDirection.Next);

            Assert.That(_area.ScratchpadsManager.ActiveScratchpad.HasChanges(_area.Thoughts[0]), Is.False);
        }

        [Test]
        public void ExportAndImportForEditor()
        {
            ThoughtGuid firstThoughtGuid = AddSampleThoughtToList();
            ThoughtGuid secondThoughtGuid = AddSampleThoughtToList();

            EditorThought firstThoughtInfo = _area.Thoughts[0];
            EditorThought secondThoughtInfo = _area.Thoughts[1];

            EditorThought[] thoughtInfos = new EditorThought[] { firstThoughtInfo, secondThoughtInfo };

            using (TemporaryFileStorage fileStorage = new TemporaryFileStorage(nameof(ThoughtsEditorAreaTests)))
            {
                IEnumerable<IImportable> importables = _area.GetEditorImportables();

                // Can export either to Zip or folder.
                // Picked folder because importing from it happened to be implemented before Zip.
                ExportableUtils.ExportToFolder(importables, fileStorage.AbsoluteFolderPath);

                Assert.That(File.Exists(Path.Combine(fileStorage.AbsoluteFolderPath, importables.First().ExportPath)), Is.True);

                _area.RemoveThought(firstThoughtGuid);
                _area.RemoveThought(secondThoughtGuid);

                Assert.That(_area.Thoughts, Is.Empty);

                ExportableUtils.ImportFromFolder(importables, fileStorage.AbsoluteFolderPath);

                Assert.That(_area.Thoughts, Has.Count.EqualTo(thoughtInfos.Length));

                for (int i = 0; i < thoughtInfos.Length; i++)
                {
                    Assert.That(_area.Thoughts[i].RuntimeThought.HasSameValues(thoughtInfos[i].RuntimeThought), Is.True);
                    Assert.That(_area.Thoughts[i].RuntimeThought.Guid, Is.EqualTo(thoughtInfos[i].RuntimeThought.Guid));
                    Assert.That(_area.Thoughts[i].EditorNote, Is.EqualTo(thoughtInfos[i].EditorNote));
                }
            }
        }

        #region Helpers

        void PopulateSampleThought(int offset = 0)
        {
            _area.SetNameKey($"nameKey{offset}");
            _area.SetDescriptionKey($"descriptionKey{offset}");
            _area.SetSide(CharacterSides.Tychon);
            _area.SetEditorNote($"editorNote{offset}");
        }

        ThoughtGuid AddSampleThoughtToList(int offset = 0)
        {
            AddEditEnum previousMode = _area.ScratchpadsManager.Selection.CurrentSelection;

            _area.ScratchpadsManager.Selection.ChangeSelection(AddEditEnum.Add);

            PopulateSampleThought(offset);

            ThoughtGuid guid = _area.FinishBuilding();

            _area.ScratchpadsManager.Selection.ChangeSelection(previousMode);

            return guid;
        }

        private class TestThoughtsEditorArea : ThoughtsEditorArea
        {
            public new int CurrentStateIndex => base.CurrentState;
        }

        #endregion
    }
}
