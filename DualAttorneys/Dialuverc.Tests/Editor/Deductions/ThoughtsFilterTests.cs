using Dialuverc.Editor.Base.Localization;
using Dialuverc.Editor.Base.Observability;
using DualAttorneys.Dialuverc.Deductions;
using DualAttorneys.Dialuverc.Editor.Deductions;

namespace DualAttorneys.Dialuverc.Tests.Editor.Deductions
{
    internal class ThoughtsFilterTests
    {
        LocalizationStore _localizationStore;

        ThoughtsFilter _filter;

        [SetUp]
        public void SetUp()
        {
            _localizationStore = new LocalizationStore(new ObservableValue<string>("it-IT"), "test.thoughtsFilter");

            _filter = new ThoughtsFilter(_localizationStore);
        }

        [Test]
        public void SideFilteringNotStrict()
        {
            Thought tychonThought = new Thought(new ThoughtGuid(), "_", "_", CharacterSides.Tychon);
            Thought forgerThought = new Thought(new ThoughtGuid(), "_", "_", CharacterSides.Forger);
            Thought anyThought = new Thought(new ThoughtGuid(), "_", "_", CharacterSides.Any);

            _filter.SideFilter = CharacterSides.Tychon;

            Assert.That(_filter.PassesAll(tychonThought), Is.True);
            Assert.That(_filter.PassesAll(forgerThought), Is.False);
            Assert.That(_filter.PassesAll(anyThought), Is.True);

            _filter.SideFilter = CharacterSides.Forger;

            Assert.That(_filter.PassesAll(tychonThought), Is.False);
            Assert.That(_filter.PassesAll(forgerThought), Is.True);
            Assert.That(_filter.PassesAll(anyThought), Is.True);
        }

        [Test]
        public void SideFilteringStrict()
        {
            Thought tychonThought = new Thought(new ThoughtGuid(), "_", "_", CharacterSides.Tychon);
            Thought forgerThought = new Thought(new ThoughtGuid(), "_", "_", CharacterSides.Forger);
            Thought anyThought = new Thought(new ThoughtGuid(), "_", "_", CharacterSides.Any);

            _filter.Strict = true;

            _filter.SideFilter = CharacterSides.Tychon;

            Assert.That(_filter.PassesAll(tychonThought), Is.True);
            Assert.That(_filter.PassesAll(forgerThought), Is.False);
            Assert.That(_filter.PassesAll(anyThought), Is.False);

            _filter.SideFilter = CharacterSides.Forger;

            Assert.That(_filter.PassesAll(tychonThought), Is.False);
            Assert.That(_filter.PassesAll(forgerThought), Is.True);
            Assert.That(_filter.PassesAll(anyThought), Is.False);

            _filter.SideFilter = CharacterSides.Any;

            Assert.That(_filter.PassesAll(tychonThought), Is.False);
            Assert.That(_filter.PassesAll(forgerThought), Is.False);
            Assert.That(_filter.PassesAll(anyThought), Is.True);
        }

        [Test]
        public void TextFileringFullMatch()
        {
            _localizationStore.AddOrSetValue("key1", "value1");
            _localizationStore.AddOrSetValue("key2", "value2");

            Thought thoughtA = new Thought(new ThoughtGuid(), "key1", "key2", CharacterSides.Any);
            Thought thoughtB = new Thought(new ThoughtGuid(), "key2", "key1", CharacterSides.Any);

            _filter.TextFilter = "value1";

            Assert.That(_filter.PassesAll(thoughtA), Is.True);
            Assert.That(_filter.PassesAll(thoughtB), Is.True);
        }

        [Test]
        public void TextFileringPartialMatch()
        {
            _localizationStore.AddOrSetValue("key1", "value1");
            _localizationStore.AddOrSetValue("key2", "value2");

            Thought thoughtA = new Thought(new ThoughtGuid(), "key1", "key2", CharacterSides.Any);
            Thought thoughtB = new Thought(new ThoughtGuid(), "key2", "key1", CharacterSides.Any);

            _filter.TextFilter = "val";

            Assert.That(_filter.PassesAll(thoughtA), Is.True);
            Assert.That(_filter.PassesAll(thoughtB), Is.True);
        }
    }
}
