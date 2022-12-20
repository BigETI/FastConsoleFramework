using FastConsoleFramework.UI;
using System.Drawing;

namespace FastConsoleFramework.Renderer
{
    public sealed class ConsoleDrawContext : IConsoleDrawContext
    {
        private static readonly Rectangle baseRectangle = new(0, 0, int.MaxValue, int.MaxValue);

        private static readonly string spaceString = " ";

        private readonly Stack<Rectangle> rectangleStack = new();

        private Task previousWriteChangesToConsoleTask = Task.CompletedTask;

        public IConsoleFrameBuffer CurrentConsoleFrameBuffer { get; private set; } = new ConsoleFrameBuffer();

        public IConsoleFrameBuffer StagedConsoleFrameBuffer { get; private set; } = new ConsoleFrameBuffer();

        public IConsoleFrameBuffer PreviouslyStagedConsoleFrameBuffer { get; private set; } = new ConsoleFrameBuffer();

        public Rectangle Rectangle => rectangleStack.TryPeek(out Rectangle rectangle) ? rectangle : baseRectangle;

        public bool IsRectangleNotEmpty
        {
            get
            {
                Rectangle rectangle = Rectangle;
                return rectangle.Width > 0 && rectangle.Height > 0;
            }
        }

        private static void InitializeDrawCommands(IConsoleWriter consoleWriter, ref bool isDrawingNotInitialized)
        {
            if (isDrawingNotInitialized)
            {
                consoleWriter.WriteHideCursorCommand();
                consoleWriter.WriteResetCursorPositionCommand();
                isDrawingNotInitialized = false;
            }
        }

        private static void WriteSetCursorPositionCommand(Point cursorPosition, IConsoleWriter consoleWriter, ref bool isDrawingNotInitialized)
        {
            InitializeDrawCommands(consoleWriter, ref isDrawingNotInitialized);
            consoleWriter.WriteSetCursorPositionCommand(cursorPosition);
        }

        private static void WriteForegroundColorCommand(Color foregroundColor, IConsoleWriter consoleWriter, ref bool isDrawingNotInitialized)
        {
            InitializeDrawCommands(consoleWriter, ref isDrawingNotInitialized);
            consoleWriter.WriteForegroundColorCommand(foregroundColor);
        }

        private static void WriteBackgroundColorCommand(Color backgroundColor, IConsoleWriter consoleWriter, ref bool isDrawingNotInitialized)
        {
            InitializeDrawCommands(consoleWriter, ref isDrawingNotInitialized);
            consoleWriter.WriteBackgroundColorCommand(backgroundColor);
        }

        private static void WriteString(string inputString, IConsoleWriter consoleWriter, ref bool isDrawingNotInitialized)
        {
            InitializeDrawCommands(consoleWriter, ref isDrawingNotInitialized);
            consoleWriter.WriteString(inputString);
        }

        private void DrawCell(Point localPosition, Point rectanglePosition, Rectangle frameBufferRectangle, DrawCellDelegate onDrawCell)
        {
            Point global_position = localPosition + (Size)rectanglePosition;
            if (frameBufferRectangle.Contains(global_position))
            {
                ref ConsoleFrameBufferCell frame_buffer_cell =
                    ref CurrentConsoleFrameBuffer.FrameBufferCells[global_position.X + global_position.Y * frameBufferRectangle.Width];
                frame_buffer_cell =
                    frame_buffer_cell.BackgroundColor.A < 0xFF ?
                        onDrawCell(localPosition).GetAlphaBlendedFrameBufferCell(frame_buffer_cell) :
                        frame_buffer_cell;
            }
        }

        public void PushRectangle(Rectangle localRectangle, EAlignment alignment)
        {
            Rectangle old_rectangle = Rectangle;
            rectangleStack.Push
            (
                Rectangle.Intersect
                (
                    old_rectangle,
                    alignment switch
                    {
                        EAlignment.TopLeft => new(localRectangle.Location + (Size)old_rectangle.Location, localRectangle.Size),
                        EAlignment.TopRight =>
                            new
                            (
                                localRectangle.X + old_rectangle.Width - localRectangle.Width,
                                localRectangle.Y + old_rectangle.Y,
                                localRectangle.Width,
                                localRectangle.Height
                            ),
                        EAlignment.BottomLeft =>
                            new
                            (
                                localRectangle.X + old_rectangle.X,
                                localRectangle.Y + old_rectangle.Height - localRectangle.Height,
                                localRectangle.Width,
                                localRectangle.Height
                            ),
                        EAlignment.BottomRight => new(localRectangle.Location + old_rectangle.Size - localRectangle.Size, localRectangle.Size),
                        EAlignment.Center => new(localRectangle.Location + ((old_rectangle.Size - localRectangle.Size) / 2), localRectangle.Size),
                        _ => throw new NotImplementedException($"Alignment \"{alignment}\" has not been implemented yet."),
                    }
                )
            );
        }

        public bool PopRectangle() => rectangleStack.TryPop(out _);

        public void DrawCells(DrawCellDelegate onDrawCell) => DrawCells(onDrawCell, false);

        public void DrawCells(DrawCellDelegate onDrawCell, bool isUsingParallelForLoop)
        {
            Rectangle frame_buffer_rectangle = new(Point.Empty, CurrentConsoleFrameBuffer.Size);
            Rectangle rectangle = Rectangle;
            if (isUsingParallelForLoop)
            {
                Parallel.For
                (
                    0,
                    rectangle.Width * rectangle.Height,
                    (index) => DrawCell(new(index % rectangle.Width, index / rectangle.Width), rectangle.Location, frame_buffer_rectangle, onDrawCell)
                );
            }
            else
            {
                for (Point local_point = Point.Empty; local_point.Y < rectangle.Height; local_point.Y++)
                {
                    for (local_point.X = 0; local_point.X < rectangle.Width; local_point.X++)
                    {
                        DrawCell(local_point, rectangle.Location, frame_buffer_rectangle, onDrawCell);
                    }
                }
            }
        }

        public bool WriteCurrentConsoleFrameBufferToConsole(IConsoleWriter consoleWriter)
        {
            TaskStatus previous_write_changes_to_console_task_status = previousWriteChangesToConsoleTask.Status;
            bool ret =
                (previous_write_changes_to_console_task_status == TaskStatus.RanToCompletion) ||
                (previous_write_changes_to_console_task_status == TaskStatus.Canceled) ||
                (previous_write_changes_to_console_task_status == TaskStatus.Faulted);
            if (ret)
            {
                (StagedConsoleFrameBuffer, PreviouslyStagedConsoleFrameBuffer, CurrentConsoleFrameBuffer) =
                    (CurrentConsoleFrameBuffer, StagedConsoleFrameBuffer, PreviouslyStagedConsoleFrameBuffer);
                IConsoleFrameBuffer staged_console_frame_buffer = StagedConsoleFrameBuffer;
                IConsoleFrameBuffer previously_staged_console_frame_buffer = PreviouslyStagedConsoleFrameBuffer;
                IConsoleWriter console_writer = consoleWriter;
                rectangleStack.Clear();
                CurrentConsoleFrameBuffer.Clear();
                previousWriteChangesToConsoleTask =
                    Task.Run
                    (
                        () =>
                        {
                            bool is_drawing_not_initialized = true;
                            ConsoleFrameBufferCell previous_staged_console_frame_buffer_cell = ConsoleFrameBufferCell.Empty;
                            Color last_applied_foreground_color = Color.Empty;
                            if (staged_console_frame_buffer.Size == previously_staged_console_frame_buffer.Size)
                            {
                                Color last_applied_background_color = Color.Empty;
                                int cursor_position_index = 0;
                                for
                                (
                                    int staged_console_frame_buffer_cell_index = 0;
                                    staged_console_frame_buffer_cell_index < staged_console_frame_buffer.FrameBufferCells.Length;
                                    staged_console_frame_buffer_cell_index++
                                )
                                {
                                    ConsoleFrameBufferCell previously_staged_console_frame_buffer_cell =
                                        previously_staged_console_frame_buffer.FrameBufferCells[staged_console_frame_buffer_cell_index];
                                    ConsoleFrameBufferCell staged_console_frame_buffer_cell =
                                        staged_console_frame_buffer.FrameBufferCells[staged_console_frame_buffer_cell_index];
                                    if (previously_staged_console_frame_buffer_cell != staged_console_frame_buffer_cell)
                                    {
                                        if (cursor_position_index != staged_console_frame_buffer_cell_index)
                                        {
                                            WriteSetCursorPositionCommand
                                            (
                                                new
                                                (
                                                    staged_console_frame_buffer_cell_index % staged_console_frame_buffer.Size.Width,
                                                    staged_console_frame_buffer_cell_index / staged_console_frame_buffer.Size.Width
                                                ),
                                                console_writer,
                                                ref is_drawing_not_initialized
                                            );
                                        }
                                        Color finalized_foreground_color = staged_console_frame_buffer_cell.FinalizedForegroundColor;
                                        Color finalized_background_color = staged_console_frame_buffer_cell.FinalizedBackgroundColor;
                                        bool is_cell_empty = staged_console_frame_buffer_cell.IsEmpty;
                                        if (!is_cell_empty && (last_applied_foreground_color != finalized_foreground_color))
                                        {
                                            WriteForegroundColorCommand(finalized_foreground_color, console_writer, ref is_drawing_not_initialized);
                                            last_applied_foreground_color = finalized_foreground_color;
                                        }
                                        if (last_applied_background_color != finalized_background_color)
                                        {
                                            WriteBackgroundColorCommand(finalized_background_color, console_writer, ref is_drawing_not_initialized);
                                            last_applied_background_color = finalized_background_color;
                                        }
                                        WriteString
                                        (
                                            is_cell_empty ? spaceString : staged_console_frame_buffer_cell.Character.ToString(),
                                            console_writer,
                                            ref is_drawing_not_initialized
                                        );
                                        previous_staged_console_frame_buffer_cell = staged_console_frame_buffer_cell;
                                        cursor_position_index = staged_console_frame_buffer_cell_index + 1;
                                    }
                                }
                            }
                            else
                            {
                                foreach (ConsoleFrameBufferCell staged_console_frame_buffer_cell in staged_console_frame_buffer.FrameBufferCells)
                                {
                                    Color finalized_foreground_color = staged_console_frame_buffer_cell.FinalizedForegroundColor;
                                    Color finalized_background_color = staged_console_frame_buffer_cell.FinalizedBackgroundColor;
                                    bool is_cell_empty = staged_console_frame_buffer_cell.IsEmpty;
                                    if (!is_cell_empty && (last_applied_foreground_color != finalized_foreground_color))
                                    {
                                        WriteForegroundColorCommand(finalized_foreground_color, console_writer, ref is_drawing_not_initialized);
                                        last_applied_foreground_color = finalized_foreground_color;
                                    }
                                    if (previous_staged_console_frame_buffer_cell.FinalizedBackgroundColor != finalized_background_color)
                                    {
                                        WriteBackgroundColorCommand(finalized_background_color, console_writer, ref is_drawing_not_initialized);
                                    }
                                    WriteString
                                    (
                                        is_cell_empty ? spaceString : staged_console_frame_buffer_cell.Character.ToString(),
                                        console_writer,
                                        ref is_drawing_not_initialized
                                    );
                                    previous_staged_console_frame_buffer_cell = staged_console_frame_buffer_cell;
                                }
                            }
                            if (!is_drawing_not_initialized)
                            {
                                console_writer.FlushCommands();
                            }
                        }
                    );
            }
            return ret;
        }

        public ValueTask DisposeAsync() => new(previousWriteChangesToConsoleTask);

        public void Dispose() => DisposeAsync().AsTask().GetAwaiter().GetResult();
    }
}
