namespace FastConsoleFramework.Input
{
    public sealed class ConsoleInputHandler : IConsoleInputHandler
    {
        public event ConsoleInputEventReceivedDelegate? OnConsoleInputEventReceived;

        public void ProcessConsoleInputEvents()
        {
            while (Console.KeyAvailable)
            {
                ConsoleKeyInfo key_info = Console.ReadKey(true);
                OnConsoleInputEventReceived?.Invoke(key_info);
            }
        }
    }
}
