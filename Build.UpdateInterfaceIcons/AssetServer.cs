using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Build.UpdateInterfaceIcons
{
    /// <summary>
    /// Serves <c>Tesserae/tps/assets</c> over http so the measurement page loads the very same
    /// stylesheets and woff2 files the toolkit ships. A <c>file://</c> page cannot be used here:
    /// Chromium refuses to load webfonts across opaque file origins.
    /// </summary>
    internal sealed class AssetServer : IDisposable
    {
        private static readonly Dictionary<string, string> ContentTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { ".html", "text/html; charset=utf-8" },
            { ".css", "text/css; charset=utf-8" },
            { ".js", "text/javascript; charset=utf-8" },
            { ".woff2", "font/woff2" },
            { ".woff", "font/woff" },
            { ".ttf", "font/ttf" },
            { ".svg", "image/svg+xml" },
            { ".png", "image/png" },
        };

        private readonly string                  _rootDirectory;
        private readonly HttpListener            _listener;
        private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();
        private readonly Dictionary<string, string> _virtualPages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public AssetServer(string rootDirectory)
        {
            _rootDirectory = Path.GetFullPath(rootDirectory);
            (_listener, Port) = StartListener();
            _ = Task.Run(AcceptLoop);
        }

        public int Port { get; }

        public string BaseUrl => $"http://127.0.0.1:{Port}/";

        /// <summary>Registers an in-memory html page, served from the asset root so relative urls resolve.</summary>
        public void AddPage(string path, string html) => _virtualPages[path] = html;

        private static (HttpListener, int) StartListener()
        {
            for (int port = 8321; port < 8421; port++)
            {
                var listener = new HttpListener();
                listener.Prefixes.Add($"http://127.0.0.1:{port}/");

                try
                {
                    listener.Start();
                    return (listener, port);
                }
                catch (HttpListenerException)
                {
                    listener.Close();
                }
            }

            throw new InvalidOperationException("Could not find a free port to serve the assets from.");
        }

        private async Task AcceptLoop()
        {
            while (!_cancellation.IsCancellationRequested)
            {
                HttpListenerContext context;

                try
                {
                    context = await _listener.GetContextAsync();
                }
                catch (Exception) when (_cancellation.IsCancellationRequested || !_listener.IsListening)
                {
                    return;
                }

                try
                {
                    Serve(context);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  warning: failed to serve {context.Request.Url}: {ex.Message}");
                }
                finally
                {
                    context.Response.Close();
                }
            }
        }

        private void Serve(HttpListenerContext context)
        {
            var path = Uri.UnescapeDataString(context.Request.Url?.AbsolutePath ?? "/").TrimStart('/');

            if (_virtualPages.TryGetValue(path, out var html))
            {
                WriteBytes(context, Encoding.UTF8.GetBytes(html), ContentTypes[".html"]);
                return;
            }

            var file = Path.GetFullPath(Path.Combine(_rootDirectory, path));

            if (!file.StartsWith(_rootDirectory, StringComparison.Ordinal) || !File.Exists(file))
            {
                context.Response.StatusCode = 404;
                return;
            }

            var extension  = Path.GetExtension(file);
            var contentType = ContentTypes.TryGetValue(extension, out var known) ? known : "application/octet-stream";

            WriteBytes(context, File.ReadAllBytes(file), contentType);
        }

        private static void WriteBytes(HttpListenerContext context, byte[] bytes, string contentType)
        {
            context.Response.ContentType     = contentType;
            context.Response.ContentLength64 = bytes.Length;
            context.Response.OutputStream.Write(bytes, 0, bytes.Length);
        }

        public void Dispose()
        {
            _cancellation.Cancel();

            try
            {
                _listener.Stop();
                _listener.Close();
            }
            catch (ObjectDisposedException)
            {
                // already closed
            }

            _cancellation.Dispose();
        }
    }
}
