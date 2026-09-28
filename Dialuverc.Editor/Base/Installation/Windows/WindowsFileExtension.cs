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

            Extension = extension;
            IconPath = iconPath;
        }
    }
}
