using BenchmarkDotNet.Attributes;
using Dialuverc.Editor.Base.Observability;
using DualAttorneys.Dialuverc.Deductions;
using DualAttorneys.Dialuverc.Editor.Deductions;
using DualAttorneys.Dialuverc.Editor.Deductions.Scratchpads;
using DualAttorneys.Dialuverc.Editor.Deductions.ThoughtsArea;
using System.Collections.Immutable;
namespace DualAttorneys.Dialuverc.Benchmarks.EditorAreas
{
    [MemoryDiagnoser]
    public class ThoughtsEditorVerifiersBenchmarks
    {
        [Params(1, 100, 1000)]
        public int ThoughtsAmount;

        AddEditScratchpads<ThoughtsEditorFields>? _scratchpads;
        ThoughtsEditorKeysVerifier? _verifier;
        ImmutableList<EditorThought>? _thoughtsList;


        [GlobalSetup]
        public void SetUpVerifier()
        {
            _thoughtsList = ImmutableList<EditorThought>.Empty;

            _scratchpads = new AddEditScratchpads<ThoughtsEditorFields>(new ObservableValue<AddEditEnum>(AddEditEnum.Add));
            _verifier = new ThoughtsEditorKeysVerifier();

            for (int i = 0; i < ThoughtsAmount; i++)
            {
                Thought runtimeThought = new Thought(new ThoughtGuid(), $"nameKey{i}", $"descriptionKey{i}", CharacterSide.Any);

                _thoughtsList = _thoughtsList.Add(new EditorThought(runtimeThought)
                {
                    EditorNote = $"This is an editor note. Make it a little longer. It's for {i}",
                });
            }
        }

        [Benchmark]
        public void RunKeyVerifier()
        {
            // TODO: Run verifier.
        }
    }
}
