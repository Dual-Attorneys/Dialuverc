using Dialuverc.Deductions;
using Dialuverc.Editor.Base;
using Dialuverc.Editor.Deductions;

namespace Dialuverc.Editor.Tests.Deductions
{
    internal class ThoughtsEditorAreaTests
    {
        ThoughtsEditorArea _area;

        [SetUp]
        public void SetUp()
        {
            _area = new ThoughtsEditorArea();
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

            Thought thought1 = new Thought(new Guid(), "thought1", "description1", CharacterSides.Tychon);
            Thought thought2 = new Thought(new Guid(), "thought2", "description2", CharacterSides.Forger);

            Guid guid1 = _area.AddThought(thought1.NameKey, thought1.DescriptionKey, thought1.Side);
            Guid guid2 = _area.AddThought(thought2.NameKey, thought2.DescriptionKey, thought2.Side);

            expectedJson = String.Format(expectedJson, guid1, guid2);

            Assert.That(_area.Thoughts.Count, Is.EqualTo(2));

            Assert.That(_area.SerializeForExport(), Is.EqualTo(expectedJson));
        }

        [Test]
        public void AppendThought()
        {
            _area.AddThought("thought1", "description1", CharacterSides.Tychon);

            Assert.That(_area.Thoughts.Count, Is.EqualTo(1));

            Thought firstThought = _area.Thoughts[0];

            Assert.That(firstThought.NameKey, Is.EqualTo("thought1"));
            Assert.That(firstThought.DescriptionKey, Is.EqualTo("description1"));
            Assert.That(firstThought.Side, Is.EqualTo(CharacterSides.Tychon));

            _area.AddThought("thought2", "description2", CharacterSides.Forger);

            Assert.That(_area.Thoughts.Count, Is.EqualTo(2));

            Thought _secondThought = _area.Thoughts[1];

            Assert.That(_secondThought.NameKey, Is.EqualTo("thought2"));
            Assert.That(_secondThought.DescriptionKey, Is.EqualTo("description2"));
            Assert.That(_secondThought.Side, Is.EqualTo(CharacterSides.Forger));
        }

        [Test]
        public void RemoveThought()
        {
            _area.AddThought("thought1", "description1", CharacterSides.Tychon);
            Guid guidToRemove = _area.AddThought("thought2", "description2", CharacterSides.Forger);
            _area.AddThought("thought3", "description3", CharacterSides.Any);

            _area.RemoveThought(guidToRemove);

            Assert.That(_area.Thoughts.Count, Is.EqualTo(2));

            Assert.That(_area.Thoughts[0].NameKey, Is.EqualTo("thought1"));
            Assert.That(_area.Thoughts[1].NameKey, Is.EqualTo("thought3"));
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

            Guid guid = _area.AddThought("starterNameKey", "starterDescriptionKey", CharacterSides.Tychon);
            _area.AddThought("thought2", "description2", CharacterSides.Any);
            _area.AddThought("thought3", "description3", CharacterSides.Any);
            _area.AddThought("thought4", "description4", CharacterSides.Any);

            _area.EditThought(guid, "newNameKey", "newDescriptionKey", CharacterSides.Forger);

            Assert.That(_area.Thoughts.Count, Is.EqualTo(4));

            Assert.That(_area.Thoughts[0].Guid, Is.EqualTo(guid));
            Assert.That(_area.Thoughts[0].NameKey, Is.EqualTo("newNameKey"));
            Assert.That(_area.Thoughts[0].DescriptionKey, Is.EqualTo("newDescriptionKey"));
            Assert.That(_area.Thoughts[0].Side, Is.EqualTo(CharacterSides.Forger));
        
            for (int i = 1; i < _area.Thoughts.Count; i++)
            {
                Assert.That(_area.Thoughts[i].NameKey == thoughtNames[i]);
            }
        }

        [Test]
        public void InsertThought()
        {
            AppendTemplatesToList();

            int indexToInsert = 1;

            Guid inserted = _area.InsertThought(indexToInsert, "insertedName", "insertedDescription", CharacterSides.Any);

            Assert.That(_area.Thoughts, Has.Count.EqualTo(_thoughtTemplates.Length + 1));

            for (int i = 0; i < _thoughtTemplates.Length; i++)
            {
                if (i == indexToInsert)
                    continue;

                if (i < indexToInsert)
                    Assert.That(ThoughtsAreEqual(_thoughtTemplates[i], _area.Thoughts[i]));
                else
                    Assert.That(ThoughtsAreEqual(_thoughtTemplates[i], _area.Thoughts[i + 1]));
            }

            Assert.That(_area.Thoughts[indexToInsert].NameKey, Is.EqualTo("insertedName"));
            Assert.That(_area.Thoughts[indexToInsert].DescriptionKey, Is.EqualTo("insertedDescription"));
            Assert.That(_area.Thoughts[indexToInsert].Side, Is.EqualTo(CharacterSides.Any));
        }

        [Test]
        public void MoveThought()
        {
            AppendTemplatesToList();

            int newIndex = 1;

            Guid toMove = _area.Thoughts[0].Guid;
            Guid previousAtIndex = _area.Thoughts[newIndex].Guid;

            _area.MoveThought(toMove, newIndex);

            Assert.That(_area.Thoughts[newIndex - 1].Guid, Is.EqualTo(previousAtIndex));
            Assert.That(_area.Thoughts[newIndex].Guid, Is.EqualTo(toMove));
        }

        [Test]
        public void RestoreAfterAppend()
        {
            AppendTemplatesToList();

            int startingCount = _area.Thoughts.Count;

            for (int i = 0; i < startingCount; i++)
            {
                Assert.That(ThoughtsAreEqual(_area.Thoughts[i], _thoughtTemplates[i]), Is.True);
            }

            while (_area.CanUndo)
            {
                _area.RestorePreviousState(RestoreDirection.Previous);

                int indexToCheck = _area.Thoughts.Count - 1;

                // We'll hit an empty list on the last undo.
                if (_area.Thoughts.Count < 1)
                    continue;

                Thought a = _area.Thoughts[indexToCheck];
                Thought b = _thoughtTemplates[indexToCheck];

                Assert.That(ThoughtsAreEqual(a, b), Is.True);
            }

            Assert.That(_area.Thoughts, Has.Count.EqualTo(0));

            while (_area.CanRedo)
            {
                _area.RestorePreviousState(RestoreDirection.Next);

                int indexToCheck = _area.Thoughts.Count - 1;

                Thought a = _area.Thoughts[indexToCheck];
                Thought b = _thoughtTemplates[indexToCheck];

                Assert.That(ThoughtsAreEqual(a, b), Is.True);
            }

            Assert.That(_area.Thoughts, Has.Count.EqualTo(startingCount));
        }

        [Test]
        public void RestoreAfterRemoveFromFront()
        {
            AppendTemplatesToList();

            int starterCount = _area.Thoughts.Count;

            // Undoing until we can't anymore will include the template appends.
            // Count how many undos are needed to ONLY undo the removes.
            int amountToRedo = 0;

            for (int i = _thoughtTemplates.Length - 1; i >= 0; i--)
            {
                _area.RemoveThought(
                    _area.Thoughts.First(t => t.NameKey == _thoughtTemplates[i].NameKey).Guid);

                amountToRedo++;
            }

            Assert.That(_area.Thoughts, Has.Count.EqualTo(0));

            Assert.That(amountToRedo, Is.EqualTo(3));

            for (int i = 0; i < amountToRedo; i++) 
            {
                _area.RestorePreviousState(RestoreDirection.Previous);

                int indexToCheck =  _area.Thoughts.Count - 1;

                Thought a = _area.Thoughts[indexToCheck];
                Thought b = _thoughtTemplates[indexToCheck];

                Assert.That(ThoughtsAreEqual(a, b), Is.True);
            }

            Assert.That(_area.Thoughts, Has.Count.EqualTo(starterCount));

            while (_area.CanRedo)
            {
                _area.RestorePreviousState(RestoreDirection.Next);

                int indexToCheck = _area.Thoughts.Count - 1;

                // We'll hit an empty list on the last redo.
                if (_area.Thoughts.Count < 1)
                    continue;

                Thought a = _area.Thoughts[indexToCheck];
                Thought b = _thoughtTemplates[indexToCheck];

                Assert.That(ThoughtsAreEqual(a, b), Is.True);
            }

            Assert.That(_area.Thoughts, Has.Count.EqualTo(0));
        }

        [Test]
        public void RestoreAfterEdit()
        {
            AppendTemplatesToList();

            int indexToEdit = (int)(_thoughtTemplates.Length / 2f);

            Guid guid = _area.Thoughts.First(t => t.NameKey == _thoughtTemplates[indexToEdit].NameKey).Guid;

            Thought newTemplate = new Thought(new Guid(), "NewKey", "NewDescription", CharacterSides.Any);

            _area.EditThought(guid, newTemplate.NameKey, newTemplate.DescriptionKey, newTemplate.Side);

            _area.RestorePreviousState(RestoreDirection.Previous);

            Assert.That(_area.Thoughts, Has.Count.EqualTo(_thoughtTemplates.Length));

            for (int i = 0; i < _thoughtTemplates.Length; i++)
            {
                Assert.That(ThoughtsAreEqual(_area.Thoughts[i], _thoughtTemplates[i]));
            }

            _area.RestorePreviousState(RestoreDirection.Next);

            int checksCount = 0;

            for (int i = 0; i < _thoughtTemplates.Length; i++)
            {
                if (i != indexToEdit)
                    Assert.That(ThoughtsAreEqual(_area.Thoughts[i], _thoughtTemplates[i]));
                else
                    Assert.That(ThoughtsAreEqual(_area.Thoughts[i], newTemplate));

                checksCount++;
            }

            Assert.That(checksCount, Is.EqualTo(_thoughtTemplates.Length));
        }

        [Test]
        public void RestoreAfterInsert()
        {
            AppendTemplatesToList();

            int indexToInsert = 1;

            _area.InsertThought(indexToInsert, "insteredName", "insertedDescription", CharacterSides.Any);

            _area.RestorePreviousState(RestoreDirection.Previous);

            Assert.That(_area.Thoughts, Has.Count.EqualTo(_thoughtTemplates.Length));

            for (int i = 0; i < _thoughtTemplates.Length; i++)
            {
                Assert.That(ThoughtsAreEqual(_area.Thoughts[i], _thoughtTemplates[i]));
            }

            _area.RestorePreviousState(RestoreDirection.Next);

            Assert.That(_area.Thoughts, Has.Count.EqualTo(_thoughtTemplates.Length + 1));

            for (int i = 0; i < _thoughtTemplates.Length; i++)
            {
                if (i == indexToInsert)
                    continue;

                if (i < indexToInsert)
                    Assert.That(ThoughtsAreEqual(_thoughtTemplates[i], _area.Thoughts[i]));
                else
                    Assert.That(ThoughtsAreEqual(_thoughtTemplates[i], _area.Thoughts[i + 1]));
            }
        }

        [Test]
        public void RestoreAfterMove()
        {
            AppendTemplatesToList();

            int indexToMove = 0;
            int targetIndex = 1; 

            Guid previousAtIndex = _area.Thoughts[targetIndex].Guid;
            Guid toMove = _area.Thoughts[indexToMove].Guid;

            _area.MoveThought(toMove, 1);

            _area.RestorePreviousState(RestoreDirection.Previous);

            for (int i = 0; i < _thoughtTemplates.Length; i++)
            {
                Assert.That(ThoughtsAreEqual(_area.Thoughts[i], _thoughtTemplates[i]));
            }

            _area.RestorePreviousState(RestoreDirection.Next);

            for (int i = 0; i < _thoughtTemplates.Length; i++)
            {
                if (i == indexToMove)
                    Assert.That(_area.Thoughts[i].Guid, Is.EqualTo(previousAtIndex));
                else if (i == targetIndex)
                    Assert.That(_area.Thoughts[i].Guid, Is.EqualTo(toMove));
                else
                    // Guids in templates will never match, so compare keys.
                    Assert.That(_area.Thoughts[i].NameKey, Is.EqualTo(_thoughtTemplates[i].NameKey));
            }
        }

        [Test]
        public void SelectionReturnsCorrectThought()
        {
            AppendTemplatesToList();

            Thought? selected = null;

            _area.OnThoughtSelectionChanged += (t) => { selected = t; };

            _area.SelectThought(_area.Thoughts[0].Guid);

            Assert.That(ThoughtsAreEqual(selected!, _area.Thoughts[0]), Is.True);

            _area.SelectThought(Guid.Empty);

            Assert.That(selected, Is.Null);
        }

        [Test]
        public void SelectingSameIsNoOp()
        {
            int selections = 0;

            _area.OnThoughtSelectionChanged += (_) => { selections++; };

            Guid toSelect = _area.AddThought("name", "desc", CharacterSides.Any);

            _area.SelectThought(toSelect);

            Assert.That(selections, Is.EqualTo(1));

            _area.SelectThought(toSelect);

            Assert.That(selections, Is.EqualTo(1));

            _area.SelectThought(Guid.Empty);

            Assert.That(selections, Is.EqualTo(2));

            _area.SelectThought(Guid.Empty);

            Assert.That(selections, Is.EqualTo(2));
        }

        [Test]
        public void SelectionRestored()
        {
            AppendTemplatesToList();

            Thought? selected = null;

            _area.OnThoughtSelectionChanged += (t) => { selected = t; };

            _area.SelectThought(_area.Thoughts[0].Guid);
            _area.SelectThought(_area.Thoughts[1].Guid);

            _area.EditThought(_area.Thoughts[2].Guid, "editedName1", "editedDesc1", CharacterSides.Any);

            _area.SelectThought(_area.Thoughts[0].Guid);

            _area.EditThought(_area.Thoughts[2].Guid, "editedName2", "editedDesc2", CharacterSides.Any);

            _area.RestorePreviousState(RestoreDirection.Previous);

            Assert.That(ThoughtsAreEqual(selected!, _area.Thoughts[1]), Is.True);

            _area.RestorePreviousState(RestoreDirection.Previous);

            Assert.That(selected, Is.Null);

            _area.RestorePreviousState(RestoreDirection.Next);

            Assert.That(ThoughtsAreEqual(selected!, _area.Thoughts[1]), Is.True);

            _area.RestorePreviousState(RestoreDirection.Next);

            Assert.That(ThoughtsAreEqual(selected!, _area.Thoughts[0]), Is.True);
        }

        Thought[] _thoughtTemplates =
        [
            // Guids from the templates are ignored.
            new Thought(new Guid(), "thought1", "description1", CharacterSides.Tychon),
            new Thought(new Guid(), "thought2", "description2", CharacterSides.Forger),
            new Thought(new Guid(), "thought3", "description3", CharacterSides.Any),
        ];

        void AppendTemplatesToList()
        {
            foreach (Thought thought in _thoughtTemplates)
            {
                _area.AddThought(thought.NameKey, thought.DescriptionKey, thought.Side);
            }
        }

        bool ThoughtsAreEqual(Thought a, Thought b)
        {
            return a.NameKey == b.NameKey &&
                a.DescriptionKey == b.DescriptionKey &&
                a.Side == b.Side;
        }
    }
}
