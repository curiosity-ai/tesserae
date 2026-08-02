using System.Collections.Generic;

namespace Build.UpdateInterfaceIcons
{
    /// <summary>
    /// The python implementation the outline surgery in <see cref="TransformedGlyf"/> replaced, kept as a
    /// second opinion. It does the same job through fontTools instead of editing the woff2 streams, so it is
    /// worth reaching for when a change to the font surgery needs proving, or when a patched font misbehaves
    /// and the question is whether the container writer or the measurement is at fault.
    /// <para>
    /// It is a comment rather than a file because nothing in the build runs it any more: the toolkit needs no
    /// python, and a script sitting in the project would suggest otherwise. Copy the two blocks below out to
    /// use them.
    /// </para>
    /// <para><b>Producing a reference set to compare against.</b> The bake stage no longer writes the offsets
    /// json the script reads, so dump it from <c>BakeIntoFontOutlines</c> - the shape is
    /// <c>{ fontFamily: { iconClass: [leftEm, topEm] } }</c>, exactly the <c>X</c> and <c>Y</c> of every glyph
    /// with <c>IsAdjusted</c> set, in css sign convention. Then, against a tree holding the vendor fonts:
    /// </para>
    /// <code>
    /// pip install fonttools brotli
    /// python3 centre-uicons-outlines.py --offsets offsets.json \
    ///         --fonts Tesserae/tps/assets/fonts --css Tesserae/tps/assets/css
    /// python3 compare_fonts.py reference-fonts Tesserae/tps/assets/fonts
    /// </code>
    /// <para>
    /// What agreement looks like, and what the two implementations differ on by design: every glyph outline,
    /// every declared bounding box and all of <c>hmtx</c> must be identical. <c>head</c> and <c>cmap</c> will
    /// not be - fontTools stamps the save time into <c>head.modified</c>, recomputes
    /// <c>head.checkSumAdjustment</c> (which the woff2 decoder overwrites when it rebuilds the font anyway),
    /// and re-encodes the <c>cmap</c> subtables more compactly. The C# writer copies all three from the
    /// vendor untouched, which is why its output is reproducible to the byte and fontTools' is not.
    /// </para>
    /// </summary>
    internal static class PythonReferenceImplementation
    {
        /// <summary>Names the blocks below, so a search for either lands here.</summary>
        public static readonly IReadOnlyList<string> Scripts = new[] { "centre-uicons-outlines.py", "compare_fonts.py" };
    }
}

/*  ============================================================================================
    centre-uicons-outlines.py - the fontTools implementation of the outline shift
    ============================================================================================

    #!/usr/bin/env python3
    """Bake optical centering offsets into the outlines of the bundled UIcons webfonts.

    Reads the offsets measured by Build.UpdateInterfaceIcons (a json file mapping icon class name to
    an em offset) and shifts the corresponding glyph outlines by that amount, in place, in the woff2
    files. The icons then arrive already centred, at any size, with no stylesheet and no pixel snapping.

    Run through the build project rather than by hand:

        dotnet run --project Build.UpdateInterfaceIcons

    Two things about these fonts make naive font surgery go wrong, and both are load bearing here:

    *   Their declared glyph bounding boxes and left side bearings disagree with their own outlines -
        a glyph whose ink starts at x=75 is declared with xMin=0 and lsb=0 - and the rasterizer places
        the glyph from the declared box. Recomputing those boxes, which fontTools does by default on
        save, moves every glyph by tens of units. So the font is opened with recalcBBoxes=False and
        hmtx is left completely alone; only the outline coordinates move.
    *   The em square is 300 units, so an offset is rounded to a whole unit. That is 1/300 em, which
        shifts the intended value by at most 0.0017 em, well under a tenth of a pixel at any icon size.

    Requires: fonttools, brotli  (pip install fonttools brotli)
    """
    from __future__ import annotations

    import argparse
    import json
    import re
    import sys
    from pathlib import Path

    try:
        from fontTools.ttLib import TTFont
    except ImportError:  # pragma: no cover - the message is the point
        sys.exit("centre-uicons-outlines.py needs fontTools and brotli: pip install fonttools brotli")


    def read_codepoints(css_path: Path) -> dict[str, int]:
        """Icon class name -> codepoint, from the vendor stylesheet that ships with the font."""
        lines = css_path.read_text(encoding="utf-8", errors="replace").split("\n")
        codepoints: dict[str, int] = {}

        for index, line in enumerate(lines[:-1]):
            selector = line.strip()

            if not (selector.startswith(".fi-") and selector.endswith(":before {")):
                continue

            for following in lines[index + 1 : index + 4]:
                match = re.search(r'content:\s*"\\([0-9a-fA-F]+)"', following)
                if match:
                    for name in re.findall(r"\.([\w-]+):before", selector):
                        codepoints[name] = int(match.group(1), 16)
                    break

        return codepoints


    def centre(font_path: Path, css_path: Path, offsets: dict[str, tuple[float, float]]) -> dict:
        """Shift the outlines of one font. Returns what was done, and raises if it did not take."""
        font = TTFont(font_path, recalcBBoxes=False)
        upem = font["head"].unitsPerEm
        codepoints = read_codepoints(css_path)
        cmap = font.getBestCmap()
        glyf = font["glyf"]

        intended: dict[str, tuple[int, int]] = {}
        before: dict[str, list[tuple[int, int]]] = {}
        worst_rounding = 0.0

        for class_name, (left_em, top_em) in offsets.items():
            codepoint = codepoints.get(class_name)

            if codepoint is None or codepoint not in cmap:
                continue

            # css x grows right and y grows down; font units grow right and up
            dx = round(left_em * upem)
            dy = round(-top_em * upem)
            worst_rounding = max(worst_rounding, abs(dx - left_em * upem), abs(dy + top_em * upem))

            if dx == 0 and dy == 0:
                continue

            glyph_name = cmap[codepoint]

            if glyph_name in intended:
                raise SystemExit(f"{font_path.name}: {glyph_name} is claimed by two icon classes, refusing to shift it twice")

            glyph = glyf[glyph_name]
            glyph.expand(glyf)

            if getattr(glyph, "numberOfContours", 0) == 0:
                continue

            before[glyph_name] = [tuple(point) for point in glyph.coordinates]

            for index, (x, y) in enumerate(glyph.coordinates):
                glyph.coordinates[index] = (x + dx, y + dy)

            intended[glyph_name] = (dx, dy)

        font.flavor = "woff2"
        font.save(font_path)

        # Read the file back and prove every glyph moved by exactly the intended amount, uniformly across
        # all of its points. A silent partial edit here would be very hard to spot downstream.
        written = TTFont(font_path, recalcBBoxes=False)
        written_glyf = written["glyf"]

        for glyph_name, (dx, dy) in intended.items():
            glyph = written_glyf[glyph_name]
            glyph.expand(written_glyf)
            was = before[glyph_name]
            now = [tuple(point) for point in glyph.coordinates]

            if len(was) != len(now):
                raise SystemExit(f"{font_path.name}: {glyph_name} has {len(now)} points, had {len(was)}")

            deltas = {(nx - ox, ny - oy) for (ox, oy), (nx, ny) in zip(was, now)}

            if deltas != {(dx, dy)}:
                raise SystemExit(f"{font_path.name}: {glyph_name} moved by {sorted(deltas)[:3]}, expected exactly {(dx, dy)}")

        return {
            "font": font_path.name,
            "unitsPerEm": upem,
            "glyphsMoved": len(intended),
            "worstRoundingEm": round(worst_rounding / upem, 5),
        }


    def main() -> int:
        parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
        parser.add_argument("--offsets", required=True, type=Path,
                            help="json: { fontFamily: { iconClass: [leftEm, topEm] } }")
        parser.add_argument("--fonts", required=True, type=Path, help="directory holding the woff2 files")
        parser.add_argument("--css", required=True, type=Path, help="directory holding the vendor uicons-*.css files")
        args = parser.parse_args()

        families = json.loads(args.offsets.read_text(encoding="utf-8"))
        report = []

        for family, offsets in sorted(families.items()):
            font_path = args.fonts / f"{family}.woff2"
            css_path = args.css / f"{family}.css"

            if not font_path.exists():
                raise SystemExit(f"no such font: {font_path}")
            if not css_path.exists():
                raise SystemExit(f"no such stylesheet: {css_path}")

            result = centre(font_path, css_path, {k: tuple(v) for k, v in offsets.items()})
            report.append(result)
            print(f"  {result['font']:34} {result['glyphsMoved']:5} glyphs moved, "
                  f"worst rounding {result['worstRoundingEm']}em", flush=True)

        print(json.dumps({"fonts": report}))
        return 0


    if __name__ == "__main__":
        sys.exit(main())

    ============================================================================================
    compare_fonts.py - glyph by glyph comparison of two directories of woff2 files
    ============================================================================================

    """Compare two woff2 fonts glyph by glyph: outlines, declared bboxes, and the tables that matter."""
    import sys, pathlib
    from fontTools.ttLib import TTFont

    def load(p):
        return TTFont(p, recalcBBoxes=False)

    def compare(a_path, b_path, label_a, label_b):
        a, b = load(a_path), load(b_path)
        problems = []
        if a.getGlyphOrder() != b.getGlyphOrder():
            problems.append("glyph order differs")
            return problems, 0, 0
        ga, gb = a['glyf'], b['glyf']
        moved = identical = 0
        for name in a.getGlyphOrder():
            A, B = ga[name], gb[name]
            A.expand(ga); B.expand(gb)
            ca = [tuple(p) for p in getattr(A, 'coordinates', [])]
            cb = [tuple(p) for p in getattr(B, 'coordinates', [])]
            if ca != cb:
                problems.append(f"{name}: coordinates differ ({ca[:2]} vs {cb[:2]})")
            elif ca:
                identical += 1
            # the declared box is what places the glyph, so it must be untouched
            for attr in ('xMin','yMin','xMax','yMax'):
                if getattr(A, attr, None) != getattr(B, attr, None):
                    problems.append(f"{name}: declared {attr} {getattr(A,attr,None)} vs {getattr(B,attr,None)}")
            if a['hmtx'][name] != b['hmtx'][name]:
                problems.append(f"{name}: hmtx {a['hmtx'][name]} vs {b['hmtx'][name]}")
        for tag in ('head','hhea','maxp','cmap','name','post','OS/2','GSUB'):
            if tag in a and tag in b:
                if a.getTableData(tag) != b.getTableData(tag):
                    problems.append(f"table {tag} differs")
            elif (tag in a) != (tag in b):
                problems.append(f"table {tag} present in only one font")
        return problems, identical, len(a.getGlyphOrder())

    if __name__ == '__main__':
        a_dir, b_dir = pathlib.Path(sys.argv[1]), pathlib.Path(sys.argv[2])
        total_problems = 0
        print(f"{'font':30} {'glyphs':>7} {'identical outlines':>19}  verdict")
        for f in sorted(a_dir.glob('*.woff2')):
            problems, identical, n = compare(str(f), str(b_dir / f.name), sys.argv[1], sys.argv[2])
            total_problems += len(problems)
            print(f"  {f.name:28} {n:>7} {identical:>19}  {'identical' if not problems else str(len(problems))+' DIFFERENCES'}")
            for p in problems[:4]: print(f"      {p}")
        print(f"\ntotal differences: {total_problems}")

    ============================================================================================ */
