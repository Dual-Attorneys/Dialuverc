using Dialuverc.Editor.Base.Verifier;
using DualAttorneys.Dialuverc.Deductions;
using DualAttorneys.Dialuverc.Editor.Deductions.Scratchpads;
using System.Diagnostics;

namespace DualAttorneys.Dialuverc.Editor.Deductions.ThoughtsArea.Verifier
{
    internal class ThoughtsEditorKeysVerifier
    {
        readonly List<KeyDuplicationProblem> _problems = new List<KeyDuplicationProblem>();
        public IReadOnlyList<KeyDuplicationProblem> Problems => _problems;

        ThoughtsEditorState? _lastVerifiedState = null;

        void VerifyScratchpads(ThoughtsEditorState state)
        {
            _problems.Clear();

            VerifyInsideScratchpad(AddEditEnum.Add, state.AddScratchpad);
            VerifyInsideScratchpad(AddEditEnum.Edit, state.EditScratchpad);

            foreach (EditorThought editorThought in state.Thoughts)
            {
                Thought runtimeThought = editorThought.RuntimeThought;

                VerifyScratchpadAgainstThoughts(AddEditEnum.Add, state.AddScratchpad, runtimeThought);

                // While we must check Add against all Thoughts,
                // we must skip the Thought being edited when checking Edit.
                if (runtimeThought.Guid != state.ThoughtSelection)
                    VerifyScratchpadAgainstThoughts(AddEditEnum.Edit, state.EditScratchpad, runtimeThought);
            }
        }

        void VerifyScratchpadAgainstThoughts(AddEditEnum mode, ThoughtsEditorFields? scratchpad, Thought runtimeThought)
        {
            if (scratchpad is null)
                return;

            if (runtimeThought.NameKey == scratchpad.NameKey || runtimeThought.DescriptionKey == scratchpad.NameKey)
                _problems.Add(new KeyDuplicationProblem(ProblemSeverity.Error, $"{mode} scratchpad's {nameof(scratchpad.NameKey)} is already defined.", mode));

            if (runtimeThought.DescriptionKey == scratchpad.DescriptionKey || runtimeThought.NameKey == scratchpad.DescriptionKey)
                _problems.Add(new KeyDuplicationProblem(ProblemSeverity.Error, $"{mode} scratchpad's {nameof(scratchpad.DescriptionKey)} is already defined.", mode));
        }

        void VerifyInsideScratchpad(AddEditEnum mode, ThoughtsEditorFields? scratchpad)
        {
            if (scratchpad is null)
                return;

            if ((scratchpad.NameKey != string.Empty || scratchpad.DescriptionKey != string.Empty) && scratchpad.NameKey == scratchpad.DescriptionKey)
                _problems.Add(new KeyDuplicationProblem(ProblemSeverity.Error, $"{mode} scratchpad's has identical keys for {nameof(scratchpad.NameKey)} and {nameof(scratchpad.DescriptionKey)}", mode));
        }

        public bool CanFinishBuilding(AddEditEnum mode) => _problems.Count(p => p.AffectedScratchpad == mode) == 0;

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
