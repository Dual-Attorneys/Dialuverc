using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

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

            // https://learn.microsoft.com/en-us/windows/win32/com/-progid--key
            if (progIDDotExtension.Length > 39)
                throw new FormatException($"Combined ProgID is too long ({progIDDotExtension.Length > 39})");

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
                        if (program.LaunchArgs is not null)
                        {
                            string joinedArgs = string.Join(' ', program.LaunchArgs.Select(s => $"\"{s}\""));

                            // TODO: Which position should %1 be in?
                            shellOpenCommandKey.SetValue(null, $"\"{program.ProgramPath}\" \"%1\" {joinedArgs}");
                        }
                        else
                        {
                            shellOpenCommandKey.SetValue(null, $"\"{program.ProgramPath}\" \"%1\"");
                        }
                    }
                }
            }

            NotifyWindows();
        }

        /// <summary>
        /// Undoes <see cref="InstallFileExtensionForProgram(WindowsFileExtension, WindowsProgram)"/>:<br/>
        /// - Stops using the previously set icon for the passed <paramref name="extension"/>.<br/>
        /// - Stops running the passed <paramref name="program"/> when attempting to open the file.
        /// </summary>
        // This leaves behind .extension key in case it's an already-in-use-by-other-software key.
        public static void UninstallFileExtensionForProgram(WindowsFileExtension extension, WindowsProgram program)
        {
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

                topmostKey.DeleteSubKeyTree(progIDDotExtension, false);
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
