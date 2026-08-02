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
