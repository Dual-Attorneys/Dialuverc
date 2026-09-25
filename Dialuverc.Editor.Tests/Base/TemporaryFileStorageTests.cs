using Dialuverc.Editor.Base.IO;

namespace Dialuverc.Editor.Tests.Base
{
    internal class TemporaryFileStorageTests
    {
        [Test]
        public void FolderIsCreatedAndDestroyed()
        {
            string? storagePath;

            using (TemporaryFileStorage storage = new TemporaryFileStorage(Path.GetRandomFileName()))
            {
                storagePath = storage.AbsoluteFolderPath;

                Assert.That(Directory.Exists(storagePath), Is.True);
            }

            Assert.That(Directory.Exists(storagePath), Is.False);
        }
    }
}
