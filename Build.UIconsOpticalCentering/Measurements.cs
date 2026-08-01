using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Build.UIconsOpticalCentering
{
    /// <summary>What the browser reported for a single glyph, in pixels relative to the pen position and baseline.</summary>
    internal sealed class GlyphMeasurement
    {
        public string IconName { get; set; }

        /// <summary>Advance width, i.e. the width of the box the browser lays the glyph out in.</summary>
        public double Advance { get; set; }

        public double InkLeft   { get; set; }
        public double InkRight  { get; set; }
        public double InkTop    { get; set; }
        public double InkBottom { get; set; }

        /// <summary>Centre of ink mass, i.e. where the glyph's weight sits.</summary>
        public double CentroidX { get; set; }

        public double CentroidY { get; set; }

        /// <summary>Trimmed ink extents: the interval holding all but a small fraction of the ink on each side.</summary>
        public double TrimLeft   { get; set; }

        public double TrimRight  { get; set; }
        public double TrimTop    { get; set; }
        public double TrimBottom { get; set; }

        /// <summary>Total ink, in fully covered pixels.</summary>
        public double Mass { get; set; }

        public string Status { get; set; }

        public bool IsUsable => Status == "ok";
    }

    /// <summary>The metrics of one font plus the measurement of each of its glyphs.</summary>
    internal sealed class FontMeasurement
    {
        public string FontFamily { get; set; }
        public double Em         { get; set; }

        /// <summary>Ascent/descent as used by inline layout (read from the dom).</summary>
        public double Ascent { get; set; }

        public double Descent { get; set; }

        /// <summary>Ascent/descent as reported by canvas, kept to cross-check the dom numbers.</summary>
        public double CanvasAscent { get; set; }

        public double CanvasDescent { get; set; }

        /// <summary>Advance of the probe glyph as laid out by the dom, cross-checked against canvas.</summary>
        public double DomAdvance { get; set; }

        public int CellWidth  { get; set; }
        public int CellHeight { get; set; }

        public List<GlyphMeasurement> Glyphs { get; } = new List<GlyphMeasurement>();

        /// <summary>Centre of the layout box, relative to the baseline. Independent of line-height.</summary>
        public double BoxCenterY => (Descent - Ascent) / 2;

        public static FontMeasurement Parse(string csv)
        {
            var result = new FontMeasurement();

            foreach (var line in csv.Split('\n'))
            {
                if (line.Length == 0) continue;

                var parts = line.Split(';');

                if (parts[0] == "#font")
                {
                    result.FontFamily    = parts[1];
                    result.Em            = Number(parts[2]);
                    result.Ascent        = Number(parts[3]);
                    result.Descent       = Number(parts[4]);
                    result.CanvasAscent  = Number(parts[5]);
                    result.CanvasDescent = Number(parts[6]);
                    result.DomAdvance    = Number(parts[7]);
                    result.CellWidth     = (int)Number(parts[8]);
                    result.CellHeight    = (int)Number(parts[9]);
                    continue;
                }

                result.Glyphs.Add(new GlyphMeasurement
                {
                    IconName   = parts[0],
                    Advance    = Number(parts[1]),
                    InkLeft    = Number(parts[2]),
                    InkRight   = Number(parts[3]),
                    InkTop     = Number(parts[4]),
                    InkBottom  = Number(parts[5]),
                    CentroidX  = Number(parts[6]),
                    CentroidY  = Number(parts[7]),
                    TrimLeft   = Number(parts[8]),
                    TrimRight  = Number(parts[9]),
                    TrimTop    = Number(parts[10]),
                    TrimBottom = Number(parts[11]),
                    Mass       = Number(parts[12]),
                    Status     = parts[13],
                });
            }

            return result;
        }

        private static double Number(string value) => double.Parse(value, CultureInfo.InvariantCulture);
    }
}
