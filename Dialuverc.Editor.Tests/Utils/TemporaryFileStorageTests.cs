namespace Dialuverc.Editor.Tests.Utils
{
    internal class TemporaryFileStorageTests
    {
        [Test]
        public void FolderIsCreatedAndDestroyed()
        {
            TemporaryFileStorage storage = new TemporaryFileStorage(
                Path.Combine(Path.GetTempPath(), $"Dialuverc{nameof(TemporaryFileStorageTests)}{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}"));

            using (storage)
            {
                Assert.That(Directory.Exists(storage.FolderPath), Is.True);
            }

            Assert.That(Directory.Exists(storage.FolderPath), Is.False);
        }
    }
}
