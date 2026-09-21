namespace Dialuverc.Editor.Base.Localization
{
    public class LocalizationMergerFlatFile
    {
        public const string MissingColumnNamesText = "Add column names here!";

        public const char CSVSeparator = ',';
        public const char TSVSeparator = '\t';

        /// <summary>
        /// Merges an existing localization flat-file with a collection of localizable keys.
        /// <para>Keys that:<br/>
        /// - Exist both in the file and the collection will be copied over with their values intact.<br/>
        /// - Do not exist in the collection will have their corresponding line dropped.<br/>
        /// - Exist in the collection but not the file will be appended at the file's end.</para>
        /// <para>The file is created if it doesn't exist yet.</para>
        /// </summary>
        /// <param name="existingKeys"></param>
        /// <param name="filePath"></param>
        /// <param name="separator">A character used to separate columns in a delimiter-separated values format.</param>
        public static void Merge(IReadOnlySet<string> existingKeys, string filePath, char separator)
        {
            ReadOnlySpan<char> fileName = Path.GetFileNameWithoutExtension(filePath);

            string? directoryPath = Path.GetDirectoryName(filePath);

            if (string.IsNullOrWhiteSpace(directoryPath))
                throw new DirectoryNotFoundException($"Can't merge localization because folder containing the file at path '{filePath}' couldn't be retrieved");

            string tempFilePath = Path.Combine(directoryPath, $"_{fileName}_{Guid.NewGuid()}");

            // TODO: This doesn't account for ordering, should it?
            HashSet<string> keysLeft = new HashSet<string>(existingKeys);

            try
            {
                // I find this generally less readable, but here indentation gets really bad otherwise.
                using (FileStream tempFileStream = File.Open(tempFilePath, FileMode.CreateNew, FileAccess.Write))
                using (StreamWriter streamWriter = new StreamWriter(tempFileStream))
                using (FileStream originalFileStream = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Read))
                using (StreamReader streamReader = new StreamReader(originalFileStream))
                {
                    string? columnNames = streamReader.ReadLine();

                    if (string.IsNullOrWhiteSpace(columnNames))
                        columnNames = MissingColumnNamesText;

                    streamWriter.WriteLine(columnNames);

                    while (!streamReader.EndOfStream)
                    {
                        // Returns null only if the stream end is reached, but we check for that in the loop already.
                        string currentLine = streamReader.ReadLine()!;

                        int indexOfFirstSeparator = currentLine.IndexOf(separator);

                        // There's no key. The line looks like ".something".
                        // The line is just a white space.
                        if (indexOfFirstSeparator == 0 || string.IsNullOrWhiteSpace(currentLine))
                            continue;

                        // There's no way to check if a string exists using a span.
                        string currentKey;

                        // The line only contains a key (if the key still exists in the collection).
                        if (indexOfFirstSeparator == -1)
                            currentKey = currentLine;
                        else
                            currentKey = currentLine.AsSpan().Slice(0, indexOfFirstSeparator).ToString();

                        // If the key is not "deprecated", copy the whole line in the new file.
                        // If it is, drop the whole line.
                        if (keysLeft.Remove(currentKey))
                            streamWriter.WriteLine(currentLine);
                    }

                    // Create a new line for each new key not in the original file.
                    foreach (string leftoverKey in keysLeft)
                    {
                        streamWriter.WriteLine(leftoverKey);
                    }
                }

                File.Move(tempFilePath, filePath, true);
            }
            catch
            {
                File.Delete(tempFilePath);

                // Surface any exceptions thrown during merging.
                throw;
            }
        }
    }
}
