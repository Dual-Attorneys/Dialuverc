using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Dialuverc.Editor.Base.Installation.Windows
{
    /// <summary>
    /// Handles installation of Dialuverc-derived editors on Windows.
    /// </summary>
    [SupportedOSPlatform("Windows")]
    public class WindowsInstaller
    {
        const string _softwareClasses = @"Software\Classes";
        const string _openWithProgIds = "OpenWithProgIds";
        const string _defaultIcon = "DefaultIcon";
        const string _shellOpenCommand = @"Shell\Open\Command";

        /// <summary>
        /// Sets up the required <see cref="RegistryKey"/>s to tell the OS:<br/>
        /// - Which icon to use for the passed <paramref name="extension"/>.<br/>
        /// - Which <paramref name="program"/> to run when the file is opened.
        /// </summary>
        public static void InstallFileExtensionForProgram(WindowsFileExtension extension, WindowsProgram program)
        {
            string extensionOpenWithProgIdsPath = $@"{extension.Extension}\{_openWithProgIds}";
            string progIDDotExtension = $"{program.ProgramID}{extension.Extension}";

            using (RegistryKey? topmostKey = Registry.CurrentUser.OpenSubKey(_softwareClasses, true))
            {
                if (topmostKey is null)
                    return;

                using (RegistryKey extensionOpenWithProgIdsKey = topmostKey.CreateSubKey(extensionOpenWithProgIdsPath))
                {
                    extensionOpenWithProgIdsKey.SetValue(progIDDotExtension, string.Empty);
                }

                using (RegistryKey progIDDotExtensionKey = topmostKey.CreateSubKey(progIDDotExtension))
                {
                    using (RegistryKey defaultIconKey = progIDDotExtensionKey.CreateSubKey(_defaultIcon))
                    {
                        defaultIconKey.SetValue(null, extension.IconPath);
                    }

                    using (RegistryKey shellOpenCommandKey = progIDDotExtensionKey.CreateSubKey(_shellOpenCommand))
                    {
                        shellOpenCommandKey.SetValue(null, $"\"{program.ProgramPath}\" \"%1\"");
                    }
                }
            }

            NotifyWindows();
        }

        /// <summary>
        /// Removes the passed <paramref name="program"/> as an intended way to open files with the passed <paramref name="extension"/>.
        /// </summary>
        public static void UninstallFileExtensionForProgram(WindowsFileExtension extension, WindowsProgram program)
        {
            string extensionPath = $@"{extension.Extension}";
            string extensionOpenWithProgIdsPath = $@"{extension.Extension}\{_openWithProgIds}";
            string progIDDotExtension = $"{program.ProgramID}{extension.Extension}";

            using (RegistryKey? topmostKey = Registry.CurrentUser.OpenSubKey(_softwareClasses, true))
            {
                if (topmostKey is null)
                    return;

                using (RegistryKey extensionOpenWithProgIdsKey = topmostKey.CreateSubKey(extensionOpenWithProgIdsPath))
                {
                    extensionOpenWithProgIdsKey.DeleteValue(progIDDotExtension, false);
                }
            }

            NotifyWindows();
        }

        /// <summary>
        /// Removes all program associations for the passed <paramref name="extension"/>.
        /// </summary>
        public static void UninstallFileExtensionForAll(WindowsFileExtension extension)
        {
            string extensionPath = $@"{extension.Extension}";

            using (RegistryKey? topmostKey = Registry.CurrentUser.OpenSubKey(_softwareClasses, true))
            {
                if (topmostKey is null)
                    return;

                topmostKey.DeleteSubKeyTree(extensionPath, false);
            }

            NotifyWindows();
        }

        [DllImport("Shell32.dll")]
        static extern void SHChangeNotify(uint eventID, uint flags, IntPtr dwItem1, IntPtr dwItem2);

        // eventID is SHCNE_ASSOCCHANGED, flags is SHCNF_IDLIST.
        // dwItem1 and dwItem2 should be IntPtr.Zero.
        static void NotifyWindows() => SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
    }
}
