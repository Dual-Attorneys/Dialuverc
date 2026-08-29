namespace Dialuverc.Editor.Tests.Utils
{
    /// <summary>
    /// Represents a temporary folder whose contents are deleted on disposal.
    /// </summary>
    // Note: This class is a convenience intended for testing. Do not use for anything else.
    internal class TemporaryFileStorage : IDisposable
    {
        public string FolderPath { get; private set; }

        /// <summary>
        /// Creates a new <see cref="TemporaryFileStorage"/> representing the folder at <paramref name="folderPath"/>.<br/>
        /// The folder is created if it doesn't exist yet.
        /// </summary>
        public TemporaryFileStorage(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                throw new ArgumentException($"Folder path can't be null or white space", nameof(folderPath));

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            FolderPath = folderPath;
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(FolderPath, true);
            }
            catch { }
        }
    }
}
