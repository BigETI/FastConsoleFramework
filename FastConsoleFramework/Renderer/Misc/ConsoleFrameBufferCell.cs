using System.Drawing;

namespace FastConsoleFramework.Renderer
{
    public readonly struct ConsoleFrameBufferCell
    {
        public static ConsoleFrameBufferCell Empty { get; } = new ConsoleFrameBufferCell(' ', Color.Empty, Color.Empty);

        public readonly char Character { get; }

        public readonly Color ForegroundColor { get; }

        public readonly Color BackgroundColor { get; }

        public readonly Color FinalizedBackgroundColor
        {
            get
            {
                float alpha = BackgroundColor.A / (float)0xFF;
                return Color.FromArgb
                (
                    0xFF,
                    Math.Clamp((int)(BackgroundColor.R * alpha), 0x0, 0xFF),
                    Math.Clamp((int)(BackgroundColor.G * alpha), 0x0, 0xFF),
                    Math.Clamp((int)(BackgroundColor.B * alpha), 0x0, 0xFF)
                );
            }
        }

        public readonly Color FinalizedForegroundColor
        {
            get
            {
                Color finalized_foreground_color = BackgroundColor.GetAlphaBlendedColor(ForegroundColor);
                return Color.FromArgb
                (
                    0xFF,
                    finalized_foreground_color.R,
                    finalized_foreground_color.G,
                    finalized_foreground_color.B
                );
            }
        }

        public bool IsEmpty => BackgroundColor.A <= 0x0 && (ForegroundColor.A <= 0x0 || char.IsWhiteSpace(Character));

        public ConsoleFrameBufferCell(char character, Color foregroundColor, Color backgroundColor)
        {
            Character = character;
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
        }

        public static ConsoleFrameBufferCell AlphaBlend(ConsoleFrameBufferCell baseConsoleFrameBufferCell, ConsoleFrameBufferCell appendConsoleFrameBufferCell)
        {
            ConsoleFrameBufferCell ret;
            if (appendConsoleFrameBufferCell.IsEmpty)
            {
                ret = baseConsoleFrameBufferCell;
            }
            else
            {
                bool is_append_character_a_white_space = char.IsWhiteSpace(appendConsoleFrameBufferCell.Character);
                ret =
                    new
                    (
                        is_append_character_a_white_space ? baseConsoleFrameBufferCell.Character : appendConsoleFrameBufferCell.Character,
                        (
                            is_append_character_a_white_space ?
                                baseConsoleFrameBufferCell.ForegroundColor :
                                appendConsoleFrameBufferCell.ForegroundColor
                        ).GetAlphaBlendedColor(appendConsoleFrameBufferCell.BackgroundColor),
                        baseConsoleFrameBufferCell.BackgroundColor.GetAlphaBlendedColor(appendConsoleFrameBufferCell.BackgroundColor)
                    );
            }
            return ret;
        }

        public static bool operator ==(ConsoleFrameBufferCell left, ConsoleFrameBufferCell right) =>
            left.Character == right.Character &&
            left.BackgroundColor == right.BackgroundColor &&
            left.ForegroundColor == right.ForegroundColor;

        public static bool operator !=(ConsoleFrameBufferCell left, ConsoleFrameBufferCell right) =>
            left.Character != right.Character ||
            left.BackgroundColor != right.BackgroundColor ||
            left.ForegroundColor != right.ForegroundColor;

        public ConsoleFrameBufferCell GetAlphaBlendedFrameBufferCell(ConsoleFrameBufferCell appendConsoleFrameBufferCell) =>
            AlphaBlend(this, appendConsoleFrameBufferCell);

        public override bool Equals(object? obj) => (obj is ConsoleFrameBufferCell console_frame_buffer_cell) && (this == console_frame_buffer_cell);

        public override int GetHashCode() => HashCode.Combine(Character, ForegroundColor, BackgroundColor);
    }
}
