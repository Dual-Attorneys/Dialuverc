using BenchmarkDotNet.Attributes;
using Dialuverc.Editor.Base;
using DualAttorneys.Dialuverc.Editor.Deductions;

using static Dialuverc.Editor.Base.Modes.EditorModeManager;

namespace DualAttorneys.Dialuverc.Benchmarks.EditorAreas
{
    [MemoryDiagnoser]
    public class ThoughtsEditorAreaBenchmark
    {
        // This value was agreed upon as it's probably many more than will be made in practice.
        const int _baseAmountOfThoughts = 200;

        ThoughtsEditorArea _area;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _area = new ThoughtsEditorArea();

            _area.ScratchpadManager.ChangeMode(Mode.Add);

            for (int i = 0; i < _baseAmountOfThoughts; i++)
            {
                _area.SetNameKey($"nameKey{i}");
                _area.SetDescriptionKey($"descKey{i}");
                _area.SetEditorNote($"editorNote{i}");
                _area.FinishBuilding();
            }
        }

        [Benchmark]
        public void UndoThenRedo()
        {
            _area.RestorePreviousState(RestoreDirection.Previous);

            _area.RestorePreviousState(RestoreDirection.Next);
        }

        int _stateSerializationCounter = 0;

        // All list operations in this area are similar.
        // This one is the easiest to use for benchmarking and still triggers a full state save.
        [Benchmark]
        public void StateSave()
        {
            
        }

        // Making enough Edits to make Verify have a significantly higher impact than it would with no problems found
        // would likely take up most of the time and memory spent in the benchmark.
        // 0-problem-verifies are the most common case and having more than a few issues is not realistic anyway.
        [Benchmark]
        public void Verify()
        {
            _area.Verify();
        }
    }
}
