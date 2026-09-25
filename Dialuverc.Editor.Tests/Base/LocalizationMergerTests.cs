using Dialuverc.Editor.Base.IO;
using Dialuverc.Editor.Base.Localization;

namespace Dialuverc.Editor.Tests.Base
{
    internal class LocalizationMergerTests
    {
        string _tempStoragePath => nameof(LocalizationMergerTests);

        LocalizationMergerFlatFile _merger;

        [SetUp]
        public void SetUp()
        {
            _merger = new LocalizationMergerFlatFile(new string[] { "key", "value" });
        }

        [Test]
        public void FileIsCreatedIfNotExisting()
        {
            HashSet<string> keys = new HashSet<string>() { "key1", "key2", "key3", "key4" };

            using (TemporaryFileStorage tempStorage = new TemporaryFileStorage(_tempStoragePath))
            {
                string outputPath = Path.Combine(tempStorage.AbsoluteFolderPath, "localizationOut.csv");

                Assert.That(File.Exists(outputPath), Is.False);

                _merger.Merge(keys, outputPath, LocalizationMergerFlatFile.CSVSeparator);

                Assert.That(File.Exists(outputPath), Is.True);

                string[] fileLines = File.ReadAllLines(outputPath);

                // Asserts have to ignore the first line which contains column names.
                Assert.That(keys.Count, Is.EqualTo(fileLines.Length - 1));

                Assert.That(fileLines[0], Is.EqualTo(string.Join(LocalizationMergerFlatFile.CSVSeparator, _merger.ColumnNames)));

                foreach (string line in fileLines.Skip(1))
                {
                    Assert.That(keys.Contains(line));
                }
            }
        }

        [Test]
        public void KeysAreAddedIfNotInFile()
        {
            HashSet<string> keys = new HashSet<string>() { "key1", "key2", "key3", "key4" };

            string startingFileContent =
@$"key{LocalizationMergerFlatFile.CSVSeparator}value
key2{LocalizationMergerFlatFile.CSVSeparator}value2
key3{LocalizationMergerFlatFile.CSVSeparator}value3{Environment.NewLine}";

            string expectedFinalFileContent =
@$"key{LocalizationMergerFlatFile.CSVSeparator}value
key2{LocalizationMergerFlatFile.CSVSeparator}value2
key3{LocalizationMergerFlatFile.CSVSeparator}value3
key1
key4{Environment.NewLine}";

            WriteAndAssertTextsAreEqual(keys, startingFileContent, expectedFinalFileContent);
        }

        [Test]
        public void KeysAreRemovedIfNotInCollection()
        {
            HashSet<string> keys = new HashSet<string>() { "key2", "key3" };

            string startingFileContent =
@$"key{LocalizationMergerFlatFile.CSVSeparator}value
key1{LocalizationMergerFlatFile.CSVSeparator}value1
key2{LocalizationMergerFlatFile.CSVSeparator}value2
key3{LocalizationMergerFlatFile.CSVSeparator}value3
key4{LocalizationMergerFlatFile.CSVSeparator}value4{Environment.NewLine}";

            string expectedFinalFileContent =
@$"key{LocalizationMergerFlatFile.CSVSeparator}value
key2{LocalizationMergerFlatFile.CSVSeparator}value2
key3{LocalizationMergerFlatFile.CSVSeparator}value3{Environment.NewLine}";

            WriteAndAssertTextsAreEqual(keys, startingFileContent, expectedFinalFileContent);
        }

        void WriteAndAssertTextsAreEqual(IReadOnlySet<string> keys, string starting, string expected)
        {
            using (TemporaryFileStorage tempStorage = new TemporaryFileStorage(_tempStoragePath))
            {
                string outputPath = Path.Combine(tempStorage.AbsoluteFolderPath, "localizationOut.csv");

                File.WriteAllText(outputPath, starting);

                _merger.Merge(keys, outputPath, LocalizationMergerFlatFile.CSVSeparator);

                string finalFileContent = File.ReadAllText(outputPath);

                Assert.That(finalFileContent, Is.EqualTo(expected));
            }
        }
    }
}
