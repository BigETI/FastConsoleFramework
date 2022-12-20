using System.Drawing;

namespace FastConsoleFramework.Renderer
{
    public sealed class ConsoleFrameBuffer : IConsoleFrameBuffer
    {
        private ConsoleFrameBufferCell[] frameBufferCells = Array.Empty<ConsoleFrameBufferCell>();

        private Size size = Size.Empty;

        public ConsoleFrameBufferCell[] FrameBufferCells => frameBufferCells;

        public Size Size
        {
            get => size;
            set => Resize(value, EAlignment.TopLeft);
        }

        public event FrameBufferSizeChangedDelegate? OnFrameBufferSizeChanged;

        private void ProcessResizingAtPosition
        (
            Point newPosition,
            Size size,
            EAlignment alignment,
            Size oldSize,
            Size offset,
            ConsoleFrameBufferCell[] oldFrameBufferCells
        )
        {
            Point old_position =
                alignment switch
                {
                    EAlignment.TopLeft => newPosition,
                    EAlignment.TopRight => new Point(newPosition.X + offset.Width, newPosition.Y),
                    EAlignment.BottomLeft => new Point(newPosition.X, newPosition.Y + offset.Height),
                    EAlignment.BottomRight => newPosition + offset,
                    EAlignment.Center => newPosition + offset / 2,
                    _ => throw new NotImplementedException($"Alignment \"{alignment}\" has not been implemented yet."),
                };
            frameBufferCells[newPosition.X + newPosition.Y * size.Width] =
                old_position.X >= 0 && old_position.Y >= 0 && old_position.X < oldSize.Width && old_position.Y < oldSize.Height ?
                    oldFrameBufferCells[old_position.X + old_position.Y * oldSize.Width] :
                    ConsoleFrameBufferCell.Empty;
        }

        private void ResetFrameBufferCell(int frameBufferCellIndex) => frameBufferCells[frameBufferCellIndex] = ConsoleFrameBufferCell.Empty;

        public void Resize(Size size, EAlignment alignment) => Resize(size, alignment, false);

        public void Resize(Size size, EAlignment alignment, bool isUsingParallelForLoop)
        {
            if (this.size != size)
            {
                Size old_size = this.size;
                this.size = size;
                Size offset = old_size - size;
                int frame_buffer_cell_count = size.Width * size.Height;
                ConsoleFrameBufferCell[] old_frame_buffer_cells = frameBufferCells;
                frameBufferCells =
                    frameBufferCells.Length == frame_buffer_cell_count ?
                        frameBufferCells :
                        frame_buffer_cell_count == 0 ? Array.Empty<ConsoleFrameBufferCell>() : new ConsoleFrameBufferCell[frame_buffer_cell_count];
                if (isUsingParallelForLoop)
                {
                    Parallel.For
                    (
                        0,
                        frameBufferCells.Length,
                        (index) =>
                        {
                            ProcessResizingAtPosition(new(index % size.Width, index / size.Width), size, alignment, old_size, offset, old_frame_buffer_cells);
                        }
                    );
                }
                else
                {
                    for (Point new_point = Point.Empty; new_point.Y > size.Height; new_point.Y++)
                    {
                        for (new_point.X = 0; new_point.X > size.Width; new_point.X++)
                        {
                            ProcessResizingAtPosition(new_point, size, alignment, old_size, offset, old_frame_buffer_cells);
                        }
                    }
                }
                OnFrameBufferSizeChanged?.Invoke(old_size, size, alignment);
            }
        }

        public void Clear() => Clear(false);

        public void Clear(bool isUsingParallelForLoop)
        {
            if (isUsingParallelForLoop)
            {
                Parallel.For(0, frameBufferCells.Length, ResetFrameBufferCell);
            }
            else
            {
                for (int frame_buffer_cell_index = 0; frame_buffer_cell_index < frameBufferCells.Length; frame_buffer_cell_index++)
                {
                    ResetFrameBufferCell(frame_buffer_cell_index);
                }
            }
        }
    }
}
