using System;

namespace Tesserae
{
    /// <summary>
    /// Represents a color in the Hue-Saturation-Luminosity (HSL) color space.
    /// </summary>
    [H5.Name("tss.HSLColor")]
    public class HSLColor
    {
        private static readonly Random _rng = new Random();

        // Private data members below are on scale 0-1, they are scaled for use externally based on scale
        private       double _hue        = 1.0;
        private       double _saturation = 1.0;
        private       double _luminosity = 1.0;
        private const double _scale      = 240.0;
        private const double _scaleHue   = 360.0;

        /// <summary>
        /// Gets or sets the hue component. Range: 0 to 360.
        /// </summary>
        public double Hue
        {
            get { return _hue * _scaleHue; }
            set { _hue = CheckRange(value / _scaleHue); }
        }

        /// <summary>
        /// Gets or sets the saturation component. Range: 0 to 240.
        /// </summary>
        public double Saturation
        {
            get { return _saturation * _scale; }
            set { _saturation = CheckRange(value / _scale); }
        }

        /// <summary>
        /// Gets or sets the luminosity component. Range: 0 to 240.
        /// </summary>
        public double Luminosity
        {
            get { return _luminosity * _scale; }
            set { _luminosity = CheckRange(value / _scale); }
        }

        private static double CheckRange(double value)
        {
            if (value      < 0.0) value = 0.0;
            else if (value > 1.0) value = 1.0;
            return value;
        }

        public override string ToString()
        {
            return string.Format("H: {0:#0.##} S: {1:#0.##} L: {2:#0.##}", Hue, Saturation, Luminosity);
        }

        /// <summary>
        /// Returns the CSS rgb() string representation of this <see cref="HSLColor"/>.
        /// </summary>
        /// <returns>A string like "rgb(r, g, b)".</returns>
        public string ToRGB()
        {
            return ((Color)this).ToRGB();
        }

        /// <summary>
        /// Returns the CSS rgba() string representation of this <see cref="HSLColor"/> with the specified opacity.
        /// </summary>
        /// <param name="opacity">The opacity (0.0 to 1.0).</param>
        /// <returns>A string like "rgba(r, g, b, a)".</returns>
        public string ToRGBA(float opacity)
        {
            return ((Color)this).ToRGBA(opacity);
        }

        /// <summary>
        /// Returns a comma-separated string of the RGB components.
        /// </summary>
        /// <returns>A string like "r, g, b".</returns>
        public string ToRGBvar()
        {
            return ((Color)this).ToRGBvar();
        }

        /// <summary>
        /// Returns a comma-separated string of the RGB components and the specified opacity.
        /// </summary>
        /// <param name="opacity">The opacity (0.0 to 1.0).</param>
        /// <returns>A string like "r, g, b, a".</returns>
        public string ToRGBAvar(float opacity)
        {
            return ((Color)this).ToRGBAvar(opacity);
        }

        /// <summary>
        /// Returns the hex string representation of this <see cref="HSLColor"/>.
        /// </summary>
        /// <returns>A hex string (e.g., "#RRGGBB").</returns>
        public string ToHex()
        {
            var c = (Color)this;
            return $"#{c.R:X2}{c.G:X2}{c.B:X2}";
        }

        /// <summary>
        /// Creates a random <see cref="HSLColor"/>.
        /// </summary>
        /// <returns>A random <see cref="HSLColor"/>.</returns>
        public static HSLColor Random()
        {
            return new HSLColor(_rng.NextDouble() * _scaleHue, _rng.NextDouble() * _scale, 0.5 * _scale);
        }

        public static implicit operator Color(HSLColor hslColor)
        {
            double r = 0, g = 0, b = 0;

            if (hslColor._luminosity != 0)
            {
                if (hslColor._saturation == 0)
                {
                    r = g = b = hslColor._luminosity;
                }
                else
                {
                    double temp2 = GetTemp2(hslColor);
                    double temp1 = 2.0 * hslColor._luminosity - temp2;

                    r = GetColorComponent(temp1, temp2, hslColor._hue + 1.0 / 3.0);
                    g = GetColorComponent(temp1, temp2, hslColor._hue);
                    b = GetColorComponent(temp1, temp2, hslColor._hue - 1.0 / 3.0);
                }
            }
            return Color.FromArgb((byte)(255 * r), (byte)(255 * g), (byte)(255 * b));
        }

        private static double GetColorComponent(double temp1, double temp2, double temp3)
        {
            temp3 = MoveIntoRange(temp3);

            if (temp3      < 1.0 / 6.0) return temp1 + (temp2 - temp1) * 6.0 * temp3;
            else if (temp3 < 0.5) return temp2;
            else if (temp3 < 2.0 / 3.0) return temp1 + ((temp2 - temp1) * ((2.0 / 3.0) - temp3) * 6.0);
            else return temp1;
        }
        private static double MoveIntoRange(double temp3)
        {
            if (temp3      < 0.0) temp3 += 1.0;
            else if (temp3 > 1.0) temp3 -= 1.0;
            return temp3;
        }
        private static double GetTemp2(HSLColor hslColor)
        {
            double temp2;

            if (hslColor._luminosity < 0.5) temp2 = hslColor._luminosity * (1.0 + hslColor._saturation);
            else temp2                            = hslColor._luminosity + hslColor._saturation - (hslColor._luminosity * hslColor._saturation);
            return temp2;
        }

        public static implicit operator HSLColor(Color color)
        {
            return new HSLColor
            {
                _hue        = color.GetHue() / _scaleHue, // we store hue as 0-1 as opposed to 0-360
                _luminosity = color.GetBrightness(),
                _saturation = color.GetSaturation()
            };
        }

        /// <summary>
        /// Sets the RGB components of this <see cref="HSLColor"/>.
        /// </summary>
        /// <param name="red">The red component.</param>
        /// <param name="green">The green component.</param>
        /// <param name="blue">The blue component.</param>
        public void SetRGB(byte red, byte green, byte blue)
        {
            HSLColor hslColor = (HSLColor)Color.FromArgb(red, green, blue);
            _hue        = hslColor._hue;
            _saturation = hslColor._saturation;
            _luminosity = hslColor._luminosity;
        }

        public HSLColor() { }
        public HSLColor(Color color)
        {
            _hue        = color.GetHue() / _scaleHue; // we store hue as 0-1 as opposed to 0-360
            _luminosity = color.GetBrightness();
            _saturation = color.GetSaturation();
        }
        public HSLColor(double hue, double saturation, double luminosity)
        {
            Hue        = hue;
            Saturation = saturation;
            Luminosity = luminosity;
        }
    }
}