using Dialuverc.Editor.Base.Verifier;
using DualAttorneys.Dialuverc.Deductions;
using DualAttorneys.Dialuverc.Editor.Deductions.Scratchpads;
using System.Diagnostics;

namespace DualAttorneys.Dialuverc.Editor.Deductions.ThoughtsArea
{
    internal class ThoughtsEditorKeysVerifier
    {
        readonly List<Problem> _problems = new List<Problem>();

        ThoughtsEditorState? _lastVerifiedState = null;

        public bool CanFinishBuilding => _problems.Count == 0;

        void VerifyScratchpads(ThoughtsEditorState state)
        {
            _problems.Clear();

            foreach (EditorThought editorThought in state.Thoughts)
            {
                Thought runtimeThought = editorThought.RuntimeThought;

                VerifyIndividualScratchpad(AddEditEnum.Add, state.AddScratchpad, runtimeThought);

                // While we must check Add against all Thoughts,
                // we must skip the Thought being edited when checking Edit.
                if (runtimeThought.Guid != state.ThoughtSelection)
                    VerifyIndividualScratchpad(AddEditEnum.Edit, state.EditScratchpad, runtimeThought);
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

        public IReadOnlyList<Problem> Run(ThoughtsEditorState state)
        {
            // TODO: Should last saved state be nullable? A null default state *could* be allowable.
            // In our usage should never be null anyway.
            Debug.Assert(state is not null, $"{nameof(state)} was null. This should never happen.");

            if (state != _lastVerifiedState)
                VerifyScratchpads(state);

            _lastVerifiedState = state;

            return _problems;
        }
    }
}
