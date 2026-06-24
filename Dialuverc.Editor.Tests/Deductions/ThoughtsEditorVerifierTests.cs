using Dialuverc.Editor.Base.Verifier;
using Dialuverc.Editor.Deductions;

namespace Dialuverc.Editor.Tests.Deductions
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
            _area.AddThought("NameOne", "DescOne", Dialuverc.Deductions.CharacterSides.Any);
            _area.AddThought("DuplicateName", "DescTwo", Dialuverc.Deductions.CharacterSides.Any);
            _area.AddThought("DuplicateName", "DescThree", Dialuverc.Deductions.CharacterSides.Any);
            _area.AddThought("NameFour", "DescFour", Dialuverc.Deductions.CharacterSides.Any);

            IReadOnlyList<Problem> problems = _area.Verify();

            Assert.That(problems, Has.Count.EqualTo(1));
        }

        [Test]
        public void DuplicateDescriptions()
        {
            _area.AddThought("NameOne", "DescOne", Dialuverc.Deductions.CharacterSides.Any);
            _area.AddThought("NameTwo", "DuplicateDescription", Dialuverc.Deductions.CharacterSides.Any);
            _area.AddThought("NameThree", "DuplicateDescription", Dialuverc.Deductions.CharacterSides.Any);
            _area.AddThought("NameFour", "DescFour", Dialuverc.Deductions.CharacterSides.Any);

            IReadOnlyList<Problem> problems = _area.Verify();

            Assert.That(problems, Has.Count.EqualTo(1));
        }
    }
}
