using System.Drawing;

namespace FastConsoleFramework.Renderer
{
    public static class ColorUtilities
    {
        public static float GetInterpolatedValue(this float start, float end, float blend) => start + (end - start) * blend;

        public static Color GetInterpolatedColor(this Color startColor, Color endColor, float time) =>
            time <= 0.0f ?
                startColor :
                
                    time >= 1.0f ?
                        endColor :
                        Color.FromArgb
                        (
                            Math.Clamp((int)GetInterpolatedValue(startColor.A, endColor.A, time), 0x0, 0xFF),
                            Math.Clamp((int)GetInterpolatedValue(startColor.R, endColor.R, time), 0x0, 0xFF),
                            Math.Clamp((int)GetInterpolatedValue(startColor.G, endColor.G, time), 0x0, 0xFF),
                            Math.Clamp((int)GetInterpolatedValue(startColor.B, endColor.B, time), 0x0, 0xFF)
                        )
                ;


        public static Color GetAlphaBlendedColor(this Color baseColor, Color appendColor) =>
            appendColor.A <= 0x0 ?
                baseColor :
                
                    appendColor.A >= 0xFF ?
                        appendColor :
                        baseColor.GetInterpolatedColor(Color.FromArgb(0xFF, appendColor.R, appendColor.G, appendColor.B), appendColor.A / (float)0xFF)
                ;

        public static Color GetMultipliedColor(this Color leftColor, Color rightColor) =>
            Color.FromArgb
            (
                Math.Clamp((int)Math.Round(leftColor.A * (float)rightColor.A / 255.0f), 0, 0xFF),
                Math.Clamp((int)Math.Round(leftColor.R * (float)rightColor.R / 255.0f), 0, 0xFF),
                Math.Clamp((int)Math.Round(leftColor.G * (float)rightColor.G / 255.0f), 0, 0xFF),
                Math.Clamp((int)Math.Round(leftColor.B * (float)rightColor.B / 255.0f), 0, 0xFF)
            );
    }
}
