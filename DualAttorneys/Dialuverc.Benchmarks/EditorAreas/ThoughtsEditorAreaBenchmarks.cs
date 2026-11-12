using BenchmarkDotNet.Attributes;
using Dialuverc.Editor.Base.Observability;
using DualAttorneys.Dialuverc.Deductions;
using DualAttorneys.Dialuverc.Editor.Deductions.Scratchpads;
using DualAttorneys.Dialuverc.Editor.Deductions.ThoughtsArea;

namespace DualAttorneys.Dialuverc.Benchmarks.EditorAreas
{
    [MemoryDiagnoser]
    public class ThoughtsEditorAreaBenchmarks
    {
        [Params(1, 100, 1000)]
        public int ThoughtsAmount;

        ThoughtsEditorArea? _area;

        int _iterationOffset;

        [GlobalSetup]
        public void SetUpArea()
        {
            _iterationOffset = 0;

            _area = new ThoughtsEditorArea(new ObservableValue<string>("it-IT"));
            _area.ScratchpadSelection.ChangeValue(AddEditEnum.Add);

            for (int i = 0; i < ThoughtsAmount; i++)
            {
                FillThought(i);
                _area.FinishBuilding();
            }
        }

        [Benchmark]
        public void AddAndDeleteThought()
        {
            _area!.ScratchpadSelection.ChangeValue(AddEditEnum.Add);

            _area.RemoveThought(BuildThought());
        }

        [Benchmark]
        public void EditThought()
        {
            _area!.ScratchpadSelection.ChangeValue(AddEditEnum.Edit);

            _area.SelectThought(_area.Thoughts[0].RuntimeThought.Guid);

            BuildThought();
        }

        ThoughtGuid BuildThought()
        {
            FillThought(ThoughtsAmount + 1 + _iterationOffset);

            _iterationOffset++;

            return _area!.FinishBuilding();
        }

        void FillThought(int id)
        {
            _area!.SetNameKey($"nameKey{id}");
            _area.SetDescriptionKey($"descriptionKey{id}");

            _area.SetEditorNote($"This is an editor note. Make it a little longer. It's for {id}");

            _area.SetNameLocalization($"This is {id}'s name translation");
            _area.SetDescriptionLocalization($"This is {id}'s description translation");
        }
    }
}
