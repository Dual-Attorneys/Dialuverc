using System.Runtime.Versioning;

namespace Dialuverc.Editor.Base.Installation.Windows
{
    [SupportedOSPlatform("Windows")]
    public class WindowsFileExtension
    {
        public readonly string Extension;
        public readonly string IconPath;

        public WindowsFileExtension(string extension, string iconPath)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(extension);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(iconPath);

            if (extension[0] != '.')
                throw new ArgumentException($"Extension '{extension}' is malformed (does not start with a dot)");

            Extension = extension;
            IconPath = iconPath;
        }
    }
}
