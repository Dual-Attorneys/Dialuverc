using System.IO.Compression;

namespace Dialuverc.Editor.Base.IO
{
    /// <summary>
    /// Contains methods to handle exporting/importing.
    /// </summary>
    public class ProjectExporter
    {
        /// <summary>
        /// Creates a <see cref="ZipArchive"/> containing a <see cref="ZipArchiveEntry"/> for each <see cref="IExportable"/> in <paramref name="toExport"/>.
        /// </summary>
        /// <returns>An open <see cref="MemoryStream"/> at position 0.</returns>
        public static MemoryStream CreateZip(IEnumerable<IExportable> toExport)
        {
            MemoryStream memoryStream = new MemoryStream();

            using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (IExportable exportable in toExport)
                {
                    ZipArchiveEntry entry = archive.CreateEntry(exportable.ExportName);

                    using (Stream entryStream = entry.Open())
                    {
                        using (StreamWriter writer = new StreamWriter(entryStream))
                        {
                            writer.Write(exportable.SerializeForExport());
                        }
                    }
                }
            }
            // Disposing the ZipArchive is needed to produce a valid object.

            memoryStream.Seek(0, SeekOrigin.Begin);

            return memoryStream;
        }
    }
}
