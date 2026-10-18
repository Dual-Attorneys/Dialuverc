using System.IO.Compression;

namespace Dialuverc.Editor.Base.IO
{
    /// <summary>
    /// Contains methods to handle exporting/importing.
    /// </summary>
    public class ExportableUtils
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
        public static void ExportToFolder(IEnumerable<IExportable> toExport, string folderPath, AllowedDirectoryTraversal allowedTraversal = AllowedDirectoryTraversal.None)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                throw new ArgumentException($"Folder path can't be null or white space", nameof(folderPath));

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            foreach (IExportable exportable in toExport)
            {
                string fullPath = Path.Combine(folderPath, exportable.ExportPath);
                string? nestedFolderPath = Path.GetDirectoryName(fullPath);

                ThrowIfPathDirectoryTraversalIsNotAllowed(fullPath, folderPath, allowedTraversal);

                if (!Directory.Exists(nestedFolderPath))
                    // TODO: Check whether this can actually never be null in our case.
                    Directory.CreateDirectory(nestedFolderPath!);

                using (FileStream fileStream = File.Open(fullPath, FileMode.Create, FileAccess.Write))
                {
                    exportable.SerializeForExport(fileStream);
                }
            }
        }

        /// <summary>
        /// Provides each <see cref="IImportable"/> with a readable <see cref="FileStream"/> of the <see cref="ZipArchiveEntry"/> in the <see cref="ZipArchive"/> corresponding to its <see cref="IExportable.ExportPath"/>.
        /// </summary>
        public static void ImportFromZip(IEnumerable<IImportable> toImport, Stream stream)
        {
            using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read, true))
            {
                foreach (IImportable importable in toImport)
                {
                    ZipArchiveEntry? entry = archive.GetEntry(importable.ExportPath);

                    // TODO: Proper error handling.
                    if (entry is null)
                        continue;

                    using (Stream entryStream = entry.Open())
                    {
                        importable.DeserializeForImport(entryStream);
                    }
                }
            }
            // Disposing the ZipArchive is needed to produce a valid object.
        }

        /// <summary>
        /// Provides each <see cref="IImportable"/> with a readable <see cref="FileStream"/> of the <see cref="File"/> in the <see cref="Directory"/> corresponding to its <see cref="IExportable.ExportPath"/>.
        /// </summary>
        public static void ImportFromFolder(IEnumerable<IImportable> toImport, string folderPath, AllowedDirectoryTraversal allowedTraversal = AllowedDirectoryTraversal.None)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                throw new ArgumentException($"Folder path can't be null or white space", nameof(folderPath));

            if (!Directory.Exists(folderPath))
                throw new DirectoryNotFoundException($"Folder path '{folderPath}' points to a non-existing folder");

            foreach (IImportable importable in toImport)
            {
                string fullPath = Path.Combine(folderPath, importable.ExportPath);
                string? nestedFolderPath = Path.GetDirectoryName(fullPath);

                ThrowIfPathDirectoryTraversalIsNotAllowed(fullPath, folderPath, allowedTraversal);

                if (!Directory.Exists(nestedFolderPath))
                    throw new DirectoryNotFoundException($"Exportable's {nameof(IImportable.ExportPath)} '{nestedFolderPath}' points to a non-existing folder");

                using (FileStream fileStream = File.Open(fullPath, FileMode.Open, FileAccess.Read))
                {
                    importable.DeserializeForImport(fileStream);
                }
            }
        }

        // https://safeguard.sh/resources/blog/path-traversal-prevention-in-c-with-pathgetfullpath
        public static bool PathDirectoryTraversalIsAllowed(string targetPath, string referenceFolder, AllowedDirectoryTraversal allowedTraversal)
        {
            string fullTargetPath = Path.GetFullPath(targetPath);
            string fullReferenceFolderPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(referenceFolder));

            switch (allowedTraversal)
            {
                case AllowedDirectoryTraversal.None:
                    return fullTargetPath.StartsWith(fullReferenceFolderPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);

                case AllowedDirectoryTraversal.Parent:

                    string? parentOfReferenceFolder = Directory.GetParent(fullReferenceFolderPath)?.FullName;

                    if (parentOfReferenceFolder is null)
                        return false;

                    parentOfReferenceFolder = Path.TrimEndingDirectorySeparator(parentOfReferenceFolder);

                    return fullTargetPath.StartsWith(parentOfReferenceFolder + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);

                default:
                    throw new NotImplementedException($"Passed {nameof(allowedTraversal)} \"{allowedTraversal}\" is not valid");
            }
        }

        static void ThrowIfPathDirectoryTraversalIsNotAllowed(string targetPath, string referenceFolder, AllowedDirectoryTraversal allowedTraversal)
        {
            if (!PathDirectoryTraversalIsAllowed(targetPath, referenceFolder, allowedTraversal))
                throw new InvalidOperationException($"{allowedTraversal} prohibits traversing to \"{targetPath}\" from \"{referenceFolder}\"");
        }

        /// <summary>
        /// Where an <see cref="IExportable.ExportPath"/> is allowed to traverse to (only upwards traversal is relevant) relative to some reference folder.
        /// </summary>
        public enum AllowedDirectoryTraversal
        {
            /// <summary>
            /// Exiting the reference folder is prohibited.
            /// </summary>
            None,

            /// <summary>
            /// Only traversing up to the parent of the reference folder is allowed.
            /// </summary>
            Parent,
        }
    }
}
