---
name: project-setup
description: How to create, configure, build, and run a Tesserae project compiled from C# to JavaScript via Transpose. Use when setting up a new Tesserae (C#/Transpose) app, configuring tps.json, or running it locally.
---

# Project Setup

Tesserae is a C# UI toolkit compiled to JavaScript by the **Transpose** compiler. A
minimal app is an `App.cs` with a `Main` entry point that builds components and
mounts them to the DOM.

## Install

```bash
dotnet add package Tesserae
```

This pulls in the required `Transpose` dependencies.

## First app

```csharp
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae.Tests
{
    internal static class App
    {
        private static void Main()
        {
            var helloWorld = TextBlock("Hello World")
                .Medium()
                .SemiBold()
                .Margin(16.px());

            MountCenteredToBody(helloWorld);   // or document.body.appendChild(c.Render())
        }
    }
}
```

## Build and run locally

```bash
dotnet build
cd bin/Debug/netstandard2.0/tps/
dotnet serve --port 5000      # install: dotnet tool update --global dotnet-serve
```

Then open `http://localhost:5000/`.

## tps.json configuration

Reflection must stay enabled — the `Router` and automatic sample loading depend
on it:

```json
{
  "reflection": { "disabled": false, "target": "inline" }
}
```

Bundle external assets (JS/CSS/fonts/images) via the `resources` section so they
are copied into the generated `index.html`:

```json
{
  "resources": [
    { "name": "images", "files": [ "tps/assets/img/*" ], "output": "assets/img" },
    { "name": "custom-styles.css", "files": [ "tps/assets/css/site.css" ] }
  ]
}
```

Place assets under an `assets/` folder in the output directory (e.g. `tps/assets/`).

## Related

- Core Concepts — `.core-concepts.md`
- Routing — `.routing.md`
- Full docs: `/tesserae/get-started/project-setup`
