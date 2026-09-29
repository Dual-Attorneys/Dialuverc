using System.Runtime.Versioning;

namespace Dialuverc.Editor.Base.Installation.Windows
{
    /// <summary>
    /// Pairs a file extension with an icon on Windows.
    /// </summary>
    [SupportedOSPlatform("Windows")]
    public class WindowsFileExtension
    {
        public readonly string Extension;
        public readonly string IconPath;

        public WindowsFileExtension(string extension, string iconPath)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(extension);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(iconPath);

            if (extension.Length < 2)
                throw new ArgumentException("An extension can't be a single character", nameof(extension));

            if (extension[0] != '.')
                throw new ArgumentException("An extension must have a leading dot", nameof(extension));

            if (MemoryExtensions.Count(extension, '.') > 1)
                throw new ArgumentException("An extension can't contain more than one dot", nameof(extension));

            foreach (char character in extension.Skip(1))
            {
                if (!char.IsAsciiLetterOrDigit(character))
                    throw new ArgumentException("An extension can't contain invalid characters for a path", nameof(extension));
            }

            if (!Path.IsPathFullyQualified(iconPath))
                throw new ArgumentException("Path is not fully qualified", nameof(iconPath));

            if (!File.Exists(iconPath))
                throw new FileNotFoundException("Icon doesn't exist", iconPath);

            if (Path.GetExtension(iconPath) != ".ico")
                throw new ArgumentException("An icon must be a '.ico' file", nameof(iconPath));

            Extension = extension;
            IconPath = iconPath;
        }
    }
}
