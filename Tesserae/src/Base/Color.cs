using H5;
using System;
using static H5.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// Represents a color with alpha, red, green, and blue components.
    /// </summary>
    [H5.Name("tss.Color")]
    public class Color
    {
        /// <summary>
        /// Gets the alpha component value of the color.
        /// </summary>
        public byte A { get; private set; }
        /// <summary>
        /// Gets the blue component value of the color.
        /// </summary>
        public byte B { get; private set; }
        /// <summary>
        /// Gets the red component value of the color.
        /// </summary>
        public byte R { get; private set; }
        /// <summary>
        /// Gets the green component value of the color.
        /// </summary>
        public byte G { get; private set; }

        private string Hex;

        /// <summary>
        /// Creates a <see cref="Color"/> from the specified 8-bit color values (red, green, and blue). The alpha value is implicitly 255 (fully opaque).
        /// </summary>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        /// <returns>The <see cref="Color"/> that this method creates.</returns>
        public static Color FromArgb(byte r, byte g, byte b)
        {
            return new Color() { A = 255, R = r, G = g, B = b };
        }

        /// <summary>
        /// Creates a <see cref="Color"/> from the four ARGB component (alpha, red, green, and blue) values.
        /// </summary>
        /// <param name="a">The alpha component.</param>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        /// <returns>The <see cref="Color"/> that this method creates.</returns>
        public static Color FromArgb(byte a, byte r, byte g, byte b)
        {
            return new Color() { A = a, R = r, G = g, B = b };
        }

        /// <summary>
        /// Evaluates a CSS variable and returns its value.
        /// </summary>
        /// <param name="variableName">The name of the CSS variable (e.g., "--my-color" or "var(--my-color)").</param>
        /// <returns>The value of the CSS variable.</returns>
        public static string EvalVar(string variableName)
        {
            if (variableName.StartsWith("var("))
            {
                variableName = variableName.Substring(4, variableName.Length - 5);
            }
            var color = window.getComputedStyle(document.body).getPropertyValue(variableName);
            return color;
        }

        /// <summary>
        /// Gets the hue-saturation-lightness (HSL) hue value, in degrees, for this <see cref="Color"/>.
        /// </summary>
        /// <returns>The hue, in degrees, of this <see cref="Color"/>. The hue is measured in degrees, ranging from 0.0 through 360.0, in HSL color space.</returns>
        public float GetHue()
        {
            if (R == G && G == B)
                return 0; // 0 makes as good an UNDEFINED value as any

            float r = R / 255.0f;
            float g = G / 255.0f;
            float b = B / 255.0f;

            float max, min;
            float delta;
            float hue = 0.0f;

            max = r;
            min = r;

            if (g > max) max = g;
            if (b > max) max = b;

            if (g < min) min = g;
            if (b < min) min = b;

            delta = max - min;

            if (r == max)
            {
                hue = (g - b) / delta;
            }
            else if (g == max)
            {
                hue = 2 + (b - r) / delta;
            }
            else if (b == max)
            {
                hue = 4 + (r - g) / delta;
            }
            hue *= 60;

            if (hue < 0.0f)
            {
                hue += 360.0f;
            }
            return hue;
        }

        /// <summary>
        /// Gets the hue-saturation-lightness (HSL) lightness value for this <see cref="Color"/>.
        /// </summary>
        /// <returns>The lightness of this <see cref="Color"/>. The lightness ranges from 0.0 through 1.0, where 0.0 represents black and 1.0 represents white.</returns>
        public float GetBrightness()
        {
            float r = R / 255.0f;
            float g = G / 255.0f;
            float b = B / 255.0f;

            float max, min;

            max = r;
            min = r;

            if (g > max) max = g;
            if (b > max) max = b;

            if (g < min) min = g;
            if (b < min) min = b;

            return (max + min) / 2;
        }

        /// <summary>
        /// Gets the hue-saturation-lightness (HSL) saturation value for this <see cref="Color"/>.
        /// </summary>
        /// <returns>The saturation of this <see cref="Color"/>. The saturation ranges from 0.0 through 1.0, where 0.0 is grayscale and 1.0 is the most saturated.</returns>
        public float GetSaturation()
        {
            float r = R / 255.0f;
            float g = G / 255.0f;
            float b = B / 255.0f;

            float max, min;
            float l,   s = 0;

            max = r;
            min = r;

            if (g > max) max = g;
            if (b > max) max = b;

            if (g < min) min = g;
            if (b < min) min = b;

            // if max == min, then there is no color and the saturation is zero.
            if (max != min)
            {
                l = (max + min) / 2;

                if (l <= .5)
                {
                    s = (max - min) / (max + min);
                }
                else
                {
                    s = (max - min) / (2 - max - min);
                }
            }
            return s;
        }

        /// <summary>
        /// Creates a <see cref="Color"/> from a string representation. Supports hex, rgb, rgba and CSS variables.
        /// </summary>
        /// <param name="hexString">The color string.</param>
        /// <returns>The <see cref="Color"/>.</returns>
        public static Color FromString(string hexString)
        {
            byte r = 0, g = 0, b = 0;

            if (hexString.Contains("rgb("))
            {
                hexString = hexString.Replace("rgb", "").Replace(" ", "").Replace("(", "").Replace(")", "");
                var parts = hexString.Split(new[] { ',' });
                r = byte.Parse(parts[0]);
                g = byte.Parse(parts[1]);
                b = byte.Parse(parts[2]);
                return FromArgb(255, r, g, b);
            }
            else if (hexString.Contains("rgba("))
            {
                hexString = hexString.Replace("rgba", "").Replace(" ", "").Replace("(", "").Replace(")", "");
                var parts = hexString.Split(new[] { ',' });
                r = byte.Parse(parts[0]);
                g = byte.Parse(parts[1]);
                b = byte.Parse(parts[2]);
                var a = float.Parse(parts[3]);
                return FromArgb((byte)(a * 255), r, g, b);
            }
            else if (hexString.Contains("var("))
            {
                int prefixLength     = "var(".Length;
                var cssColorVariable = hexString.Substring(prefixLength, hexString.Length - prefixLength - 1);
                var rgbString        = window.getComputedStyle(document.body).getPropertyValue(cssColorVariable);
                return FromString(rgbString);
            }

            var hex = hexString.TrimStart('#');

            if (hex.Length == 3)
            {
                hex = hex.Substring(0, 1) + hex.Substring(0, 1) + hex.Substring(1, 1) + hex.Substring(1, 1) + hex.Substring(2, 1) + hex.Substring(2, 1);
            }

            if (hex.Length != 6) throw new ArgumentException();


            Script.Write("var bigint = parseInt(hex, 16); var r = (bigint >> 16) & 255; var g = (bigint >> 8) & 255; var b = bigint & 255;");

            return FromArgb(255, r, g, b);


            //if (!System.Text.RegularExpressions.Regex.IsMatch(hexString, @"[#]([0-9]|[a-f]|[A-F]){6}\b")) throw new ArgumentException();

            //var value = Convert.ToUInt32(hexString.Substring(1), 16);

            //return FromArgb((byte)((value >> 24) & 0xFF),
            //                 (byte)((value >> 16) & 0xFF),
            //                 (byte)((value >> 8) & 0xFF),
            //                 (byte)(value & 0xFF));
        }

        /// <summary>
        /// Returns the hex string representation of this <see cref="Color"/>.
        /// </summary>
        /// <returns>A hex string (e.g., "#RRGGBB").</returns>
        public string ToHex()
        {
            if (Hex is null)
            {
                Hex = $"#{R:X2}{G:X2}{B:X2}";
            }
            return Hex;
        }

        /// <summary>
        /// Returns the CSS rgb() string representation of this <see cref="Color"/>.
        /// </summary>
        /// <returns>A string like "rgb(r, g, b)".</returns>
        public string ToRGB()
        {
            return $"rgb({R:#0.##}, {G:#0.##}, {B:#0.##})";
        }

        /// <summary>
        /// Returns a comma-separated string of the RGB components.
        /// </summary>
        /// <returns>A string like "r, g, b".</returns>
        public string ToRGBvar()
        {
            return $"{R:#0.##}, {G:#0.##}, {B:#0.##}";
        }

        /// <summary>
        /// Returns the CSS rgba() string representation of this <see cref="Color"/> with the specified opacity.
        /// </summary>
        /// <param name="opacity">The opacity (0.0 to 1.0).</param>
        /// <returns>A string like "rgba(r, g, b, a)".</returns>
        public string ToRGBA(float opacity)
        {
            return $"rgba({R:#0.##}, {G:#0.##}, {B:#0.##}, {opacity:#0.##})";
        }

        /// <summary>
        /// Returns a comma-separated string of the RGB components and the specified opacity.
        /// </summary>
        /// <param name="opacity">The opacity (0.0 to 1.0).</param>
        /// <returns>A string like "r, g, b, a".</returns>
        public string ToRGBAvar(float opacity)
        {
            return $"{R:#0.##}, {G:#0.##}, {B:#0.##}, {opacity:#0.##}";
        }
    }
}