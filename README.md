[![Build Status](https://dev.azure.com/curiosity-ai/mosaik/_apis/build/status/h5.tesserae?branchName=master)](https://dev.azure.com/curiosity-ai/mosaik/_build/latest?definitionId=42&branchName=master)

<a href="https://curiosity.ai"><img src="http://curiosity.ai/media/cat.color.square.svg" width="100" height="100" align="right" /></a>

# Tesserae

**Tesserae** is a UI toolkit for building web applications entirely in C#, inspired by Microsoft's [Fluent UI](https://github.com/microsoft/fluentui) toolkit. It leverages the [h5 C# to JavaScript compiler](https://github.com/theolivenbaum/h5) to provide a strongly typed, easy-to-use UI development experience.

Official Documentation: [https://docs.curiosity.ai/tesserae](https://docs.curiosity.ai/tesserae)

## Getting Started

To create a new, blank Tesserae project from scratch, follow these steps:

1.  **Update or install the h5-compiler:**
    ```bash
    dotnet tool update --global h5-compiler
    ```

2.  **Install the h5 project templates:**
    ```bash
    dotnet new install h5.Template
    ```

3.  **Create a new h5 project:**
    ```bash
    dotnet new h5
    ```

4.  **Add the Tesserae package:**
    ```bash
    dotnet add package Tesserae
    ```

## Build Process

Tesserae projects are compiled from C# to JavaScript using the **h5 compiler**.

### h5.json Configuration
The build process is controlled by the `h5.json` file located in your project root. This file defines:
-   **Output Directory**: Where the compiled files will be placed (e.g., `"output": "$(OutDir)/h5/"`).
-   **Entry Point**: The name of the generated JavaScript file (e.g., `"fileName": "app.js"`).
-   **HTML Generation**: Whether to generate an `index.html` file and its title.
-   **Resources**: Additional CSS, images, or JavaScript files to be included in the build output.

### Compilation Output
When you build the project (e.g., via `dotnet build` or in Visual Studio), the h5 compiler translates your C# code into JavaScript and copies necessary assets to the output folder. By default, these files are located in:
`bin/Debug/netstandard2.0/h5/` (or `bin/Release/...` depending on your configuration).

## Local Testing

To test your application locally, we recommend using the `dotnet-serve` tool, which is a simple command-line HTTP server.

1.  **Install dotnet-serve:**
    ```bash
    dotnet tool install dotnet-serve --global
    ```

2.  **Serve the compiled files:**
    Navigate to the h5 output directory and start the server:
    ```bash
    cd bin/Debug/netstandard2.0/h5/
    dotnet serve --port 5000
    ```

3.  **View your app:**
    Open your browser and navigate to `http://localhost:5000/`.

## Documentation

Detailed guides and documentation can be found in the following sections:

-   [Styling](./docs/STYLING.md)
-   [Custom Styles](./docs/CUSTOM_STYLES.md)
-   [Colors](./docs/COLORS.md)
-   [Iconography](./docs/ICONOGRAPHY.md)
-   [Layout & Alignment](./docs/LAYOUT_ALIGNMENT.md)
-   [Routing](./docs/ROUTING.md)

## Samples

The `Tesserae.Tests` project contains numerous examples demonstrating how to use the library components. A live version of these samples is hosted at [https://curiosity.ai/tesserae](https://curiosity.ai/tesserae).
