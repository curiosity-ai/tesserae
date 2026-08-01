using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Build.UIconsOpticalCentering
{
    /// <summary>
    /// Builds the page the icons are measured on, and holds the browser side script that renders and
    /// measures one glyph at a time.
    /// </summary>
    internal static class MeasurementPage
    {
        public const string Path = "uicons-optical-centering.html";

        public static string BuildHtml(IEnumerable<IconFont> fonts)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!doctype html>");
            sb.AppendLine("<html lang=\"en\">");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset=\"utf-8\">");
            sb.AppendLine("<title>UIcons optical centering</title>");

            foreach (var font in fonts)
            {
                sb.AppendLine($"<link rel=\"stylesheet\" href=\"css/{font.FontFamily}.css\">");
            }

            sb.AppendLine("<style>");
            sb.AppendLine(PreviewCss);
            sb.AppendLine("</style>");
            sb.AppendLine("<style id=\"tss-adjustments\"></style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div id=\"preview\"></div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
            return sb.ToString();
        }

        private const string PreviewCss = """
            body { margin: 0; padding: 16px; background: #fff; color: #222; font: 12px/1.4 -apple-system, Segoe UI, Roboto, sans-serif; }
            .row { display: flex; align-items: center; gap: 12px; padding: 4px 0; }
            .name { width: 260px; font-family: ui-monospace, Consolas, monospace; }
            .delta { width: 150px; font-family: ui-monospace, Consolas, monospace; color: #777; }
            .cell { position: relative; width: 96px; height: 96px; border: 1px solid #d8d8d8; display: flex;
                    align-items: center; justify-content: center; font-size: 76px; line-height: 1; }
            .cell::before, .cell::after { content: ""; position: absolute; background: #ff2d55; opacity: .35; }
            .cell::before { left: 50%; top: 0; bottom: 0; width: 1px; }
            .cell::after { top: 50%; left: 0; right: 0; height: 1px; }
            .cell i { position: relative; z-index: 1; }
            .raw i::before { left: 0 !important; top: 0 !important; }
            .overlay i { position: absolute; }
            .overlay i:first-child { color: rgba(0, 90, 255, .55); }
            .overlay i + i { color: rgba(255, 45, 85, .55); }
            .caption { width: 100%; font-weight: 600; padding: 16px 0 4px; border-top: 1px solid #eee; }
            """;

        /// <summary>
        /// Rasterizes every glyph of one font and returns one line of metrics per glyph.
        /// <para>
        /// Everything is measured against the box the browser lays the glyph out in: horizontally the
        /// advance width, vertically the font's ascent/descent (read from the dom, because that is what
        /// inline layout uses). The centre of that box, relative to the pen position and baseline, is
        /// <c>(advance / 2, (descent - ascent) / 2)</c> whatever the line-height is, because a line-height
        /// larger than the content area adds the same half-leading above and below.
        /// </para>
        /// </summary>
        public const string MeasureFontScript = """
            async (request) => {
              const { family, em, trim, inkThreshold, maxCanvas } = request;
              const glyphs = request.glyphs;

              const text = (code) => String.fromCodePoint(code);

              // Make sure the webfont is actually available before anything is measured.
              await document.fonts.load(`${em}px "${family}"`, glyphs.slice(0, 64).map((g) => text(g.c)).join(""));
              await document.fonts.ready;

              // --- how the browser lays a glyph of this font out -------------------------------------
              const probeLine = document.createElement("div");
              probeLine.setAttribute("style",
                `position:absolute;left:0;top:0;visibility:hidden;white-space:nowrap;` +
                `font-family:"${family}";font-size:${em}px;line-height:normal;`);
              const baselineProbe = document.createElement("span");
              baselineProbe.setAttribute("style", "display:inline-block;width:0;height:0;vertical-align:baseline;");
              const glyphProbe = document.createElement("span");
              glyphProbe.textContent = text(glyphs[0].c);
              probeLine.appendChild(baselineProbe);
              probeLine.appendChild(glyphProbe);
              document.body.appendChild(probeLine);
              const baselineRect = baselineProbe.getBoundingClientRect();
              const glyphRect = glyphProbe.getBoundingClientRect();
              const dom = {
                ascent: baselineRect.top - glyphRect.top,
                descent: glyphRect.bottom - baselineRect.top,
                advance: glyphRect.width,
              };
              probeLine.remove();

              const canvas = document.createElement("canvas");
              const ctx = canvas.getContext("2d", { willReadFrequently: true });
              const setFont = () => {
                ctx.font = `${em}px "${family}"`;
                ctx.textAlign = "left";
                ctx.textBaseline = "alphabetic";
                ctx.fillStyle = "#fff";
              };
              canvas.width = canvas.height = 8;
              setFont();

              const probeMetrics = ctx.measureText(text(glyphs[0].c));
              const canvasAscent = probeMetrics.fontBoundingBoxAscent;
              const canvasDescent = probeMetrics.fontBoundingBoxDescent;

              // The dom is authoritative (it is what inline layout uses), but fall back to the canvas
              // metrics if the baseline probe produced something implausible.
              const domLooksSane = dom.ascent > em * 0.4 && dom.descent >= 0 && dom.ascent + dom.descent < em * 3;
              const ascent = domLooksSane ? dom.ascent : canvasAscent;
              const descent = domLooksSane ? dom.descent : canvasDescent;

              // --- advance widths, needed to size the raster cells ----------------------------------
              const advances = new Float64Array(glyphs.length);
              let maxAdvance = 0;
              for (let i = 0; i < glyphs.length; i++) {
                advances[i] = ctx.measureText(text(glyphs[i].c)).width;
                if (advances[i] > maxAdvance) maxAdvance = advances[i];
              }

              // --- raster grid ----------------------------------------------------------------------
              // Cells are padded generously so no glyph can bleed into its neighbour, and glyphs are
              // rasterized in batches so a single getImageData call covers many of them.
              const padX = Math.ceil(em * 0.5);
              const padY = Math.ceil(em * 0.4);
              const cellW = Math.ceil(maxAdvance) + 2 * padX;
              const cellH = Math.ceil(ascent + descent) + 2 * padY;
              const penX = padX;
              const baselineY = padY + Math.ceil(ascent);
              const cols = Math.max(1, Math.floor(maxCanvas / cellW));
              const rows = Math.max(1, Math.floor(maxCanvas / cellH));
              const perBatch = cols * rows;
              canvas.width = cols * cellW;
              canvas.height = rows * cellH;
              setFont();

              const colSum = new Float64Array(cellW);
              const rowSum = new Float64Array(cellH);

              // Smallest interval that holds all but `trim` of the ink mass, linearly interpolated inside
              // the boundary pixels so the result is sub-pixel accurate. Trimming keeps hairlines and
              // antialiasing from dictating where an icon's visual frame is.
              const axisStats = (profile, length) => {
                let total = 0;
                let weighted = 0;
                for (let i = 0; i < length; i++) {
                  total += profile[i];
                  weighted += profile[i] * (i + 0.5);
                }
                if (total <= 0) return null;

                const quantile = (target) => {
                  let cumulative = 0;
                  for (let i = 0; i < length; i++) {
                    if (cumulative + profile[i] >= target) return i + (target - cumulative) / profile[i];
                    cumulative += profile[i];
                  }
                  return length;
                };

                return { centroid: weighted / total, lo: quantile(total * trim), hi: quantile(total * (1 - trim)), mass: total };
              };

              const lines = [];

              for (let start = 0; start < glyphs.length; start += perBatch) {
                const count = Math.min(perBatch, glyphs.length - start);
                const usedRows = Math.ceil(count / cols);
                const usedW = canvas.width;
                const usedH = usedRows * cellH;

                // Cleared to transparent rather than painted black: the alpha channel is the ink coverage,
                // and an opaque backdrop would flatten alpha to 255 and switch on subpixel antialiasing.
                ctx.clearRect(0, 0, usedW, usedH);

                for (let i = 0; i < count; i++) {
                  const originX = (i % cols) * cellW;
                  const originY = Math.floor(i / cols) * cellH;
                  ctx.fillText(text(glyphs[start + i].c), originX + penX, originY + baselineY);
                }

                const data = ctx.getImageData(0, 0, usedW, usedH).data;

                for (let i = 0; i < count; i++) {
                  const glyph = glyphs[start + i];
                  const originX = (i % cols) * cellW;
                  const originY = Math.floor(i / cols) * cellH;

                  colSum.fill(0);
                  rowSum.fill(0);
                  let minX = cellW, maxX = -1, minY = cellH, maxY = -1;

                  for (let y = 0; y < cellH; y++) {
                    let rowTotal = 0;
                    let offset = ((originY + y) * usedW + originX) * 4 + 3;
                    for (let x = 0; x < cellW; x++, offset += 4) {
                      const alpha = data[offset];
                      if (alpha === 0) continue;
                      rowTotal += alpha;
                      colSum[x] += alpha;
                      if (alpha >= inkThreshold) {
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                      }
                    }
                    rowSum[y] = rowTotal;
                  }

                  const statsX = axisStats(colSum, cellW);
                  const statsY = axisStats(rowSum, cellH);

                  if (statsX === null || statsY === null || maxX < 0) {
                    lines.push([glyph.n, advances[start + i].toFixed(3), 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "empty"].join(";"));
                    continue;
                  }

                  const clipped = minX <= 0 || maxX >= cellW - 1 || minY <= 0 || maxY >= cellH - 1;
                  const r = (v) => v.toFixed(3);

                  lines.push([
                    glyph.n,
                    r(advances[start + i]),
                    r(minX - penX), r(maxX + 1 - penX),
                    r(minY - baselineY), r(maxY + 1 - baselineY),
                    r(statsX.centroid - penX), r(statsY.centroid - baselineY),
                    r(statsX.lo - penX), r(statsX.hi - penX),
                    r(statsY.lo - baselineY), r(statsY.hi - baselineY),
                    r(statsX.mass / 255),
                    clipped ? "clipped" : "ok",
                  ].join(";"));
                }
              }

              return [
                ["#font", family, em, r6(ascent), r6(descent), r6(canvasAscent), r6(canvasDescent), r6(dom.advance), cellW, cellH].join(";"),
                ...lines,
              ].join("\n");

              function r6(v) { return v.toFixed(6); }
            }
            """;

        /// <summary>Renders the preview rows (raw vs adjusted vs overlay) used for eyeballing the result.</summary>
        public const string BuildPreviewScript = """
            (request) => {
              document.getElementById("tss-adjustments").textContent = request.css;
              const host = document.getElementById("preview");
              host.textContent = "";

              const cell = (classNames, extraClass) => {
                const div = document.createElement("div");
                div.className = "cell" + (extraClass ? " " + extraClass : "");
                for (const className of classNames) {
                  const icon = document.createElement("i");
                  icon.className = className;
                  div.appendChild(icon);
                }
                return div;
              };

              for (const section of request.sections) {
                const caption = document.createElement("div");
                caption.className = "caption";
                caption.textContent = section.title;
                host.appendChild(caption);

                for (const item of section.rows) {
                  const row = document.createElement("div");
                  row.className = "row";

                  const name = document.createElement("div");
                  name.className = "name";
                  name.textContent = item.label;
                  row.appendChild(name);

                  const delta = document.createElement("div");
                  delta.className = "delta";
                  delta.textContent = item.detail;
                  row.appendChild(delta);

                  for (const spec of item.cells) {
                    const classes = (spec.raw ? "raw" : "") + (spec.classNames.length > 1 ? " overlay" : "");
                    row.appendChild(cell(spec.classNames, classes.trim()));
                  }

                  host.appendChild(row);
                }
              }

              return host.getBoundingClientRect().height;
            }
            """;

        /// <summary>Reads back the applied offsets from the browser, to prove the generated selectors match.</summary>
        public const string ReadAppliedOffsetsScript = """
            (request) => {
              document.getElementById("tss-adjustments").textContent = request.css;
              const probe = document.createElement("i");
              probe.style.fontSize = "100px";
              document.body.appendChild(probe);
              const result = [];
              for (const className of request.classNames) {
                probe.className = className;
                const style = getComputedStyle(probe, "::before");
                result.push([className, style.position, style.left, style.top].join(";"));
              }
              probe.remove();
              return result.join("\n");
            }
            """;
    }
}
