using Dialuverc.Deductions;
using Dialuverc.Editor.Deductions;

namespace Dialuverc.Editor.Tests.Deductions
{
    internal class ThoughtsFilterTests
    {
        ThoughtsFilter _filter;

        Thought _tychonThought = new Thought(Guid.NewGuid(), "nameKeyOne", "descriptionKeyOne", CharacterSides.Tychon);
        Thought _forgerThought = new Thought(Guid.NewGuid(), "nameKeyTwo", "descriptionKeyTwo", CharacterSides.Forger);
        Thought _anyThought = new Thought(Guid.NewGuid(), "nameKeyThree", "descriptionKeyThree", CharacterSides.Any);

        [SetUp]
        public void SetUp()
        {
            _filter = new ThoughtsFilter();
        }

        [Test]
        public void AllPassWithNoFilter()
        {
            Assert.That(_filter.PassesAll(_tychonThought), Is.True);
            Assert.That(_filter.PassesAll(_forgerThought), Is.True);
            Assert.That(_filter.PassesAll(_anyThought), Is.True);
        }

        [Test]
        public void AnySidePassesNotStrict()
        {
            _filter.SideFilter = CharacterSides.Tychon;

            Assert.That(_filter.PassesAll(_tychonThought), Is.True);
            Assert.That(_filter.PassesAll(_forgerThought), Is.False);
            Assert.That(_filter.PassesAll(_anyThought), Is.True);

            _filter.SideFilter = CharacterSides.Forger;

            Assert.That(_filter.PassesAll(_tychonThought), Is.False);
            Assert.That(_filter.PassesAll(_forgerThought), Is.True);
            Assert.That(_filter.PassesAll(_anyThought), Is.True);
        }

        [Test]
        public void AnySideDoesNotPassStrict()
        {
            _filter.Strict = true;

            _filter.SideFilter = CharacterSides.Tychon;

            Assert.That(_filter.PassesAll(_tychonThought), Is.True);
            Assert.That(_filter.PassesAll(_forgerThought), Is.False);
            Assert.That(_filter.PassesAll(_anyThought), Is.False);

            _filter.SideFilter = CharacterSides.Forger;

            Assert.That(_filter.PassesAll(_tychonThought), Is.False);
            Assert.That(_filter.PassesAll(_forgerThought), Is.True);
            Assert.That(_filter.PassesAll(_anyThought), Is.False);

            _filter.SideFilter = CharacterSides.Any;

            Assert.That(_filter.PassesAll(_tychonThought), Is.False);
            Assert.That(_filter.PassesAll(_forgerThought), Is.False);
            Assert.That(_filter.PassesAll(_anyThought), Is.True);
        }

        [Test]
        public void AllPassWithBlankOrNullString()
        {
            _filter.NameKeyFilter = "       ";

            Assert.That(_filter.PassesAll(_tychonThought), Is.True);
            Assert.That(_filter.PassesAll(_forgerThought), Is.True);
            Assert.That(_filter.PassesAll(_anyThought), Is.True);

            _filter.NameKeyFilter = null;

            Assert.That(_filter.PassesAll(_tychonThought), Is.True);
            Assert.That(_filter.PassesAll(_forgerThought), Is.True);
            Assert.That(_filter.PassesAll(_anyThought), Is.True);
        }

        [Test]
        public void StringIsCaseInsensitive()
        {
            Assert.That(_forgerThought.NameKey.All(c => char.IsUpper(c)), Is.False);

            _filter.NameKeyFilter = _forgerThought.NameKey.ToUpper();

            Assert.That(_filter.PassesAll(_tychonThought), Is.False);
            Assert.That(_filter.PassesAll(_forgerThought), Is.True);
            Assert.That(_filter.PassesAll(_anyThought), Is.False);
        }

        [Test]
        public void PartialStringMatchPasses()
        {
            _filter.NameKeyFilter = "One";

            Assert.That(_filter.PassesAll(_tychonThought), Is.True);
            Assert.That(_filter.PassesAll(_forgerThought), Is.False);
            Assert.That(_filter.PassesAll(_anyThought), Is.False);
        }

        [Test]
        public void NoPassFilterCombo()
        {
            _filter.NameKeyFilter = "Two";
            _filter.SideFilter = CharacterSides.Tychon;

            Assert.That(_filter.PassesAll(_tychonThought), Is.False);
            Assert.That(_filter.PassesAll(_forgerThought), Is.False);
            Assert.That(_filter.PassesAll(_anyThought), Is.False);
        }
    }
}
