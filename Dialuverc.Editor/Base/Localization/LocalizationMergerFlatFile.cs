namespace Dialuverc.Editor.Base.Localization
{
    public class LocalizationMergerFlatFile
    {
        public const char CSVSeparator = ',';
        public const char TSVSeparator = '\t';

        public readonly ICollection<string> ColumnNames;

        public LocalizationMergerFlatFile(ICollection<string> columnNames)
        {
            ArgumentNullException.ThrowIfNull(columnNames);

            ColumnNames = columnNames;
        }

        /// <summary>
        /// Updates or creates a flat-file containing localizable keys (and eventual already-existing values).
        /// <para>
        /// The existing content is merged with the passed <paramref name="allExistingKeys"/> with the following rules based on where the key is present:<br/>
        /// <list type="table">
        /// <item>
        /// <term>In collection</term>
        /// <term>In file</term>
        /// <description>Result</description>
        /// </item>
        /// <item>
        /// <term>Y</term>
        /// <term>Y</term>
        /// <description>Line is copied</description>
        /// </item>
        /// <item>
        /// <term>Y</term>
        /// <term>N</term>
        /// <description>New line with key is appended at end of file</description>
        /// </item>
        /// <item>
        /// <term>N</term>
        /// <term>Y</term>
        /// <description>Line is lost</description>
        /// </item>
        /// </list>
        /// </para>
        /// <para>
        /// Column names are always set to <see cref="ColumnNames"/> if the file doesn't exist yet.<br/>
        /// If it does, they're copied without any special processing on the corresponding values.
        /// </para>
        /// <para>
        /// <b>Note</b>: Appending is unordered.
        /// </para>
        /// </summary>
        /// <param name="allExistingKeys">Determines which keys currently exist.</param>
        /// <param name="filePath"></param>
        /// <param name="separator"></param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public void Merge(IReadOnlySet<string> allExistingKeys, string filePath, char separator)
        {
            ReadOnlySpan<char> fileName = Path.GetFileNameWithoutExtension(filePath);

            string? directoryPath = Path.GetDirectoryName(filePath);

            if (string.IsNullOrWhiteSpace(directoryPath))
                throw new DirectoryNotFoundException($"Can't merge localization because folder containing the file at path '{filePath}' couldn't be retrieved");

            string tempFilePath = Path.Combine(directoryPath, $"_{fileName}_{Guid.NewGuid()}");

            // TODO: This doesn't account for ordering, should it?
            HashSet<string> keysLeft = new HashSet<string>(allExistingKeys);

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
                        columnNames = string.Join(separator, ColumnNames);

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
