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

        public readonly string[]? LaunchArgs;

        public WindowsProgram(string programID, string programPath, string[]? launchArgs = null)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(programID);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(programPath);

            if (!Path.IsPathFullyQualified(programPath))
                throw new ArgumentException("Path is not fully qualified", nameof(programPath));

            // https://learn.microsoft.com/en-us/windows/win32/com/-progid--key
            if (!char.IsAsciiLetter(programID[0]))
                throw new ArgumentException("A ProgID can only start with an ASCII letter", nameof(programID));

            if (programID[programID.Length - 1] == '.')
                throw new ArgumentException("A ProgID can only end with an ASCII number or letter", nameof(programID));

            foreach (char character in programID)
            {
                if (!char.IsAsciiLetterOrDigit(character) && character != '.')
                    throw new ArgumentException("A ProgID can only contain ASCII numbers, letters or dots", nameof(programID));
            }

            ProgramID = programID;
            ProgramPath = programPath;

            LaunchArgs = launchArgs;
        }
    }
}
