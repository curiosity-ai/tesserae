---
name: tntc-translate
description: >-
  Translate the untranslated UI strings of a project that manages its
  translations with TNTC (a `.tnt` folder next to the sources). Use whenever
  asked to "translate the missing strings", "update the translations", "add a
  language", "fix the translations", or after new `.t()` strings were added to
  the code. Drives the tntc loop - extract, missing, translate in-context,
  apply, verify - so every translation is validated (placeholders, HTML, URLs,
  whitespace) before it is written, and records each one as
  ClaudeSkillGenerated with the model that produced it.
---

# Translating with TNTC

TNTC extracts translatable strings from C# code (`"...".t()` / `t($"...")`),
tracks their translations per language in `.tnt/translation-{code}.json`, and
ships the flat pairs the application loads from `.tnt-content/{code}.tnt`.
The tool never calls an LLM: **you are the translator.** It hands you the
pending strings as a batch file, you fill the translations in, and it
validates and merges them back.

A "project" is any folder with a `.tnt` folder in it.
`.tnt/extra-sources.json` lists further source folders to scan.

## The loop

```bash
# 0. once per machine
dotnet tool install --global TNTC
export DOTNET_ROLL_FORWARD=LatestMajor   # when only a newer runtime is installed
# (this skill itself is installed/updated by `tntc install-skill <repoRoot>` -
#  if a command prints "Updated the tntc-translate skill ... Commit the diff",
#  include that diff in your commit)

# 1. scan the sources, refresh locations, queue what has no translation
tntc extract <projectFolder> [--languages de,fr]

# 2. get a batch of pending strings (repeat with --limit for a big backlog)
tntc missing <projectFolder> --limit 50 --output batch.json [--languages de,fr]

# 3. fill in every empty "translations" value in batch.json  <- your job

# 4. merge back; every translation is validated, rejections exit 2
tntc apply <projectFolder> batch.json --model <your-model-id>

# 5. prove the whole store is clean
tntc verify <projectFolder>
```

Repeat 2-4 until `missing` reports `0 of 0 pending`. Run `verify` once at the
end. All commands default to all 20 languages
(`cs, de, el, es, fr, he, hi, it, ja, ko, ms, ne, nl, pl, pt, ru, sr, sv, uk, zh`);
`--languages` narrows them. Unless the user asked for specific languages,
translate the languages the project already has translation files for.

`--model` must be the model id you are actually running as (e.g.
`claude-opus-5`) - it is recorded on every record as `GeneratedBy`, next to
the `ClaudeSkillGenerated` state, so a reviewer can tell what produced which
translation.

## Filling in the batch

Each item looks like:

```json
{
  "originalString": "Delete {0} items",
  "sourceLocations": [ "MyApp/Views/ListView.cs:212" ],
  "translations": { "de": "", "fr": "" }
}
```

Rules of the file itself:

- **Never edit `originalString`** - it is the key `apply` merges on. An edited
  key is rejected as "not a known string".
- Fill every empty `translations` value. Leaving one empty skips it (it stays
  pending); do that only when you genuinely cannot translate it.
- A `currentTranslations` entry means you are re-translating something that
  already has text (see `--retranslate` below) - treat it as a draft to
  improve on, not as ground truth.
- **When the string is ambiguous, open the source.** `sourceLocations` is a
  real `path:line` into the repository - read the surrounding code to see
  whether "Open" is a verb on a button or an adjective in a status, what `{0}`
  will hold, whether the string is concatenated with a neighbour. This is the
  reason the translating happens here instead of in a blind API batch.

## How to translate

You are localizing application UI strings from English. They may be buttons,
menu items, labels, settings, tooltips, onboarding text, empty states, error
messages, admin/technical configuration text, or short help text.

- Translate for real product usage, not word-for-word; prefer the standard
  terminology of desktop/web apps in the target language.
- Buttons/actions sound like clickable commands; titles like section titles;
  status texts like system states; warnings clear and natural.
- Keep translations concise when the source is concise. For buttons, menu
  items and short labels prefer short established UI wording over explanatory
  wording.
- Keep the meaning, tone and intended action. Preserve whether the text is an
  instruction, label, status, warning, question or command, and the politeness
  level appropriate for software UI in the target language.
- If the English source is ungrammatical or misspelled, translate the intended
  meaning naturally.
- Technical terms ("index", "model", "pipeline", "connector", "endpoint",
  "scope", "facet", "sync", "audit", "token", ...) are translated only when a
  well-established natural equivalent exists; otherwise keep the borrowed term
  software UI in that language actually uses. Avoid literal renderings.
- Translate identical source strings identically, and keep recurring
  terminology consistent across the whole batch.
- Do not add explanations, notes, or quotation marks that are not in the
  source.

### The project glossary

Before translating, read **`.tnt/glossary.md`** in the project folder if it
exists. It lists the product's do-not-translate terms (product names, feature
names that are brands) and any product-specific terminology choices. Those
rules override the general guidance above. If the project has no glossary and
you notice product names being at risk, suggest creating one.

### What must survive verbatim

`apply` rejects a translation that breaks these, so get them right first time:

- Placeholders, exactly: `{0}`, `{1}`, `{0:n0}`, `{0:MMM dd, yyyy}`,
  `{{variable}}`, `%s`, `%d`. **Format specifiers are code, not prose** - the
  `dd`/`yyyy` in `{0:MMM dd, yyyy}` must not become `jj`/`aaaa`. Placeholders
  may move within the sentence; they may not change.
- HTML/XML: tags, attributes, entities - `<b>`, `</a>`, `<br>` stay exactly as
  written (no transliteration of tag names).
- URLs and their query parameters.
- Line breaks, and leading/trailing whitespace (strings are often
  concatenated, so a leading space is load-bearing).
- Keyboard shortcuts and key names: `Ctrl`, `Shift`, `Esc`, `+ N`.
- Code-like fragments, file paths, CSS classes, MIME types, identifiers.
- Emoji, ellipses and punctuation style, unless the target language requires a
  small natural adjustment.

## When `apply` rejects translations

`apply` prints each rejection with its rule and exits `2`; rejected
translations are not written, accepted ones in the same batch are. Fix the
rejected entries in the batch file and run `apply` again - re-applying an
already-applied batch is harmless (`ClaudeSkillGenerated` records are simply
overwritten with the same content).

Warnings (whitespace, tag attributes) also reject unless `--allow-warnings`.
Prefer fixing the translation over passing the flag; pass it only when the
difference is genuinely correct for the target language.

## States - what is safe to touch

| State | Meaning | `apply` will |
| --- | --- | --- |
| `New` + empty text | pending - queued by `extract` | fill it |
| `New` + text | legacy machine translation, unreviewed | keep it unless the batch came from `--retranslate New` |
| `LLMGenerated`, `GPT4oMiniGenerated` | earlier LLM pipeline | overwrite via `--retranslate` batches only |
| `ClaudeSkillGenerated` | this skill | overwrite freely |
| `Translated`, `Final` | human-written / reviewed | **never touch** (needs `--force`, which needs the user's explicit ask) |

`tntc missing --retranslate New,GPT4oMiniGenerated ...` queues existing
translations in those states for redoing; their current text arrives in
`currentTranslations`.

## Finishing up

- Commit the `.tnt/translation-*.json` **and** `.tnt-content/*.tnt` diffs
  together - the second is generated from the first and the application ships
  it.
- Never hand-edit `.tnt-content` - it is regenerated on every write.
- A string still pending is deliberately left out of `.tnt-content`, so the
  app falls back to English instead of showing an empty label.
- `tntc verify <projectFolder>` exits `3` on any error - run it last and fix
  what it finds. It also catches pre-existing damage (translated format
  specifiers, transliterated tags, corrupt JSON), which is worth reporting to
  the user even when it is not yours.
