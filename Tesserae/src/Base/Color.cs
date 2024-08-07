﻿using H5;
using System;
using static H5.Core.dom;

namespace Tesserae
{
    [H5.Name("tss.Color")]
    public class Color
    {
        public byte A { get; private set; }
        public byte B { get; private set; }
        public byte R { get; private set; }
        public byte G { get; private set; }

        private string Hex;

        public static Color FromArgb(byte r, byte g, byte b)
        {
            return new Color() { A = 255, R = r, G = g, B = b };
        }

        public static Color FromArgb(byte a, byte r, byte g, byte b)
        {
            return new Color() { A = a, R = r, G = g, B = b };
        }

        public static string EvalVar(string variableName)
        {
            if (variableName.StartsWith("var("))
            {
                variableName = variableName.Substring(4, variableName.Length - 5);
            }
            var color = window.getComputedStyle(document.body).getPropertyValue(variableName);
            return color;
        }

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
                var rgbString        = window.getComputedStyle(document.documentElement).getPropertyValue(cssColorVariable);
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

        public string ToHex()
        {
            if (Hex is null)
            {
                Hex = $"#{R:X2}{G:X2}{B:X2}";
            }
            return Hex;
        }

        public string ToRGB()
        {
            return $"rgb({R:#0.##}, {G:#0.##}, {B:#0.##})";
        }

        public string ToRGBvar()
        {
            return $"{R:#0.##}, {G:#0.##}, {B:#0.##}";
        }

        public string ToRGBA(float opacity)
        {
            return $"rgba({R:#0.##}, {G:#0.##}, {B:#0.##}, {opacity:#0.##})";
        }

        public string ToRGBAvar(float opacity)
        {
            return $"{R:#0.##}, {G:#0.##}, {B:#0.##}, {opacity:#0.##}";
        }
    }
}