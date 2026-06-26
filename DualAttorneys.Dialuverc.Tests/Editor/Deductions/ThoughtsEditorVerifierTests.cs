using Dialuverc.Editor.Base.Verifier;
using DualAttorneys.Dialuverc.Deductions;
using DualAttorneys.Dialuverc.Editor.Deductions;

namespace DualAttorneys.Dialuverc.Tests.Editor.Deductions
{
    internal class ThoughtsEditorVerifierTests
    {
        ThoughtsEditorArea _area;

        [SetUp]
        public void SetUp()
        {
            _area = new ThoughtsEditorArea();
        }

        [Test]
        public void DuplicateNames()
        {
            _area.AddThought("NameOne", "DescOne", CharacterSides.Any);
            _area.AddThought("DuplicateName", "DescTwo", CharacterSides.Any);
            _area.AddThought("DuplicateName", "DescThree", CharacterSides.Any);
            _area.AddThought("NameFour", "DescFour", CharacterSides.Any);

            IReadOnlyList<Problem> problems = _area.Verify();

            Assert.That(problems, Has.Count.EqualTo(1));
        }

        [Test]
        public void DuplicateDescriptions()
        {
            _area.AddThought("NameOne", "DescOne", CharacterSides.Any);
            _area.AddThought("NameTwo", "DuplicateDescription", CharacterSides.Any);
            _area.AddThought("NameThree", "DuplicateDescription", CharacterSides.Any);
            _area.AddThought("NameFour", "DescFour", CharacterSides.Any);

            IReadOnlyList<Problem> problems = _area.Verify();

            Assert.That(problems, Has.Count.EqualTo(1));
        }
    }
}
