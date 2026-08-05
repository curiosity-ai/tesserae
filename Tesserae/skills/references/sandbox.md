---
name: sandbox
description: A locked-down iframe for rendering untrusted HTML or external URLs in an isolated context, with a CSP, post-message channel, and error reporting. Use when embedding untrusted content or a third-party app in a Tesserae (C#/Transpose) app.
---

# Sandbox

Wraps an `<iframe>`. By default content is loaded via `srcdoc` with `sandbox="allow-scripts allow-forms"` (no `allow-same-origin`, so an opaque origin with no access to the host), plus a strict injected CSP and a bootstrap script that reports errors and relays messages. CSP/bootstrap apply only to inline HTML, not cross-origin `src` URLs.

## Create

- `UI.Sandbox(string html = null)` — inline HTML content.
- `UI.SandboxUrl(string url)` — load an external URL.
Both return a `Sandbox`. Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.FromHtml(string)` / `.FromUrl(string)` — set content after construction.
- `.OnError(Action<SandboxError>)` — uncaught errors, promise rejections, and CSP violations; `SandboxError` has `Kind`, `Message`, `Source`, `Line`, `Column`, `Stack`, `IsContentSecurityPolicyViolation`.
- `.OnMessage(Action<object>)` / `.PostMessage(object)` — host ↔ sandbox messaging.
- `.OnLoaded(Action<HTMLIFrameElement>)` — fires each load.
- `.FitHeightToContent(bool = true)` — size the frame to its content height (grows and shrinks). Content is watched with a `ResizeObserver` + `MutationObserver`. For a fully-sandboxed frame this is reported from inside via the bootstrap (needs `allow-scripts`); when `.AllowSameOrigin()` is set the height is measured host-side instead, so it also works with scripts disabled. Set before render.
- Sandbox flags: `.AllowScripts()`, `.AllowForms()`, `.AllowPopups()`, `.AllowModals()`, `.AllowDownloads()`, `.AllowToken(string)`, `.SandboxAttribute(string)`.
- `.ContentSecurityPolicy(string)` / `.NoContentSecurityPolicy()` — override or disable the CSP.
- `.AllowSameOrigin()` — **weakens isolation**; only for trusted content.

## What the default CSP does and does not stop

`Sandbox.DefaultContentSecurityPolicy` is
`default-src 'none'; script-src 'unsafe-inline'; style-src 'unsafe-inline'; img-src data: blob:; form-action 'none'; base-uri 'none';`

It stops every way the content can *fetch* something: remote images, stylesheets, fonts,
`fetch`/`XHR`/`sendBeacon`, WebSockets, nested frames, `prefetch`, and CSS `url()` — including
`@import` and `@font-face`. `form-action` and `base-uri` are spelled out because neither falls back
to `default-src`; without `form-action 'none'` a framed document with `allow-forms` can submit a
form to any host on load and exfiltrate that way.

It does **not** stop the document navigating *itself* — `location = …`, a `meta refresh`, or a click
on a link — because no fetch directive covers that. Sandbox flags are what govern those: without
`allow-scripts` the automatic ones (`location`, `meta refresh`, a scripted `.click()`) are blocked
too, but a **reader's click on a link inside the frame still navigates it**. To close that, strip the
links out of the content before framing it — e.g. sanitize with
`MarkdownSanitization.NoLinksOrEmbeddedContent`'s DOMPurify configuration (see
`markdown-block.md`). `allow-popups` and top-level navigation are already blocked by the default
flags.

## Example

```csharp
using static Tesserae.UI;

var sandbox = Sandbox("<h1>Hello</h1><script>console.log('hi')</script>")
    .FitHeightToContent()
    .OnError(err => console.log(err.ToString()));
```

## Related

- Full docs & API: `/tesserae/components/sandbox`
