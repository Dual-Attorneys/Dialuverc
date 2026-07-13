using Dialuverc.Editor.Base;
using DualAttorneys.Dialuverc.Deductions;
using DualAttorneys.Dialuverc.Editor.Deductions;

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
        public void JsonSerializationFormat()
        {
            string expectedJson =
@"[
  {{
    ""Guid"": ""{0}"",
    ""NameKey"": ""thought1"",
    ""DescriptionKey"": ""description1"",
    ""Side"": 0
  }},
  {{
    ""Guid"": ""{1}"",
    ""NameKey"": ""thought2"",
    ""DescriptionKey"": ""description2"",
    ""Side"": 1
  }}
]";

            Thought thought1 = new Thought(new ThoughtGuid(), "thought1", "description1", CharacterSides.Tychon);
            Thought thought2 = new Thought(new ThoughtGuid(), "thought2", "description2", CharacterSides.Forger);

            ThoughtGuid guid1 = _area.AddThought(thought1.NameKey, thought1.DescriptionKey, thought1.Side);
            ThoughtGuid guid2 = _area.AddThought(thought2.NameKey, thought2.DescriptionKey, thought2.Side);

            expectedJson = String.Format(expectedJson, guid1, guid2);

            Assert.That(_area.Thoughts.Count, Is.EqualTo(2));

            Assert.That(_area.SerializeForExport(), Is.EqualTo(expectedJson));
        }

        [Test]
        public void AppendThought()
        {
            _area.AddThought("thought1", "description1", CharacterSides.Tychon);

            Assert.That(_area.Thoughts.Count, Is.EqualTo(1));

            Thought firstThought = _area.Thoughts[0].RuntimeThought;

            Assert.That(firstThought.NameKey, Is.EqualTo("thought1"));
            Assert.That(firstThought.DescriptionKey, Is.EqualTo("description1"));
            Assert.That(firstThought.Side, Is.EqualTo(CharacterSides.Tychon));

            _area.AddThought("thought2", "description2", CharacterSides.Forger);

            Assert.That(_area.Thoughts.Count, Is.EqualTo(2));

            Thought _secondThought = _area.Thoughts[1].RuntimeThought;

            Assert.That(_secondThought.NameKey, Is.EqualTo("thought2"));
            Assert.That(_secondThought.DescriptionKey, Is.EqualTo("description2"));
            Assert.That(_secondThought.Side, Is.EqualTo(CharacterSides.Forger));
        }

        [Test]
        public void RemoveThought()
        {
            _area.AddThought("thought1", "description1", CharacterSides.Tychon);
            ThoughtGuid guidToRemove = _area.AddThought("thought2", "description2", CharacterSides.Forger);
            _area.AddThought("thought3", "description3", CharacterSides.Any);

            _area.RemoveThought(guidToRemove);

            Assert.That(_area.Thoughts.Count, Is.EqualTo(2));

            Assert.That(_area.Thoughts[0].RuntimeThought.NameKey, Is.EqualTo("thought1"));
            Assert.That(_area.Thoughts[1].RuntimeThought.NameKey, Is.EqualTo("thought3"));
        }

        [Test]
        public void EditThought()
        {
            string[] thoughtNames =
            [
                "starterNameKey",
                "thought2",
                "thought3",
                "thought4",
            ];

            ThoughtGuid toEdit = _area.AddThought("starterNameKey", "starterDescriptionKey", CharacterSides.Tychon);
            _area.AddThought("thought2", "description2", CharacterSides.Any);
            _area.AddThought("thought3", "description3", CharacterSides.Any);
            _area.AddThought("thought4", "description4", CharacterSides.Any);

            _area.EditThought(toEdit, "newNameKey", "newDescriptionKey", CharacterSides.Forger);

            Assert.That(_area.Thoughts.Count, Is.EqualTo(4));

            Assert.That(_area.Thoughts[0].RuntimeThought.Guid, Is.EqualTo(toEdit));
            Assert.That(_area.Thoughts[0].RuntimeThought.NameKey, Is.EqualTo("newNameKey"));
            Assert.That(_area.Thoughts[0].RuntimeThought.DescriptionKey, Is.EqualTo("newDescriptionKey"));
            Assert.That(_area.Thoughts[0].RuntimeThought.Side, Is.EqualTo(CharacterSides.Forger));
        
            for (int i = 1; i < _area.Thoughts.Count; i++)
            {
                Assert.That(_area.Thoughts[i].RuntimeThought.NameKey == thoughtNames[i]);
            }

            Assert.That(_area.CurrentStateIndex, Is.EqualTo(5));
        }

        [Test]
        public void EditThoughtNoChangeNoStateSave()
        {
            Thought template = new Thought(new ThoughtGuid(), "nameKey", "descriptionKey", CharacterSides.Any);

            ThoughtGuid toEdit = _area.AddThought(template.NameKey, template.DescriptionKey, template.Side);
            _area.EditThought(toEdit, template.NameKey, template.DescriptionKey, template.Side);

            Assert.That(_area.CurrentStateIndex, Is.EqualTo(1));
        }

        [Test]
        public void InsertThought()
        {
            AppendDummyThoughts();

            int indexToInsert = 1;

            ThoughtGuid inserted = _area.InsertThought(indexToInsert, "insertedName", "insertedDescription", CharacterSides.Any);

            Assert.That(_area.Thoughts, Has.Count.EqualTo(_dummyThoughts.Length + 1));

            for (int i = 0; i < _dummyThoughts.Length; i++)
            {
                if (i == indexToInsert)
                    continue;

                if (i < indexToInsert)
                    Assert.That(_dummyThoughts[i].HasSameValues(_area.Thoughts[i].RuntimeThought), Is.True);
                else
                    Assert.That(_dummyThoughts[i].HasSameValues(_area.Thoughts[i + 1].RuntimeThought), Is.True);
            }

            Assert.That(_area.Thoughts[indexToInsert].RuntimeThought.NameKey, Is.EqualTo("insertedName"));
            Assert.That(_area.Thoughts[indexToInsert].RuntimeThought.DescriptionKey, Is.EqualTo("insertedDescription"));
            Assert.That(_area.Thoughts[indexToInsert].RuntimeThought.Side, Is.EqualTo(CharacterSides.Any));
        }

        [Test]
        public void MoveThought()
        {
            AppendDummyThoughts();

            int originalIndex = 0;
            int newIndex = 1;

            ThoughtGuid toMove = _area.Thoughts[originalIndex].RuntimeThought.Guid;
            ThoughtGuid previousAtIndex = _area.Thoughts[newIndex].RuntimeThought.Guid;

            _area.MoveThought(toMove, newIndex);

            Assert.That(_area.Thoughts[originalIndex].RuntimeThought.Guid, Is.EqualTo(previousAtIndex));
            Assert.That(_area.Thoughts[newIndex].RuntimeThought.Guid, Is.EqualTo(toMove));

            // Check whether the number of possible Undos is correct.
            // Needed for the test that checks whether moving to same index does not save state.
            Assert.That(_area.CurrentStateIndex, Is.EqualTo(4));
        }

        [Test]
        public void MoveThoughtToSameNoStateSave()
        {
            AppendDummyThoughts();

            int index = 1;

            ThoughtGuid toMove = _area.Thoughts[index].RuntimeThought.Guid;

            _area.MoveThought(toMove, index);

            Assert.That(_area.CurrentStateIndex, Is.EqualTo(3));
        }


        [Test]
        public void RestoreAfterAppend()
        {
            AppendDummyThoughts();

            int startingCount = _area.Thoughts.Count;

            for (int i = 0; i < startingCount; i++)
            {
                Assert.That(_area.Thoughts[i].RuntimeThought.HasSameValues(_dummyThoughts[i]), Is.True);
            }

            while (_area.CanUndo)
            {
                _area.RestorePreviousState(RestoreDirection.Previous);

                int indexToCheck = _area.Thoughts.Count - 1;

                // We'll hit an empty list on the last undo.
                if (_area.Thoughts.Count < 1)
                    continue;

                Thought a = _area.Thoughts[indexToCheck].RuntimeThought;
                Thought b = _dummyThoughts[indexToCheck];

                Assert.That(a.HasSameValues(b), Is.True);
            }

            Assert.That(_area.Thoughts, Has.Count.EqualTo(0));

            while (_area.CanRedo)
            {
                _area.RestorePreviousState(RestoreDirection.Next);

                int indexToCheck = _area.Thoughts.Count - 1;

                Thought a = _area.Thoughts[indexToCheck].RuntimeThought;
                Thought b = _dummyThoughts[indexToCheck];

                Assert.That(a.HasSameValues(b), Is.True);
            }

            Assert.That(_area.Thoughts, Has.Count.EqualTo(startingCount));
        }

        [Test]
        public void RestoreAfterRemoveFromFront()
        {
            AppendDummyThoughts();

            int starterCount = _area.Thoughts.Count;

            // Undoing until we can't anymore will include the template appends.
            // Count how many undos are needed to ONLY undo the removes.
            int amountToRedo = 0;

            for (int i = _dummyThoughts.Length - 1; i >= 0; i--)
            {
                _area.RemoveThought(
                    _area.Thoughts.First(t => t.RuntimeThought.NameKey == _dummyThoughts[i].NameKey).RuntimeThought.Guid);

                amountToRedo++;
            }

            Assert.That(_area.Thoughts, Has.Count.EqualTo(0));

            Assert.That(amountToRedo, Is.EqualTo(3));

            for (int i = 0; i < amountToRedo; i++) 
            {
                _area.RestorePreviousState(RestoreDirection.Previous);

                int indexToCheck =  _area.Thoughts.Count - 1;

                Thought a = _area.Thoughts[indexToCheck].RuntimeThought;
                Thought b = _dummyThoughts[indexToCheck];

                Assert.That(a.HasSameValues(b), Is.True);
            }

            Assert.That(_area.Thoughts, Has.Count.EqualTo(starterCount));

            while (_area.CanRedo)
            {
                _area.RestorePreviousState(RestoreDirection.Next);

                int indexToCheck = _area.Thoughts.Count - 1;

                // We'll hit an empty list on the last redo.
                if (_area.Thoughts.Count < 1)
                    continue;

                Thought a = _area.Thoughts[indexToCheck].RuntimeThought;
                Thought b = _dummyThoughts[indexToCheck];

                Assert.That(a.HasSameValues(b), Is.True);
            }

            Assert.That(_area.Thoughts, Has.Count.EqualTo(0));
        }

        [Test]
        public void RestoreAfterEdit()
        {
            AppendDummyThoughts();

            int indexToEdit = _dummyThoughts.Length / 2;

            ThoughtGuid toEdit = _area.Thoughts.First(t => t.RuntimeThought.NameKey == _dummyThoughts[indexToEdit].NameKey).RuntimeThought.Guid;

            Thought newTemplate = new Thought(new ThoughtGuid(), "NewKey", "NewDescription", CharacterSides.Any);

            _area.EditThought(toEdit, newTemplate.NameKey, newTemplate.DescriptionKey, newTemplate.Side);

            _area.RestorePreviousState(RestoreDirection.Previous);

            Assert.That(_area.Thoughts, Has.Count.EqualTo(_dummyThoughts.Length));

            for (int i = 0; i < _dummyThoughts.Length; i++)
            {
                Assert.That(_area.Thoughts[i].RuntimeThought.HasSameValues(_dummyThoughts[i]), Is.True);
            }

            _area.RestorePreviousState(RestoreDirection.Next);

            int checksCount = 0;

            for (int i = 0; i < _dummyThoughts.Length; i++)
            {
                if (i != indexToEdit)
                    Assert.That(_area.Thoughts[i].RuntimeThought.HasSameValues(_dummyThoughts[i]), Is.True);
                else
                    Assert.That(_area.Thoughts[i].RuntimeThought.HasSameValues(newTemplate), Is.True);

                checksCount++;
            }

            Assert.That(checksCount, Is.EqualTo(_dummyThoughts.Length));
        }

        [Test]
        public void RestoreAfterInsert()
        {
            AppendDummyThoughts();

            int indexToInsert = 1;

            _area.InsertThought(indexToInsert, "insteredName", "insertedDescription", CharacterSides.Any);

            _area.RestorePreviousState(RestoreDirection.Previous);

            Assert.That(_area.Thoughts, Has.Count.EqualTo(_dummyThoughts.Length));

            for (int i = 0; i < _dummyThoughts.Length; i++)
            {
                Assert.That(_area.Thoughts[i].RuntimeThought.HasSameValues(_dummyThoughts[i]), Is.True);
            }

            _area.RestorePreviousState(RestoreDirection.Next);

            Assert.That(_area.Thoughts, Has.Count.EqualTo(_dummyThoughts.Length + 1));

            for (int i = 0; i < _dummyThoughts.Length; i++)
            {
                if (i == indexToInsert)
                    continue;

                if (i < indexToInsert)
                    Assert.That(_dummyThoughts[i].HasSameValues(_area.Thoughts[i].RuntimeThought), Is.True);
                else
                    Assert.That(_dummyThoughts[i].HasSameValues(_area.Thoughts[i + 1].RuntimeThought), Is.True);
            }
        }

        [Test]
        public void RestoreAfterMove()
        {
            AppendDummyThoughts();

            int indexToMove = 0;
            int targetIndex = 1;

            ThoughtGuid previousAtIndex = _area.Thoughts[targetIndex].RuntimeThought.Guid;
            ThoughtGuid toMove = _area.Thoughts[indexToMove].RuntimeThought.Guid;

            _area.MoveThought(toMove, 1);

            _area.RestorePreviousState(RestoreDirection.Previous);

            for (int i = 0; i < _dummyThoughts.Length; i++)
            {
                Assert.That(_area.Thoughts[i].RuntimeThought.HasSameValues(_dummyThoughts[i]), Is.True);
            }

            _area.RestorePreviousState(RestoreDirection.Next);

            for (int i = 0; i < _dummyThoughts.Length; i++)
            {
                if (i == indexToMove)
                    Assert.That(_area.Thoughts[i].RuntimeThought.Guid, Is.EqualTo(previousAtIndex));
                else if (i == targetIndex)
                    Assert.That(_area.Thoughts[i].RuntimeThought.Guid, Is.EqualTo(toMove));
                else
                    // Guids in templates will never match, so compare keys.
                    Assert.That(_area.Thoughts[i].RuntimeThought.NameKey, Is.EqualTo(_dummyThoughts[i].NameKey));
            }
        }

        [Test]
        public void SelectionReturnsCorrectThought()
        {
            AppendDummyThoughts();

            EditorThought? selected = null;

            _area.OnThoughtSelectionChanged += (t) => { selected = t; };

            _area.SelectThought(_area.Thoughts[0].RuntimeThought.Guid);

            Assert.That(selected!.RuntimeThought.HasSameValues(_area.Thoughts[0].RuntimeThought), Is.True);

            _area.SelectThought(null);

            Assert.That(selected, Is.Null);
        }

        [Test]
        public void SelectingSameIsNoOp()
        {
            int selections = 0;

            _area.OnThoughtSelectionChanged += (_) => { selections++; };

            ThoughtGuid toSelect = _area.AddThought("name", "desc", CharacterSides.Any);

            _area.SelectThought(toSelect);

            Assert.That(selections, Is.EqualTo(1));

            _area.SelectThought(toSelect);

            Assert.That(selections, Is.EqualTo(1));

            _area.SelectThought(null);

            Assert.That(selections, Is.EqualTo(2));

            _area.SelectThought(null);

            Assert.That(selections, Is.EqualTo(2));
        }

        [Test]
        public void SelectionRestored()
        {
            AppendDummyThoughts();

            EditorThought? selected = null;

            _area.OnThoughtSelectionChanged += (t) => { selected = t; };

            _area.SelectThought(_area.Thoughts[0].RuntimeThought.Guid);
            _area.SelectThought(_area.Thoughts[1].RuntimeThought.Guid);

            _area.EditThought(_area.Thoughts[2].RuntimeThought.Guid, "editedName1", "editedDesc1", CharacterSides.Any);

            _area.SelectThought(_area.Thoughts[0].RuntimeThought.Guid);

            _area.EditThought(_area.Thoughts[2].RuntimeThought.Guid, "editedName2", "editedDesc2", CharacterSides.Any);

            _area.RestorePreviousState(RestoreDirection.Previous);

            Assert.That(selected!.RuntimeThought.HasSameValues(_area.Thoughts[1].RuntimeThought), Is.True);

            _area.RestorePreviousState(RestoreDirection.Previous);

            Assert.That(selected, Is.Null);

            _area.RestorePreviousState(RestoreDirection.Next);

            Assert.That(selected!.RuntimeThought.HasSameValues(_area.Thoughts[1].RuntimeThought), Is.True);

            _area.RestorePreviousState(RestoreDirection.Next);

            Assert.That(selected!.RuntimeThought.HasSameValues(_area.Thoughts[0].RuntimeThought), Is.True);
        }

        [Test]
        public void SelectingSameAfterEditIsNoOp()
        {
            int selections = 0;

            _area.OnThoughtSelectionChanged += (_) => { selections++; };

            ThoughtGuid toEdit = _area.AddThought("nameKey", "descriptionKey", CharacterSides.Any);

            _area.SelectThought(toEdit);

            _area.EditThought(toEdit, "changedNameKey", "descriptionKey2", CharacterSides.Tychon);

            _area.SelectThought(toEdit);

            Assert.That(selections, Is.EqualTo(1));
        }

        [Test]
        public void RemoveDeselects()
        {
            EditorThought? selected = null;

            _area.OnThoughtSelectionChanged += (t) => { selected = t; };

            ThoughtGuid toRemove = _area.AddThought("nameKey", "descriptionKey", CharacterSides.Any);

            _area.SelectThought(toRemove);

            Assert.That(selected!.RuntimeThought.Guid, Is.EqualTo(toRemove));

            _area.RemoveThought(toRemove);

            Assert.That(selected, Is.Null);
        }

        // TODO: Restoring the previous selection after undoing a Remove needs more thinking (if needed).
        // Easiest way would be making selections save state. Do we even want that?

        [Test]
        public void SetEditorNote()
        {
            ThoughtGuid toEdit = _area.AddThought("nameKey", "descriptionKey", CharacterSides.Any);

            _area.SetEditorNote(toEdit, "changed note");

            Assert.That(_area.Thoughts[0].EditorNote, Is.EqualTo("changed note"));

            Assert.That(_area.CurrentStateIndex, Is.EqualTo(2));
        }

        [Test]
        public void SetEditorNoteNoChangeNoStateSave()
        {
            ThoughtGuid toEdit = _area.AddThought("nameKey", "descriptionKey", CharacterSides.Any);

            _area.SetEditorNote(toEdit, "changed note");
            _area.SetEditorNote(toEdit, "changed note");

            Assert.That(_area.CurrentStateIndex, Is.EqualTo(2));
        }

        [Test]
        public void RestoreAfterSetEditorNote()
        {
            ThoughtGuid toEdit = _area.AddThought("nameKey", "descriptionKey", CharacterSides.Any);

            _area.SetEditorNote(toEdit, "changed note");

            Assert.That(_area.Thoughts[0].EditorNote, Is.EqualTo("changed note"));

            _area.RestorePreviousState(RestoreDirection.Previous);

            Assert.That(_area.Thoughts[0].EditorNote, Is.EqualTo(string.Empty));
        }

        Thought[] _dummyThoughts =
        [
            // Guids from these are ignored.
            new Thought(new ThoughtGuid(), "thought1", "description1", CharacterSides.Tychon),
            new Thought(new ThoughtGuid(), "thought2", "description2", CharacterSides.Forger),
            new Thought(new ThoughtGuid(), "thought3", "description3", CharacterSides.Any),
        ];

        void AppendDummyThoughts()
        {
            foreach (Thought thought in _dummyThoughts)
            {
                _area.AddThought(thought.NameKey, thought.DescriptionKey, thought.Side);
            }
        }

        private class TestThoughtsEditorArea : ThoughtsEditorArea
        {
            public new int CurrentStateIndex => base.CurrentStateIndex;
        }
    }
}
