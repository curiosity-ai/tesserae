using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Build.UIconsOpticalCentering
{
    /// <summary>
    /// Writes every measurement and every intermediate number to a csv, so a questionable adjustment can
    /// be traced back to the pixels it came from without re-running the browser.
    /// </summary>
    internal static class MeasurementDump
    {
        public static void Write(string path, List<FontAdjustments> fonts)
        {
            var sb = new StringBuilder();

            sb.AppendLine(string.Join(",",
                "font", "class", "icon", "status", "advance_em", "box_center_y_em",
                "ink_left_em", "ink_right_em", "ink_top_em", "ink_bottom_em",
                "trim_left_em", "trim_right_em", "trim_top_em", "trim_bottom_em",
                "centroid_x_em", "centroid_y_em", "mass_em2",
                "frame_x", "frame_y", "optical_x", "optical_y",
                "target_x", "target_y", "rejected_x", "rejected_y", "css_left", "css_top",
                "frame_cluster", "frame_cluster_size", "shares_frame"));

            foreach (var font in fonts)
            {
                var em = font.Measurement.Em;

                foreach (var glyph in font.Glyphs)
                {
                    var m = glyph.Measurement;

                    sb.AppendLine(string.Join(",",
                        font.Font.FontFamily,
                        glyph.Glyph.CssClass,
                        glyph.Glyph.IconName,
                        m.Status,
                        N(m.Advance / em), N(font.Measurement.BoxCenterY / em),
                        N(m.InkLeft / em), N(m.InkRight / em), N(m.InkTop / em), N(m.InkBottom / em),
                        N(m.TrimLeft / em), N(m.TrimRight / em), N(m.TrimTop / em), N(m.TrimBottom / em),
                        N(m.CentroidX / em), N(m.CentroidY / em), N(m.Mass / (em * em)),
                        N(glyph.FrameX), N(glyph.FrameY), N(glyph.OpticalX), N(glyph.OpticalY),
                        N(glyph.TargetX), N(glyph.TargetY),
                        glyph.RejectedX ? "1" : "0", glyph.RejectedY ? "1" : "0",
                        N(glyph.X), N(glyph.Y),
                        glyph.FrameCluster.ToString(CultureInfo.InvariantCulture),
                        glyph.FrameClusterSize.ToString(CultureInfo.InvariantCulture),
                        glyph.SharesFrame ? "1" : "0"));
                }
            }

            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
        }

        private static string N(double value) => value.ToString("0.#####", CultureInfo.InvariantCulture);
    }
}
