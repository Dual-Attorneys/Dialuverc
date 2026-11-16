using BenchmarkDotNet.Attributes;
using Dialuverc.Editor.Base.Observability;
using DualAttorneys.Dialuverc.Deductions;
using DualAttorneys.Dialuverc.Editor.Deductions;
using DualAttorneys.Dialuverc.Editor.Deductions.Scratchpads;
using DualAttorneys.Dialuverc.Editor.Deductions.ThoughtsArea;
using DualAttorneys.Dialuverc.Editor.Deductions.ThoughtsArea.Verifier;
using System.Collections.Immutable;
namespace DualAttorneys.Dialuverc.Benchmarks.EditorAreas
{
    [MemoryDiagnoser]
    public class ThoughtsEditorVerifiersBenchmarks
    {
        [Params(1, 100, 1000)]
        public int ThoughtsAmount;

        ThoughtsEditorKeysVerifier _verifier = new ThoughtsEditorKeysVerifier();

        ImmutableList<EditorThought> _thoughtsList = ImmutableList<EditorThought>.Empty;

        ThoughtsEditorState _noProblemsState = null!;
        ThoughtsEditorState _stateWithProblems = null!;

        [GlobalSetup]
        public void SetUpVerifier()
        {
            _thoughtsList = ImmutableList<EditorThought>.Empty;

            for (int i = 0; i < ThoughtsAmount; i++)
            {
                Thought runtimeThought = new Thought(new ThoughtGuid(), $"nameKey{i}", $"descriptionKey{i}", CharacterSide.Any);

                _thoughtsList = _thoughtsList.Add(new EditorThought(runtimeThought)
                {
                    EditorNote = $"This is an editor note. Make it a little longer. It's for {i}",
                });
            }

            _noProblemsState = new ThoughtsEditorState()
            {
                Thoughts = _thoughtsList,

                AddScratchpad = null,
                EditScratchpad = null,

                ThoughtSelection = null,
                ScratchpadSelection = default,

                LocalizationLookup = null!,
            };

            // This should resemble closely the maximum amount of key-related problems that can exist at once.
            _stateWithProblems = new ThoughtsEditorState()
            {
                Thoughts = _thoughtsList,

                AddScratchpad = new ThoughtsEditorFields()
                {
                    NameKey = "nameKey0",
                    DescriptionKey = "descriptionKey0",
                },

                EditScratchpad = new ThoughtsEditorFields()
                {
                    NameKey = "nameKey0",
                    DescriptionKey = "descriptionKey0",
                },

                // Try to find an index we can edit that is not 0.
                ThoughtSelection = _thoughtsList[_thoughtsList.Count - 1].RuntimeThought.Guid,
                ScratchpadSelection = default,

                LocalizationLookup = null!,
            };
        }

        [Benchmark]
        public void RunKeyVerifierNoProblems()
        {
            _verifier.Clear();

            _verifier.Run(_noProblemsState);
        }

        [Benchmark]
        public void RunKeyVerifierWithProblems()
        {
            _verifier.Clear();

            _verifier.Run(_stateWithProblems);
        }
    }
}
