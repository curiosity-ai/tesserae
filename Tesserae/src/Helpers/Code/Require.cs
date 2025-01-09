using System;
using System.Threading.Tasks;
using static H5.Core.dom;

namespace Tesserae
{
    [H5.Name("tss.Require")]
    public static class Require
    {
        private static readonly SingleSemaphoreSlim singleCall = new SingleSemaphoreSlim();
        public static void LoadStyleAsync(params string[] styles)
        {
            for (int i = 0; i < styles.Length; i++)
            {
                var         url           = styles[i];
                HTMLElement existingStyle = (HTMLElement)document.querySelector($"link[href^='{url}']");

                if (existingStyle == null)
                {
                    var l = document.createElement("link") as HTMLLinkElement;
                    l.rel  = "stylesheet";
                    l.href = url;
                    document.head.appendChild(l);
                }
            }
        }

        public static async Task LoadScriptAsync(params string[] libraries)
        {
            await singleCall.WaitAsync();

            try
            {
                var tcs = new TaskCompletionSource<bool>();
                LoadScriptAsync(() => tcs.SetResult(true), (err) => tcs.SetException(new Exception(err)), false, libraries);
                await tcs.Task;
            }
            finally
            {
                singleCall.Release();
            }
        }

        public static async Task LoadModuleAsync(params string[] libraries)
        {
            await singleCall.WaitAsync();

            try
            {
                var tcs = new TaskCompletionSource<bool>();
                LoadScriptAsync(() => tcs.SetResult(true), (err) => tcs.SetException(new Exception(err)), true, libraries);
                await tcs.Task;
            }
            finally
            {
                singleCall.Release();
            }
        }

        private static void LoadScriptAsync(Action onComplete, Action<string> onFail, bool isModule, params string[] libraries)
        {
            var loadedCount = 0;

            for (int i = 0; i < libraries.Length; i++)
            {
                var         url         = libraries[i];
                HTMLElement existingLib = (HTMLElement)document.querySelector($"script[src^='{url}']");

                if (existingLib != null)
                {
                    // Is already loaded?
                    loadedCount++;
                }
                else
                {
                    var script = new HTMLScriptElement
                    {
                        type  = isModule ? "module" : "text/javascript",
                        src   = url,
                        async = true,
                        onerror = e =>
                        {
                            onFail?.Invoke(url);
                            loadedCount++;

                            if (loadedCount == libraries.Length)
                            {
                                onComplete?.Invoke();
                            }
                        },
                        onload = OnScriptLoaded
                    };

                    try
                    {
                        document.head.appendChild(script);
                    }
                    catch
                    {
                        onFail?.Invoke(url);
                        loadedCount++;
                    }
                }
            }

            if (loadedCount == libraries.Length)
            {
                onComplete?.Invoke();
            }

            void OnScriptLoaded(Event e)
            {
                loadedCount++;
                if (loadedCount == libraries.Length) onComplete?.Invoke();
            }
        }
    }
}