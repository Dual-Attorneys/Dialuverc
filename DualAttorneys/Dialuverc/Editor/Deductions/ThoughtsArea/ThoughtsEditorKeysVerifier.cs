using Dialuverc.Editor.Base.Verifier;
using DualAttorneys.Dialuverc.Deductions;
using DualAttorneys.Dialuverc.Editor.Deductions.Scratchpads;
using System.Collections.Immutable;

namespace DualAttorneys.Dialuverc.Editor.Deductions.ThoughtsArea
{
    internal class ThoughtsEditorKeysVerifier
    {
        readonly AddEditScratchpads<ThoughtsEditorFields> _scratchpadsManager;

        readonly List<Problem> _problems = new List<Problem>();

        public bool CanFinishBuilding => _problems.Count == 0;

        public ThoughtsEditorKeysVerifier(AddEditScratchpads<ThoughtsEditorFields> scratchpadsManager)
        {
            _scratchpadsManager = scratchpadsManager;
        }

        public void VerifyScratchpads(ThoughtGuid? ignore, ImmutableList<EditorThought> thoughts)
        {
            _problems.Clear();

            foreach (EditorThought editorThought in thoughts)
            {
                Thought runtimeThought = editorThought.RuntimeThought;

                VerifyIndividualScratchpad(AddEditEnum.Add, _scratchpadsManager.AddScratchpad, runtimeThought);

                // While we must check Add against all Thoughts,
                // we must skip the Thought being edited when checking Edit.
                if (runtimeThought.Guid != ignore)
                    VerifyIndividualScratchpad(AddEditEnum.Edit, _scratchpadsManager.EditScratchpad, runtimeThought);
            }
        }

        void VerifyIndividualScratchpad(AddEditEnum mode, ThoughtsEditorFields? scratchpad, Thought runtimeThought)
        {
            if (scratchpad is null)
                return;

            if (runtimeThought.NameKey == scratchpad.NameKey || runtimeThought.DescriptionKey == scratchpad.NameKey)
                _problems.Add(new Problem(ProblemSeverity.Error, $"{mode} scratchpad's {nameof(scratchpad.NameKey)} is already defined."));

            if (runtimeThought.DescriptionKey == scratchpad.DescriptionKey || runtimeThought.NameKey == scratchpad.DescriptionKey)
                _problems.Add(new Problem(ProblemSeverity.Error, $"{mode} scratchpad's {nameof(scratchpad.DescriptionKey)} is already defined."));
        }

        // This check is run internally when a field representing a key is changed.
        public IReadOnlyList<Problem> Run() => _problems;
    }
}
