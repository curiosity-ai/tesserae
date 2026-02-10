using System;
using System.Threading.Tasks;
using static H5.Core.dom;
using static Tesserae.UI;
using TNT;
using static TNT.T;

namespace Tesserae
{
    [H5.Name("tss.DCWP")]
    internal sealed class DeferedComponentWithProgress : IDefer
    {
        private readonly Func<float, string, IComponent> _loadMessageGenerator;
        private readonly Func<Action<float, string>, Task<IComponent>> _asyncGenerator;
        private          IComponent _currentLoadMessage;
        private          bool                   _needsRefresh, _waitForComponentToBeMountedBeforeFullyInitiatingRender, _renderHasBeenCalled;
        private          DebouncerWithMaxDelay  _debouncer;
        private          int _loadingMessageDelay = 1000;
        private          TaskCompletionSource<bool> _refreshCompleted;

        private int id = 0;
        private DeferedComponentWithProgress(Func<Action<float, string>, Task<IComponent>> asyncGenerator, Func<float, string, IComponent> loadMessageGenerator)
        {
            if (loadMessageGenerator is null)
                throw new ArgumentNullException(nameof(loadMessageGenerator));

            _loadMessageGenerator                                   = loadMessageGenerator;
            _asyncGenerator                                         = asyncGenerator ?? throw new ArgumentNullException(nameof(asyncGenerator));
            _needsRefresh                                           = true;
            _waitForComponentToBeMountedBeforeFullyInitiatingRender = true;
            _renderHasBeenCalled                                    = false;

            _currentLoadMessage = _loadMessageGenerator(0, "");
            Container           = DIV(_currentLoadMessage.Render());

            _debouncer = new DebouncerWithMaxDelay(() => TriggerRefresh());
        }

        internal static DeferedComponentWithProgress Create(Func<Action<float, string>, Task<IComponent>> asyncGenerator, Func<float, string, IComponent> loadMessageGenerator)
        {
            if (asyncGenerator is null)
                throw new ArgumentNullException(nameof(asyncGenerator));

            if (loadMessageGenerator is null)
            {
                loadMessageGenerator = (p, m) =>
                {
                    var msg = TextBlock(string.IsNullOrEmpty(m) ? "Loading...".t() : m, textSize: TextSize.XSmall).Class("tss-defer-loading-msg");
                    return VStack().Children(msg, ProgressIndicator().Progress(p * 100));
                };
            }

            return new DeferedComponentWithProgress(asyncGenerator, loadMessageGenerator);
        }

        public void Refresh()
        {
            _needsRefresh = true;
            _debouncer.RaiseOnValueChanged();
        }

        public Task RefreshAsync()
        {
            if (_refreshCompleted is null) _refreshCompleted = new TaskCompletionSource<bool>();

            _needsRefresh = true;
            _debouncer.RaiseOnValueChanged();
            return _refreshCompleted.Task;
        }

        public IDefer Debounce(int delayInMs, int millisecondsForLoadingMessage = 1000)
        {
            _debouncer = new DebouncerWithMaxDelay(() => TriggerRefresh(), delayInMs: delayInMs);
            _loadingMessageDelay = millisecondsForLoadingMessage;
            return this;
        }

        public IDefer Debounce(int delayInMs, int maxDelayInMs, int millisecondsForLoadingMessage = 1000)
        {
            _debouncer = new DebouncerWithMaxDelay(() => TriggerRefresh(), delayInMs: delayInMs, maxDelayInMs: maxDelayInMs);
            _loadingMessageDelay = millisecondsForLoadingMessage;
            return this;
        }

        public IDefer DoNotWaitForComponentMountingBeforeRendering()
        {
            _waitForComponentToBeMountedBeforeFullyInitiatingRender = false;
            return this;
        }

        public HTMLElement Render()
        {
            if (!_renderHasBeenCalled)
            {
                if (_waitForComponentToBeMountedBeforeFullyInitiatingRender)
                {
                    DomObserver.WhenMounted(Container, Refresh);
                }
                else
                {
                    setTimeout(_ => Refresh(), 1);
                }
                _renderHasBeenCalled = true;
            }
            return Container;
        }

        internal HTMLElement Container { get; }

        private void TriggerRefresh()
        {
            if (!_needsRefresh) return;

            _needsRefresh = false;

            id++;

            var currentID = id;

            // Reset load message state for new run
            _currentLoadMessage = _loadMessageGenerator(0, "");

            window.setTimeout(
                _ =>
                {
                    if (currentID == id)
                    {
                        if (_currentLoadMessage == null)
                        {
                            _currentLoadMessage = _loadMessageGenerator(0, "");
                        }

                        var renderedMessage = _currentLoadMessage.Render();
                        // Only replace if content is different (avoid flickering if already showing loading message)
                        if (Container.firstElementChild != renderedMessage)
                        {
                            ClearChildren(Container);
                            Container.appendChild(renderedMessage);
                            Container.classList.add("tss-defer-with-loading-msg");
                        }
                    }
                },
                _loadingMessageDelay
            );

            Action<float, string> onProgress = (p, m) =>
            {
                if (currentID == id)
                {
                    var newMessage = _loadMessageGenerator(p, m);
                    // We always update if we have a new message component
                    if (newMessage != _currentLoadMessage)
                    {
                        ClearChildren(Container);
                        Container.appendChild(newMessage.Render());
                        _currentLoadMessage = newMessage;
                        Container.classList.add("tss-defer-with-loading-msg");
                    }
                }
            };

            _asyncGenerator(onProgress)
               .ContinueWith(r =>
                {
                    if (currentID == id)
                    {
                        id++;

                        Container.classList.remove("tss-defer-with-loading-msg");

                        ClearChildren(Container);

                        if (r.IsFaulted)
                        {
                            Container.appendChild(TextBlock("Error rendering async element").Danger());
                            Container.appendChild(TextBlock(r.Exception.ToString()).XSmall());
                            _refreshCompleted?.SetResult(false);
                        }
                        else if (r.IsCanceled)
                        {
                            _refreshCompleted?.SetResult(false);
                        }
                        else
                        {
                            if (r.Result is object)
                            {
                                Container.appendChild(r.Result.Render());
                            }
                            _refreshCompleted?.SetResult(true);
                        }

                        _refreshCompleted = null;

                        Tippy.CheckRepositionNeeded(Container);
                    }
                })
               .FireAndForget();
        }
    }
}
