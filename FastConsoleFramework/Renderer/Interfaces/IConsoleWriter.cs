using System.Drawing;

namespace FastConsoleFramework.Renderer
{
    public interface IConsoleWriter : IAsyncDisposable, IDisposable
    {
        void WriteString(string inputString);

        void WriteString(string inputString, bool isFlushingCommands);

        Task WriteStringAsync(string inputString, CancellationToken cancellationToken = default);

        Task WriteStringAsync(string inputString, bool isFlushingCommands, CancellationToken cancellationToken = default);

        void WriteForegroundColorCommand(Color foregroundColor);

        void WriteForegroundColorCommand(Color foregroundColor, bool isFlushingCommands);

        Task WriteForegroundColorCommandAsync(Color foregroundColor, CancellationToken cancellationToken = default);

        Task WriteForegroundColorCommandAsync(Color foregroundColor, bool isFlushingCommands, CancellationToken cancellationToken = default);

        void WriteBackgroundColorCommand(Color backgroundColor);

        void WriteBackgroundColorCommand(Color backgroundColor, bool isFlushingCommands);

        Task WriteBackgroundColorCommandAsync(Color backgroundColor, CancellationToken cancellationToken = default);

        Task WriteBackgroundColorCommandAsync(Color backgroundColor, bool isFlushingCommands, CancellationToken cancellationToken = default);

        void WriteClearConsoleCommand();

        void WriteClearConsoleCommand(bool isFlushingCommands);

        Task WriteClearConsoleCommandAsync(CancellationToken cancellationToken = default);

        Task WriteClearConsoleCommandAsync(bool isFlushingCommands, CancellationToken cancellationToken = default);

        void WriteResetCursorPositionCommand();

        void WriteResetCursorPositionCommand(bool isFlushingCommands);

        Task WriteResetCursorPositionCommandAsync(CancellationToken cancellationToken = default);

        Task WriteResetCursorPositionCommandAsync(bool isFlushingCommands, CancellationToken cancellationToken = default);

        void WriteSetCursorPositionCommand(Point cursorPosition);

        void WriteSetCursorPositionCommand(Point cursorPosition, bool isFlushingCommands);

        Task WriteSetCursorPositionCommandAsync(Point cursorPosition, CancellationToken cancellationToken = default);

        Task WriteSetCursorPositionCommandAsync(Point cursorPosition, bool isFlushingCommands, CancellationToken cancellationToken = default);

        void WriteShowCursorCommand();

        void WriteShowCursorCommand(bool isFlushingCommands);

        void WriteHideCursorCommand();

        void WriteHideCursorCommand(bool isFlushingCommands);

        Task WriteHideCursorCommandAsync(CancellationToken cancellationToken = default);

        Task WriteHideCursorCommandAsync(bool isFlushingCommands, CancellationToken cancellationToken = default);

        void FlushCommands();

        Task FlushCommandsAsync(CancellationToken cancellationToken = default);
    }
}
