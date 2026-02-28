
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using H5.Compiler;
using H5.Compiler.Hosted;
using H5.Translator;
using NuGet.Versioning;

namespace Tesserae.Playground.Host
{
    public class CompilerService
    {
        private readonly PackageDownloader _downloader;
        private readonly string _packagesDir;
        private readonly string _sdksDir;

        public CompilerService()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            _packagesDir = Path.Combine(baseDir, "packages");
            _sdksDir = Path.Combine(baseDir, "sdks");
            _downloader = new PackageDownloader(_packagesDir);
        }

        public async Task PreparePackagesAsync()
        {
             await _downloader.EnsurePackageAsync("h5", "25.11.62743");
             await _downloader.EnsurePackageAsync("h5.core", "25.11.62746");
             await _downloader.EnsurePackageAsync("h5.Target", "25.11.62725");

             if (!Directory.Exists(_sdksDir)) Directory.CreateDirectory(_sdksDir);

             Environment.SetEnvironmentVariable("MSBuildSDKsPath", _sdksDir);
             Environment.SetEnvironmentVariable("MSBuildEnableWorkloadResolver", "false"); // Disable workloads

             // 1. Copy/Link h5.Target
             var packageSdkDir = Path.Combine(_packagesDir, "h5.target", "25.11.62725", "Sdk");
             if (Directory.Exists(packageSdkDir))
             {
                 var targetSdkDirVersioned = Path.Combine(_sdksDir, "h5.Target", "25.11.62725", "Sdk");
                 if (!Directory.Exists(targetSdkDirVersioned))
                 {
                     Directory.CreateDirectory(Path.GetDirectoryName(targetSdkDirVersioned)!);
                     CopyDirectory(packageSdkDir, targetSdkDirVersioned);
                 }

                 var targetSdkDirVersionless = Path.Combine(_sdksDir, "h5.Target", "Sdk");
                 if (!Directory.Exists(targetSdkDirVersionless))
                 {
                     Directory.CreateDirectory(Path.GetDirectoryName(targetSdkDirVersionless)!);
                     CopyDirectory(packageSdkDir, targetSdkDirVersionless);
                 }
             }

             // 2. Link System SDKs
             var dotnetRoot = "/usr/lib/dotnet/sdk";
             if (Directory.Exists(dotnetRoot))
             {
                 var versions = Directory.GetDirectories(dotnetRoot);
                 var latest = versions.OrderByDescending(v => v).FirstOrDefault();
                 if (latest != null)
                 {
                     Environment.SetEnvironmentVariable("MSBuildExtensionsPath", latest);

                     var systemSdks = Path.Combine(latest, "Sdks");
                     if (Directory.Exists(systemSdks))
                     {
                         foreach (var sdk in Directory.GetDirectories(systemSdks))
                         {
                             var sdkName = Path.GetFileName(sdk);
                             var dest = Path.Combine(_sdksDir, sdkName);
                             if (!Directory.Exists(dest))
                             {
                                 try
                                 {
                                     Directory.CreateSymbolicLink(dest, sdk);
                                 }
                                 catch
                                 {
                                     CopyDirectory(sdk, dest);
                                 }
                             }
                         }
                     }
                 }
             }
        }

        private void CopyDirectory(string sourceDir, string destinationDir)
        {
            Directory.CreateDirectory(destinationDir);
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                File.Copy(file, Path.Combine(destinationDir, Path.GetFileName(file)), true);
            }
            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                CopyDirectory(dir, Path.Combine(destinationDir, Path.GetFileName(dir)));
            }
        }

        public class CompilationResult
        {
            public bool Success { get; set; }
            public string? JavaScript { get; set; }
            public string? IndexHtml { get; set; }
            public List<string> Errors { get; set; } = new List<string>();
        }

        public async Task<CompilationResult> CompileAsync(string source)
        {
            await PreparePackagesAsync();

            var result = new CompilationResult();

            try
            {
                var settings = Activator.CreateInstance(typeof(H5DotJson_AssemblySettings)) as H5DotJson_AssemblySettings;
                if (settings == null) throw new Exception("Could not instantiate H5DotJson_AssemblySettings");

                var referencesPathProp = settings.GetType().GetProperty("ReferencesPath");
                if (referencesPathProp != null)
                {
                    referencesPathProp.SetValue(settings, _packagesDir);
                }

                settings.Output = "h5";

                var request = new CompilationRequest("Playground", settings);

                request.WithSourceFile("Program.cs", source);

                request.WithPackageReference("h5", NuGetVersion.Parse("25.11.62743"));
                request.WithPackageReference("h5.core", NuGetVersion.Parse("25.11.62746"));
                request.WithPackageReference("h5.Target", NuGetVersion.Parse("25.11.62725"));

                var output = await CompilationProcessor.CompileAsync(request, CancellationToken.None);

                if (output != null && output.Output != null)
                {
                    foreach(var kvp in output.Output)
                    {
                        var fileName = kvp.Key;
                        var content = kvp.Value;

                        if (fileName.EndsWith(".js") && !fileName.EndsWith(".min.js") && !fileName.EndsWith(".meta.js"))
                        {
                            result.JavaScript = content;
                        }
                        else if (fileName.EndsWith(".html"))
                        {
                            result.IndexHtml = content;
                        }
                    }

                    result.Success = true;

                    if (string.IsNullOrEmpty(result.IndexHtml))
                    {
                         result.IndexHtml = @"<!DOCTYPE html><html><head><meta charset=""utf-8"" /><script src=""Playground.js""></script></head><body></body></html>";
                    }
                }
                else
                {
                    result.Success = false;
                    result.Errors.Add("No output generated");
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
                result.Errors.Add(ex.StackTrace);
                result.Success = false;
            }

            return result;
        }
    }
}
