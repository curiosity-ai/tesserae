
[![Build Status](https://dev.azure.com/curiosity-ai/mosaik/_apis/build/status/h5.tesserae?branchName=master)](https://dev.azure.com/curiosity-ai/mosaik/_build/latest?definitionId=42&branchName=master)

<a href="https://curiosity.ai"><img src="https://curiosity.ai/assets/images/logos/curiosity.png" width="100" height="100" align="right" /></a>
# tesserae

_**Tesserae**_ is a UI toolkit for building websites entirely in C#, inspired by Microsoft's [Fluent UI](https://github.com/microsoft/fluentui) toolkit.

It uses the [h5](https://github.com/theolivenbaum/h5) C# to Javascript transpiler to provide an easy to use, strongly typed UI development experience.

### Usage

#### Using NuGet:

[![Nuget](https://img.shields.io/nuget/v/Tesserae.svg?maxAge=0&colorB=brightgreen)](https://www.nuget.org/packages/Tesserae/) 

```
install-package Tesserae
```

#### Local development

For development, we recomend installing the [dotnet serve](https://github.com/natemcmaster/dotnet-serve) global tool, so you can test your site locally:

````bash
cd \bin\Debug\netstandard2.0\h5\
dotnet serve --port 5000
start http://localhost:5000/
````

### Samples

The Tesserae.Tests project contains multiple samples of how to use this library. It is also built automatically and hosted [on our website](http://tesserae.curiosity.ai/).
