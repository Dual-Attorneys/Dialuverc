using Dialuverc.Editor.Base.IO;
using Dialuverc.Editor.Base.Localization;

namespace Dialuverc.Editor.Tests.Base
{
    internal class LocalizationMergerTests
    {
        string _tempStoragePathForTest => $"Dialuverc{nameof(LocalizationMergerTests)}{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

        [Test]
        public void FileIsCreatedIfNotExisting()
        {
            HashSet<string> keys = new HashSet<string>() { "key1", "key2", "key3",  "key4" };

            using (TemporaryFileStorage tempStorage = new TemporaryFileStorage(
                Path.Combine(Path.GetTempPath(), _tempStoragePathForTest)))
            {
                string outputPath = Path.Combine(tempStorage.FolderPath, "localizationOut.csv");

                Assert.That(File.Exists(outputPath), Is.False);

                LocalizationMergerFlatFile.Merge(keys, outputPath, LocalizationMergerFlatFile.CSVSeparator);

                Assert.That(File.Exists(outputPath), Is.True);

                IEnumerable<string> fileLines = File.ReadLines(outputPath);

                // Asserts have to ignore the first line which contains column names.
                Assert.That(keys.Count, Is.EqualTo(fileLines.Count() - 1));

                foreach (string line in fileLines.Skip(1))
                {
                    Assert.That(keys.Contains(line));
                }
            }
        }

        [Test]
        public void KeysAreAddedIfNotInFile()
        {
            HashSet<string> keys = new HashSet<string>() { "key1", "key2", "key3",  "key4" };

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
            using (TemporaryFileStorage tempStorage = new TemporaryFileStorage(
                Path.Combine(Path.GetTempPath(), _tempStoragePathForTest)))
            {
                string outputPath = Path.Combine(tempStorage.FolderPath, "localizationOut.csv");

                File.WriteAllText(outputPath, starting);

                LocalizationMergerFlatFile.Merge(keys, outputPath, LocalizationMergerFlatFile.CSVSeparator);

                string finalFileContent = File.ReadAllText(outputPath);

                Assert.That(finalFileContent, Is.EqualTo(expected));
            }
        }
    }
}
