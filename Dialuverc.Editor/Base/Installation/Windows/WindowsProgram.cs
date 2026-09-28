namespace Dialuverc.Editor.Base.Installation.Windows
{
    public class WindowsProgram
    {
        public readonly string ProgramID;
        public readonly string ProgramPath;

        public WindowsProgram(string programID, string programPath)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(programID);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(programPath);

            ProgramID = programID;
            ProgramPath = programPath;
        }
    }
}
