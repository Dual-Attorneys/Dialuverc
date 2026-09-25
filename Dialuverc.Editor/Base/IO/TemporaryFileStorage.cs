namespace Dialuverc.Editor.Base.IO
{
    /// <summary>
    /// Represents a temporary folder which is deleted (along with its contents) on disposal.<br/>
    /// The folder is created at <see cref="Path.GetTempPath"/>.
    /// <para><b>Note</b>: This class is intended for testing purposes only.</para>
    /// </summary>
    public class TemporaryFileStorage : IDisposable
    {
        /// <summary>
        /// The absolute path the folder is at.
        /// </summary>
        public string AbsoluteFolderPath { get; private set; }

        /// <summary>
        /// Creates a new <see cref="TemporaryFileStorage"/> representing the folder at <paramref name="folderPath"/>.<br/>
        /// The folder is created if it doesn't exist yet.
        /// </summary>
        public TemporaryFileStorage(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                throw new ArgumentException($"Folder path can't be null or white space", nameof(folderPath));

            string fullPath = Path.Combine(Path.GetTempPath(), folderPath);

            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);

            AbsoluteFolderPath = fullPath;
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(AbsoluteFolderPath, true);
            }
            catch { }
        }
    }
}
