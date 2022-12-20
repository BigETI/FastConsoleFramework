using FastConsoleFramework.UI;
using System.Drawing;

namespace FastConsoleFramework.Renderer
{
    public interface IConsoleDrawContext : IAsyncDisposable, IDisposable
    {
        IConsoleFrameBuffer CurrentConsoleFrameBuffer { get; }

        IConsoleFrameBuffer StagedConsoleFrameBuffer { get; }

        IConsoleFrameBuffer PreviouslyStagedConsoleFrameBuffer { get; }

        Rectangle Rectangle { get; }

        bool IsRectangleNotEmpty { get; }

        void PushRectangle(Rectangle rectangle, EAlignment alignment);

        bool PopRectangle();

        void DrawCells(DrawCellDelegate onDrawCell);

        void DrawCells(DrawCellDelegate onDrawCell, bool isUsingParallelForLoop);

        bool WriteCurrentConsoleFrameBufferToConsole(IConsoleWriter consoleWriter);
    }
}
