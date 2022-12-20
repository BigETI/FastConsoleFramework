using System.Drawing;

namespace FastConsoleFramework.Renderer
{
    public interface IConsoleFrameBuffer
    {
        ConsoleFrameBufferCell[] FrameBufferCells { get; }

        Size Size { get; set; }

        event FrameBufferSizeChangedDelegate? OnFrameBufferSizeChanged;

        void Resize(Size size, EAlignment alignment);

        void Resize(Size size, EAlignment alignment, bool isUsingParallelForLoop);

        void Clear();

        void Clear(bool isUsingParallelForLoop);
    }
}
