using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace FastConsoleFramework.Renderer
{
    public sealed class ConsoleWriter : IConsoleWriter
    {
#if WINDOWS
        private const int STD_OUTPUT_HANDLE = -11;

        private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

        private const uint DISABLE_NEWLINE_AUTO_RETURN = 0x0008;
#endif
        private static readonly string clearConsoleCommand = "\u001b[2J\u001b[1;1H";

        private static readonly string resetCursorPositionCommand = "\u001b[1;1H";

        private static readonly string showCursorCommand = "\u001b[?25h";

        private static readonly string hideCursorCommand = "\u001b[?25l";

        private readonly StreamWriter streamWriter = new(new MemoryStream(), Encoding.Unicode);
#if WINDOWS
        private readonly FileStream consoleFileStream;
#else
        private readonly byte[] copyBytesBuffer = new byte[2048];
#endif
#if WINDOWS
        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        public ConsoleWriter()
        {
            IntPtr std_out_handle = GetStdHandle(STD_OUTPUT_HANDLE);
            if (GetConsoleMode(std_out_handle, out uint console_mode))
            {
                SetConsoleMode(std_out_handle, console_mode | ENABLE_VIRTUAL_TERMINAL_PROCESSING | DISABLE_NEWLINE_AUTO_RETURN);
            }
            consoleFileStream = new("CONOUT$", FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
        }
#else
        [DllImport("libc.so.6", EntryPoint = "write")]
        private static extern int Write(int fd, byte[] buffer, int count);
#endif
        public void WriteString(string inputString) => WriteString(inputString, false);

        public void WriteString(string inputString, bool isFlushingCommands)
        {
            streamWriter.Write(inputString);
            if (isFlushingCommands)
            {
                FlushCommands();
            }
        }

        public Task WriteStringAsync(string inputString, CancellationToken cancellationToken = default) => WriteStringAsync(inputString, false, cancellationToken);

        public Task WriteStringAsync(string inputString, bool isFlushingCommands, CancellationToken cancellationToken = default)
        {
            string input_string = inputString;
            bool is_flushing_commands = isFlushingCommands;
            CancellationToken cancellation_token = cancellationToken;
            return
                Task.Run
                (
                    async () =>
                    {
                        await streamWriter.WriteAsync(input_string);
                        if (is_flushing_commands)
                        {
                            await FlushCommandsAsync(cancellation_token);
                        }
                    },
                    cancellation_token
                );
        }

        public void WriteForegroundColorCommand(Color foregroundColor) => WriteForegroundColorCommand(foregroundColor, false);

        public void WriteForegroundColorCommand(Color foregroundColor, bool isFlushingCommands) =>
            WriteString($"\u001b[38;2;{foregroundColor.R};{foregroundColor.G};{foregroundColor.B}m", isFlushingCommands);

        public Task WriteForegroundColorCommandAsync(Color foregroundColor, CancellationToken cancellationToken = default) =>
            WriteForegroundColorCommandAsync(foregroundColor, false, cancellationToken);

        public Task WriteForegroundColorCommandAsync(Color foregroundColor, bool isFlushingCommands, CancellationToken cancellationToken = default) =>
            WriteStringAsync($"\u001b[38;2;{foregroundColor.R};{foregroundColor.G};{foregroundColor.B}m", isFlushingCommands, cancellationToken);

        public void WriteBackgroundColorCommand(Color backgroundColor) => WriteBackgroundColorCommand(backgroundColor, false);

        public void WriteBackgroundColorCommand(Color backgroundColor, bool isFlushingCommands) =>
            WriteString($"\u001b[48;2;{backgroundColor.R};{backgroundColor.G};{backgroundColor.B}m", isFlushingCommands);

        public Task WriteBackgroundColorCommandAsync(Color backgroundColor, CancellationToken cancellationToken = default) =>
            WriteBackgroundColorCommandAsync(backgroundColor, false, cancellationToken);

        public Task WriteBackgroundColorCommandAsync(Color backgroundColor, bool isFlushingCommands, CancellationToken cancellationToken = default) =>
            WriteStringAsync($"\u001b[48;2;{backgroundColor.R};{backgroundColor.G};{backgroundColor.B}m", isFlushingCommands, cancellationToken);

        public void WriteClearConsoleCommand() => WriteClearConsoleCommand(false);

        public void WriteClearConsoleCommand(bool isFlushingCommands) => WriteString(clearConsoleCommand, isFlushingCommands);

        public Task WriteClearConsoleCommandAsync(CancellationToken cancellationToken = default) => WriteClearConsoleCommandAsync(false, cancellationToken);

        public Task WriteClearConsoleCommandAsync(bool isFlushingCommands, CancellationToken cancellationToken = default) =>
            WriteStringAsync(clearConsoleCommand, isFlushingCommands, cancellationToken);

        public void WriteResetCursorPositionCommand() => WriteResetCursorPositionCommand(false);

        public void WriteResetCursorPositionCommand(bool isFlushingCommands) => WriteString(resetCursorPositionCommand, isFlushingCommands);

        public Task WriteResetCursorPositionCommandAsync(CancellationToken cancellationToken = default) => WriteResetCursorPositionCommandAsync(false, cancellationToken);

        public Task WriteResetCursorPositionCommandAsync(bool isFlushingCommands, CancellationToken cancellationToken = default) =>
            WriteStringAsync(resetCursorPositionCommand, isFlushingCommands, cancellationToken);

        public void WriteSetCursorPositionCommand(Point cursorPosition) => WriteSetCursorPositionCommand(cursorPosition, false);

        public void WriteSetCursorPositionCommand(Point cursorPosition, bool isFlushingCommands) =>
            WriteString($"\u001b[{cursorPosition.Y + 1};{cursorPosition.X + 1}H", isFlushingCommands);

        public Task WriteSetCursorPositionCommandAsync(Point cursorPosition, CancellationToken cancellationToken = default) =>
            WriteSetCursorPositionCommandAsync(cursorPosition, false, cancellationToken);

        public Task WriteSetCursorPositionCommandAsync(Point cursorPosition, bool isFlushingCommands, CancellationToken cancellationToken = default) =>
            WriteStringAsync($"\u001b[{cursorPosition.Y + 1};{cursorPosition.X + 1}H", isFlushingCommands, cancellationToken);

        public void WriteShowCursorCommand() => WriteShowCursorCommand(false);

        public void WriteShowCursorCommand(bool isFlushingCommands) => WriteString(showCursorCommand, isFlushingCommands);

        public void WriteHideCursorCommand() => WriteHideCursorCommand(false);

        public void WriteHideCursorCommand(bool isFlushingCommands) => WriteString(hideCursorCommand, isFlushingCommands);

        public Task WriteHideCursorCommandAsync(CancellationToken cancellationToken = default) => WriteHideCursorCommandAsync(false, cancellationToken);

        public Task WriteHideCursorCommandAsync(bool isFlushingCommands, CancellationToken cancellationToken = default) =>
            WriteStringAsync(hideCursorCommand, isFlushingCommands, cancellationToken);

        public void FlushCommands()
        {
            streamWriter.Flush();
            streamWriter.BaseStream.Seek(0L, SeekOrigin.Begin);
#if WINDOWS
            streamWriter.BaseStream.CopyTo(consoleFileStream);
#else
            int read_byte_count;
            while ((read_byte_count = streamWriter.BaseStream.Read(copyBytesBuffer)) > 0)
            {
                if (Write(1, copyBytesBuffer, read_byte_count) < read_byte_count)
                {
                    break;
                }
            }
#endif
            streamWriter.BaseStream.Seek(0L, SeekOrigin.Begin);
            streamWriter.BaseStream.SetLength(0L);
#if WINDOWS
            consoleFileStream.Flush();
#endif
        }

        public Task FlushCommandsAsync(CancellationToken cancellationToken = default)
        {
            CancellationToken cancellation_token = cancellationToken;
            return
                Task.Run
                (
                    async () =>
                    {
                        await streamWriter.FlushAsync();
                        streamWriter.BaseStream.Seek(0L, SeekOrigin.Begin);
#if WINDOWS
                        await streamWriter.BaseStream.CopyToAsync(consoleFileStream, cancellation_token);
#else
                        byte[] copy_bytes_buffer = new byte[copyBytesBuffer.Length];
                        int read_byte_count;
                        while ((read_byte_count = await streamWriter.BaseStream.ReadAsync(copy_bytes_buffer, cancellation_token)) > 0)
                        {
                            if (Write(1, copy_bytes_buffer, read_byte_count) < read_byte_count)
                            {
                                break;
                            }
                        }
#endif
                        streamWriter.BaseStream.Seek(0L, SeekOrigin.Begin);
                        streamWriter.BaseStream.SetLength(0L);
#if WINDOWS
                        await consoleFileStream.FlushAsync(cancellation_token);
#endif
                    },
                    cancellation_token
                );
        }

        public async ValueTask DisposeAsync()
        {
            await streamWriter.DisposeAsync();
#if WINDOWS
            await consoleFileStream.DisposeAsync();
#endif
        }

        public void Dispose() => DisposeAsync().AsTask().GetAwaiter().GetResult();
    }
}
