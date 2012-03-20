namespace CmdLine
{
    public interface ICommandEnvironment
    {
        string CommandLine { get; }

        string[] GetCommandLineArgs();

        string Program { get; }
    }
}