using System.Diagnostics.CodeAnalysis;
using Dialuverc.Editor.Base.Localization;
using Dialuverc.Editor.Base.Observability;
using Dialuverc.Editor.Base.Verifier;
using DualAttorneys.Dialuverc.Deductions;
using DualAttorneys.Dialuverc.Editor.Deductions;
using DualAttorneys.Dialuverc.Editor.Deductions.Scratchpads;

namespace DualAttorneys.Dialuverc.Tests.Editor.Deductions
{
    internal class ThoughtsEditorVerifierTests
    {
        ObservableValue<AddEditEnum> _scratchpadSelection;
        AddEditScratchpads<ThoughtsEditorFields> _scratchpads;
        List<EditorThought> _thoughts;

        TestLocalizationStore _localizationStore;

        [SetUp]
        public void SetUp()
        {
            _scratchpadSelection = new ObservableValue<AddEditEnum>(AddEditEnum.Add);
            _scratchpads = new AddEditScratchpads<ThoughtsEditorFields>(_scratchpadSelection);
            _thoughts = new List<EditorThought>();

            _localizationStore = new TestLocalizationStore();
        }

        [Test]
        public void FindsAllDuplicateKeys()
        {
            void AssertForNFields(int fields)
            {
                IReadOnlyList<Problem> foundProblems = ThoughtsEditorVerifier.Run(_scratchpads, null, _thoughts, _localizationStore);

                Assert.That(foundProblems, Has.Count.EqualTo(fields));
                Assert.That(foundProblems.Count(p => p.Severity == ProblemSeverity.Error), Is.EqualTo(fields));
                Assert.That(foundProblems.Count(p => p.Description.Contains("index 1.")), Is.EqualTo(fields));
            }

            _thoughts.Add(new EditorThought(new Thought(new ThoughtGuid(), "singletonNameKey", "singletonDescriptionKey", CharacterSide.Any)));
            _thoughts.Add(new EditorThought(new Thought(new ThoughtGuid(), "nameKey", "descriptionKey", CharacterSide.Any)));

            _localizationStore.Lookup.Add("singletonNameKey", "value");
            _localizationStore.Lookup.Add("singletonDescriptionKey", "value");
            _localizationStore.Lookup.Add("nameKey", "value");
            _localizationStore.Lookup.Add("descriptionKey", "value");

            _scratchpads.AddScratchpad = new ThoughtsEditorFields
            {
                NameKey = "nameKey",
            };

            AssertForNFields(1);

            _scratchpads.AddScratchpad = _scratchpads.AddScratchpad with
            {
                DescriptionKey = "descriptionKey",
            };

            AssertForNFields(2);

            _scratchpads.EditScratchpad = new ThoughtsEditorFields
            {
                NameKey = "nameKey",
            };

            AssertForNFields(3);

            _scratchpads.EditScratchpad = _scratchpads.EditScratchpad with
            {
                DescriptionKey = "descriptionKey",
            };

            AssertForNFields(4);
        }

        [Test]
        public void EditedThoughtNotCountedForDuplication()
        {
            ThoughtGuid editedGuid = new ThoughtGuid();

            _thoughts.Add(new EditorThought(new Thought(editedGuid, "nameKey", "descriptionKey", CharacterSide.Any)));

            _localizationStore.Lookup.Add("nameKey", "value");
            _localizationStore.Lookup.Add("descriptionKey", "value");

            _scratchpads.EditScratchpad = new ThoughtsEditorFields()
            {
                NameKey = "nameKey",
                DescriptionKey = "descriptionKey",
            };

            IReadOnlyList<Problem> foundProblems = ThoughtsEditorVerifier.Run(_scratchpads, editedGuid, _thoughts, _localizationStore);

            Assert.That(foundProblems, Is.Empty);

            foundProblems = ThoughtsEditorVerifier.Run(_scratchpads, new ThoughtGuid(), _thoughts, _localizationStore);

            Assert.That(foundProblems, Is.Not.Empty);
        }

        // Mocks a localization store for a single locale.
        private class TestLocalizationStore : IReadOnlyLocalizationStore
        {
            readonly public Dictionary<string, string> Lookup = new Dictionary<string, string>();

            public event EventHandler<DictionaryChangeEventArgs<string, ICollection<KeyValuePair<string, string?>>>>? CollectionChanged;

            public bool TryGetValue(string key, [NotNullWhen(true)] out string? value) => Lookup.TryGetValue(key, out value);
        }
    }
}
