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
        /// <para>The <paramref name="stream"/> is left open.</para>
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
    }
}
