using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Tesserae.Benchmark.Measure;

internal sealed class StaticFileServer : IAsyncDisposable
{
    private readonly WebApplication _app;

    public string BaseUrl { get; }

    private StaticFileServer(WebApplication app, string baseUrl)
    {
        _app    = app;
        BaseUrl = baseUrl;
    }

    public static async Task<StaticFileServer> StartAsync(string rootDirectory, int port)
    {
        if (!Directory.Exists(rootDirectory))
            throw new DirectoryNotFoundException($"Harness directory not found: {rootDirectory}");

        var builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.UseKestrel(options =>
        {
            options.Listen(IPAddress.Loopback, port);
        });

        var app = builder.Build();

        var provider = new FileExtensionContentTypeProvider();
        provider.Mappings[".js"]    = "application/javascript; charset=utf-8";
        provider.Mappings[".css"]   = "text/css; charset=utf-8";
        provider.Mappings[".map"]   = "application/json";
        provider.Mappings[".woff2"] = "font/woff2";

        var fileProvider = new PhysicalFileProvider(rootDirectory);

        app.UseDefaultFiles(new DefaultFilesOptions
        {
            FileProvider = fileProvider,
            DefaultFileNames = new List<string> { "index.html" }
        });

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider     = fileProvider,
            ContentTypeProvider = provider,
            ServeUnknownFileTypes = true,
            DefaultContentType    = "application/octet-stream",
            OnPrepareResponse = ctx =>
            {
                // Disable caching so consecutive runs always see fresh assets.
                ctx.Context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
                ctx.Context.Response.Headers["Pragma"]        = "no-cache";
            }
        });

        await app.StartAsync();

        var actualPort = port;
        if (actualPort == 0)
        {
            // Kestrel chose a free port — read it back from the server addresses feature.
            var server = app.Services.GetRequiredService<Microsoft.AspNetCore.Hosting.Server.IServer>();
            var feature = server.Features.Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>();
            var addr = feature?.Addresses.FirstOrDefault();
            if (addr != null)
            {
                var uri = new Uri(addr);
                actualPort = uri.Port;
            }
        }

        return new StaticFileServer(app, $"http://localhost:{actualPort}");
    }

    public async ValueTask DisposeAsync()
    {
        await _app.StopAsync();
        await _app.DisposeAsync();
    }
}
