using System.Runtime.Versioning;

namespace Dialuverc.Editor.Base.Installation.Windows
{
    /// <summary>
    /// Pairs a <see href="https://learn.microsoft.com/en-us/windows/win32/shell/fa-progids">ProgID</see> with a program on Windows.
    /// </summary>
    [SupportedOSPlatform("Windows")]
    public class WindowsProgram
    {
        public readonly string ProgramID;
        public readonly string ProgramPath;

        public readonly string? LaunchArgs;

        public WindowsProgram(string programID, string programPath, string launchArgs = null)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(programID);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(programPath);

            ProgramID = programID;
            ProgramPath = programPath;

            LaunchArgs = launchArgs;
        }
    }
}
