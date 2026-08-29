using System.IO.Compression;

namespace Dialuverc.Editor.Base.IO
{
    /// <summary>
    /// Contains methods to handle exporting/importing.
    /// </summary>
    public class ProjectExporter
    {
        /// <summary>
        /// Creates a <see cref="ZipArchive"/> containing a <see cref="ZipArchiveEntry"/> for each <see cref="IExportable"/> in <paramref name="toExport"/>.<br/>
        /// The <paramref name="stream"/> is left open.
        /// <para>Consider using this when exporting a "finished" version of the project for game consumption as a single file.</para>
        /// </summary>
        /// <param name="toExport"></param>
        /// <param name="stream"></param>
        /// <param name="resetStreamPosition">Whether to reset <see cref="Stream.Position"/> to 0.</param>
        public static void CreateZip(IEnumerable<IExportable> toExport, Stream stream, bool resetStreamPosition = true)
        {
            using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
            {
                foreach (IExportable exportable in toExport)
                {
                    ZipArchiveEntry entry = archive.CreateEntry(exportable.ExportPath);

                    using (Stream entryStream = entry.Open())
                    {
                        exportable.SerializeForExport(entryStream);
                    }
                }
            }
            // Disposing the ZipArchive is needed to produce a valid object.

            if (resetStreamPosition)
                stream.Seek(0, SeekOrigin.Begin);
        }

        /// <summary>
        /// Creates a folder at <paramref name="folderPath"/> containing a <see cref="File"/> for each <see cref="IExportable"/> in <paramref name="toExport"/>.
        /// <para>This is preferable over <see cref="CreateZip(IEnumerable{IExportable}, Stream, bool)"/> when working with version control<br/>
        /// and <see cref="IExportable.SerializeForExport(Stream)"/> outputs a text format.</para>
        /// <para>The folder is created if it doesn't exist yet.</para>
        /// </summary>
        public static void ExportToFolder(IEnumerable<IExportable> toExport, string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                throw new ArgumentException($"Folder path can't be null or white space", nameof(folderPath));

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            foreach (IExportable exportable in toExport)
            {
                string fullPath = Path.Combine(folderPath, exportable.ExportPath);
                string? nestedFolderPath = Path.GetDirectoryName(fullPath);

                if (nestedFolderPath is not null && !Directory.Exists(nestedFolderPath))
                    Directory.CreateDirectory(nestedFolderPath);

                using (FileStream fileStream = File.Open(fullPath, FileMode.Create, FileAccess.Write))
                {
                    exportable.SerializeForExport(fileStream);
                }
            }
        }
    }
}
