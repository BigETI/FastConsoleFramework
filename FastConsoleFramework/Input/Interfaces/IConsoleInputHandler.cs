namespace FastConsoleFramework.Input
{
    public interface IConsoleInputHandler
    {
        event ConsoleInputEventReceivedDelegate? OnConsoleInputEventReceived;

        void ProcessConsoleInputEvents();
    }
}
