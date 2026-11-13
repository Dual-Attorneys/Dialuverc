using DualAttorneys.Dialuverc.Deductions;
using DualAttorneys.Dialuverc.Editor.Deductions;
using DualAttorneys.Dialuverc.Editor.Deductions.Scratchpads;
using DualAttorneys.Dialuverc.Editor.Deductions.ThoughtsArea;
using DualAttorneys.Dialuverc.Editor.Deductions.ThoughtsArea.Verifier;
using System.Collections.Immutable;

namespace DualAttorneys.Dialuverc.Tests.Editor.Deductions
{
    internal class ThoughtsEditorKeysVerifierTests
    {
        ThoughtsEditorKeysVerifier _verifier;

        [SetUp]
        public void SetUp()
        {
            _verifier = new ThoughtsEditorKeysVerifier();
        }

        [Test]
        public void DuplicateKeyInsideList()
        {
            ThoughtsEditorState state = new ThoughtsEditorState()
            {
                Thoughts = new List<EditorThought>()
                {
                    new EditorThought(
                        new Thought(new ThoughtGuid(), "addNameKey", "differentDescriptionKey", CharacterSide.Any))
                }.ToImmutableList(),

                AddScratchpad = new ThoughtsEditorFields()
                {
                    NameKey = "addNameKey",
                    DescriptionKey = "addDescriptionKey",
                },

                EditScratchpad = null,

                LocalizationLookup = null!,
                ScratchpadSelection = AddEditEnum.Add,
                ThoughtSelection = null,
            };

            _verifier.Run(state);

            Assert.That(_verifier.Problems.Count(p => p.AffectedScratchpad == AddEditEnum.Add), Is.EqualTo(1));

            state = state with
            {
                Thoughts = new List<EditorThought>()
                {
                    new EditorThought(
                        new Thought(new ThoughtGuid(), "differentNameKey", "addDescriptionKey", CharacterSide.Any))
                }.ToImmutableList(),
            };

            _verifier.Run(state);

            Assert.That(_verifier.Problems.Count(p => p.AffectedScratchpad == AddEditEnum.Add), Is.EqualTo(1));

            state = state with
            {
                Thoughts = new List<EditorThought>()
                {
                    new EditorThought(
                        new Thought(new ThoughtGuid(), "editNameKey", "differentDescriptionKey", CharacterSide.Any))
                }.ToImmutableList(),

                AddScratchpad = null,

                EditScratchpad = new ThoughtsEditorFields()
                {
                    NameKey = "editNameKey",
                    DescriptionKey = "editDescriptionKey",
                },
            };

            _verifier.Run(state);

            Assert.That(_verifier.Problems.Count(p => p.AffectedScratchpad == AddEditEnum.Edit), Is.EqualTo(1));

            state = state with
            {
                Thoughts = new List<EditorThought>()
                {
                    new EditorThought(
                        new Thought(new ThoughtGuid(), "differentNameKey", "editDescriptionKey", CharacterSide.Any))
                }.ToImmutableList(),

                AddScratchpad = null,

                EditScratchpad = new ThoughtsEditorFields()
                {
                    NameKey = "editNameKey",
                    DescriptionKey = "editDescriptionKey",
                },
            };

            _verifier.Run(state);

            Assert.That(_verifier.Problems.Count(p => p.AffectedScratchpad == AddEditEnum.Edit), Is.EqualTo(1));
        }

        [Test]
        public void DuplicateKeysAllowedInEditingScratchpad()
        {
            ThoughtGuid editedThought = new ThoughtGuid();

            ThoughtsEditorState state = new ThoughtsEditorState()
            {
                Thoughts = new List<EditorThought>()
                {
                    new EditorThought(
                        new Thought(editedThought, "editedNameKey", "editedDescriptionKey", CharacterSide.Any))
                }.ToImmutableList(),

                AddScratchpad = null,

                EditScratchpad = new ThoughtsEditorFields()
                {
                    NameKey = "editedNameKey",
                    DescriptionKey = "editedDescriptionKey",
                },

                LocalizationLookup = null!,
                ScratchpadSelection = AddEditEnum.Add,
                ThoughtSelection = editedThought,
            };

            _verifier.Run(state);

            Assert.That(_verifier.Problems, Is.Empty);
        }

        [Test]
        public void DuplicateKeyInsideSameScratchpad()
        {
            // Any state that would create 1 or more Problem(s) works.
            ThoughtsEditorState state = new ThoughtsEditorState()
            {
                Thoughts = ImmutableList<EditorThought>.Empty,

                AddScratchpad = new ThoughtsEditorFields()
                {
                    NameKey = "addNameKey",
                    DescriptionKey = "addNameKey",
                },

                EditScratchpad = null,

                LocalizationLookup = null!,
                ScratchpadSelection = AddEditEnum.Add,
                ThoughtSelection = null,
            };

            _verifier.Run(state);

            Assert.That(_verifier.Problems.Count(p => p.AffectedScratchpad == AddEditEnum.Add), Is.EqualTo(1));

            state = state with
            {
                AddScratchpad = null,

                EditScratchpad = new ThoughtsEditorFields()
                {
                    NameKey = "editNameKey",
                    DescriptionKey = "editNameKey",
                },

                ScratchpadSelection = AddEditEnum.Edit,
            };

            _verifier.Run(state);

            Assert.That(_verifier.Problems.Count(p => p.AffectedScratchpad == AddEditEnum.Edit), Is.EqualTo(1));
        }

        [Test]
        public void ScracthpadProblemsDoNotAffectOther()
        {
            // Any state that would create 1 or more Problem(s) for a single scratchpad works.
            ThoughtsEditorState state = new ThoughtsEditorState()
            {
                Thoughts = ImmutableList<EditorThought>.Empty,

                AddScratchpad = new ThoughtsEditorFields()
                {
                    NameKey = "nameKey",
                    DescriptionKey = "nameKey",
                },

                EditScratchpad = new ThoughtsEditorFields()
                {
                    NameKey = "editNameKey",
                    DescriptionKey = "editDescriptionKey",
                },

                LocalizationLookup = null!,
                ScratchpadSelection = AddEditEnum.Add,
                ThoughtSelection = null,
            };

            _verifier.Run(state);

            Assert.That(_verifier.CanFinishBuilding(AddEditEnum.Add), Is.False);
            Assert.That(_verifier.CanFinishBuilding(AddEditEnum.Edit), Is.True);
        }
    }
}
