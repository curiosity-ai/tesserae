---
name: sandbox
description: A locked-down iframe for rendering untrusted HTML or external URLs in an isolated context, with a CSP, post-message channel, and error reporting. Use when embedding untrusted content or a third-party app in a Tesserae (C#/h5) app.
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
- `.FitHeightToContent(bool = true)` — grow the frame to content height.
- Sandbox flags: `.AllowScripts()`, `.AllowForms()`, `.AllowPopups()`, `.AllowModals()`, `.AllowDownloads()`, `.AllowToken(string)`, `.SandboxAttribute(string)`.
- `.ContentSecurityPolicy(string)` / `.NoContentSecurityPolicy()` — override or disable the CSP.
- `.AllowSameOrigin()` — **weakens isolation**; only for trusted content.

## Example

```csharp
using static Tesserae.UI;

var sandbox = Sandbox("<h1>Hello</h1><script>console.log('hi')</script>")
    .FitHeightToContent()
    .OnError(err => console.log(err.ToString()));
```

## Related

- Full docs & API: `/tesserae/components/sandbox`
