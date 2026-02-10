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
                    var msg = TextBlock(string.IsNullOrEmpty(m) ? "Loading...".t() : m, textSize: TextSize.XSmall).SemiBold();
                    return VStack().AlignItemsCenter().WS().Children(ProgressIndicator().WS().Progress(p * 100).MT(8).MB(8), msg.PB(8));
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

        internal static DeferedComponentWithProgress Observe<T1>(IObservable<T1> o1, Func<T1, Action<float, string>, Task<IComponent>> asyncGenerator, Func<float, string, IComponent> loadMessageGenerator = null)
        {
            var d = Create((progress) => asyncGenerator(o1.Value, progress), loadMessageGenerator);

            DomObserver.WhenMounted(d.Container, () =>
            {
                o1.ObserveFutureChanges(DoRefresh);

                DomObserver.WhenRemoved(d.Container, () =>
                {
                    o1.StopObserving(DoRefresh);
                });
            });

            void DoRefresh<T>(T val)
            {
                d.Refresh();
            }

            return d;
        }

        internal static DeferedComponentWithProgress Observe<T1, T2>(IObservable<T1> o1, IObservable<T2> o2, Func<T1, T2, Action<float, string>, Task<IComponent>> asyncGenerator, Func<float, string, IComponent> loadMessageGenerator = null)
        {
            var d = Create((progress) => asyncGenerator(o1.Value, o2.Value, progress), loadMessageGenerator);

            DomObserver.WhenMounted(d.Container, () =>
            {
                o1.ObserveFutureChanges(DoRefresh);
                o2.ObserveFutureChanges(DoRefresh);

                DomObserver.WhenRemoved(d.Container, () =>
                {
                    o1.StopObserving(DoRefresh);
                    o2.StopObserving(DoRefresh);
                });
            });

            void DoRefresh<T>(T val)
            {
                d.Refresh();
            }

            return d;
        }

        internal static DeferedComponentWithProgress Observe<T1, T2, T3>(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, Func<T1, T2, T3, Action<float, string>, Task<IComponent>> asyncGenerator, Func<float, string, IComponent> loadMessageGenerator = null)
        {
            var d = Create((progress) => asyncGenerator(o1.Value, o2.Value, o3.Value, progress), loadMessageGenerator);

            DomObserver.WhenMounted(d.Container, () =>
            {
                o1.ObserveFutureChanges(DoRefresh);
                o2.ObserveFutureChanges(DoRefresh);
                o3.ObserveFutureChanges(DoRefresh);

                DomObserver.WhenRemoved(d.Container, () =>
                {
                    o1.StopObserving(DoRefresh);
                    o2.StopObserving(DoRefresh);
                    o3.StopObserving(DoRefresh);
                });
            });

            void DoRefresh<T>(T val)
            {
                d.Refresh();
            }

            return d;
        }

        internal static DeferedComponentWithProgress Observe<T1, T2, T3, T4>(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, IObservable<T4> o4, Func<T1, T2, T3, T4, Action<float, string>, Task<IComponent>> asyncGenerator, Func<float, string, IComponent> loadMessageGenerator = null)
        {
            var d = Create((progress) => asyncGenerator(o1.Value, o2.Value, o3.Value, o4.Value, progress), loadMessageGenerator);

            DomObserver.WhenMounted(d.Container, () =>
            {
                o1.ObserveFutureChanges(DoRefresh);
                o2.ObserveFutureChanges(DoRefresh);
                o3.ObserveFutureChanges(DoRefresh);
                o4.ObserveFutureChanges(DoRefresh);

                DomObserver.WhenRemoved(d.Container, () =>
                {
                    o1.StopObserving(DoRefresh);
                    o2.StopObserving(DoRefresh);
                    o3.StopObserving(DoRefresh);
                    o4.StopObserving(DoRefresh);
                });
            });

            void DoRefresh<T>(T val)
            {
                d.Refresh();
            }

            return d;
        }

        internal static DeferedComponentWithProgress Observe<T1, T2, T3, T4, T5>(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, IObservable<T4> o4, IObservable<T5> o5, Func<T1, T2, T3, T4, T5, Action<float, string>, Task<IComponent>> asyncGenerator, Func<float, string, IComponent> loadMessageGenerator = null)
        {
            var d = Create((progress) => asyncGenerator(o1.Value, o2.Value, o3.Value, o4.Value, o5.Value, progress), loadMessageGenerator);

            DomObserver.WhenMounted(d.Container, () =>
            {
                o1.ObserveFutureChanges(DoRefresh);
                o2.ObserveFutureChanges(DoRefresh);
                o3.ObserveFutureChanges(DoRefresh);
                o4.ObserveFutureChanges(DoRefresh);
                o5.ObserveFutureChanges(DoRefresh);

                DomObserver.WhenRemoved(d.Container, () =>
                {
                    o1.StopObserving(DoRefresh);
                    o2.StopObserving(DoRefresh);
                    o3.StopObserving(DoRefresh);
                    o4.StopObserving(DoRefresh);
                    o5.StopObserving(DoRefresh);
                });
            });

            void DoRefresh<T>(T val)
            {
                d.Refresh();
            }

            return d;
        }

        internal static DeferedComponentWithProgress Observe<T1, T2, T3, T4, T5, T6>(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, IObservable<T4> o4, IObservable<T5> o5, IObservable<T6> o6, Func<T1, T2, T3, T4, T5, T6, Action<float, string>, Task<IComponent>> asyncGenerator, Func<float, string, IComponent> loadMessageGenerator = null)
        {
            var d = Create((progress) => asyncGenerator(o1.Value, o2.Value, o3.Value, o4.Value, o5.Value, o6.Value, progress), loadMessageGenerator);

            DomObserver.WhenMounted(d.Container, () =>
            {
                o1.ObserveFutureChanges(DoRefresh);
                o2.ObserveFutureChanges(DoRefresh);
                o3.ObserveFutureChanges(DoRefresh);
                o4.ObserveFutureChanges(DoRefresh);
                o5.ObserveFutureChanges(DoRefresh);
                o6.ObserveFutureChanges(DoRefresh);

                DomObserver.WhenRemoved(d.Container, () =>
                {
                    o1.StopObserving(DoRefresh);
                    o2.StopObserving(DoRefresh);
                    o3.StopObserving(DoRefresh);
                    o4.StopObserving(DoRefresh);
                    o5.StopObserving(DoRefresh);
                    o6.StopObserving(DoRefresh);
                });
            });

            void DoRefresh<T>(T val)
            {
                d.Refresh();
            }

            return d;
        }

        internal static DeferedComponentWithProgress Observe<T1, T2, T3, T4, T5, T6, T7>(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, IObservable<T4> o4, IObservable<T5> o5, IObservable<T6> o6, IObservable<T7> o7, Func<T1, T2, T3, T4, T5, T6, T7, Action<float, string>, Task<IComponent>> asyncGenerator, Func<float, string, IComponent> loadMessageGenerator = null)
        {
            var d = Create((progress) => asyncGenerator(o1.Value, o2.Value, o3.Value, o4.Value, o5.Value, o6.Value, o7.Value, progress), loadMessageGenerator);

            DomObserver.WhenMounted(d.Container, () =>
            {
                o1.ObserveFutureChanges(DoRefresh);
                o2.ObserveFutureChanges(DoRefresh);
                o3.ObserveFutureChanges(DoRefresh);
                o4.ObserveFutureChanges(DoRefresh);
                o5.ObserveFutureChanges(DoRefresh);
                o6.ObserveFutureChanges(DoRefresh);
                o7.ObserveFutureChanges(DoRefresh);

                DomObserver.WhenRemoved(d.Container, () =>
                {
                    o1.StopObserving(DoRefresh);
                    o2.StopObserving(DoRefresh);
                    o3.StopObserving(DoRefresh);
                    o4.StopObserving(DoRefresh);
                    o5.StopObserving(DoRefresh);
                    o6.StopObserving(DoRefresh);
                    o7.StopObserving(DoRefresh);
                });
            });

            void DoRefresh<T>(T val)
            {
                d.Refresh();
            }

            return d;
        }

        internal static DeferedComponentWithProgress Observe<T1, T2, T3, T4, T5, T6, T7, T8>(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, IObservable<T4> o4, IObservable<T5> o5, IObservable<T6> o6, IObservable<T7> o7, IObservable<T8> o8, Func<T1, T2, T3, T4, T5, T6, T7, T8, Action<float, string>, Task<IComponent>> asyncGenerator, Func<float, string, IComponent> loadMessageGenerator = null)
        {
            var d = Create((progress) => asyncGenerator(o1.Value, o2.Value, o3.Value, o4.Value, o5.Value, o6.Value, o7.Value, o8.Value, progress), loadMessageGenerator);

            DomObserver.WhenMounted(d.Container, () =>
            {
                o1.ObserveFutureChanges(DoRefresh);
                o2.ObserveFutureChanges(DoRefresh);
                o3.ObserveFutureChanges(DoRefresh);
                o4.ObserveFutureChanges(DoRefresh);
                o5.ObserveFutureChanges(DoRefresh);
                o6.ObserveFutureChanges(DoRefresh);
                o7.ObserveFutureChanges(DoRefresh);
                o8.ObserveFutureChanges(DoRefresh);

                DomObserver.WhenRemoved(d.Container, () =>
                {
                    o1.StopObserving(DoRefresh);
                    o2.StopObserving(DoRefresh);
                    o3.StopObserving(DoRefresh);
                    o4.StopObserving(DoRefresh);
                    o5.StopObserving(DoRefresh);
                    o6.StopObserving(DoRefresh);
                    o7.StopObserving(DoRefresh);
                    o8.StopObserving(DoRefresh);
                });
            });

            void DoRefresh<T>(T val)
            {
                d.Refresh();
            }

            return d;
        }

        internal static DeferedComponentWithProgress Observe<T1, T2, T3, T4, T5, T6, T7, T8, T9>(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, IObservable<T4> o4, IObservable<T5> o5, IObservable<T6> o6, IObservable<T7> o7, IObservable<T8> o8, IObservable<T9> o9, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, Action<float, string>, Task<IComponent>> asyncGenerator, Func<float, string, IComponent> loadMessageGenerator = null)
        {
            var d = Create((progress) => asyncGenerator(o1.Value, o2.Value, o3.Value, o4.Value, o5.Value, o6.Value, o7.Value, o8.Value, o9.Value, progress), loadMessageGenerator);

            DomObserver.WhenMounted(d.Container, () =>
            {
                o1.ObserveFutureChanges(DoRefresh);
                o2.ObserveFutureChanges(DoRefresh);
                o3.ObserveFutureChanges(DoRefresh);
                o4.ObserveFutureChanges(DoRefresh);
                o5.ObserveFutureChanges(DoRefresh);
                o6.ObserveFutureChanges(DoRefresh);
                o7.ObserveFutureChanges(DoRefresh);
                o8.ObserveFutureChanges(DoRefresh);
                o9.ObserveFutureChanges(DoRefresh);

                DomObserver.WhenRemoved(d.Container, () =>
                {
                    o1.StopObserving(DoRefresh);
                    o2.StopObserving(DoRefresh);
                    o3.StopObserving(DoRefresh);
                    o4.StopObserving(DoRefresh);
                    o5.StopObserving(DoRefresh);
                    o6.StopObserving(DoRefresh);
                    o7.StopObserving(DoRefresh);
                    o8.StopObserving(DoRefresh);
                    o9.StopObserving(DoRefresh);
                });
            });

            void DoRefresh<T>(T val)
            {
                d.Refresh();
            }

            return d;
        }

        internal static DeferedComponentWithProgress Observe<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, IObservable<T4> o4, IObservable<T5> o5, IObservable<T6> o6, IObservable<T7> o7, IObservable<T8> o8, IObservable<T9> o9, IObservable<T10> o10, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, Action<float, string>, Task<IComponent>> asyncGenerator, Func<float, string, IComponent> loadMessageGenerator = null)
        {
            var d = Create((progress) => asyncGenerator(o1.Value, o2.Value, o3.Value, o4.Value, o5.Value, o6.Value, o7.Value, o8.Value, o9.Value, o10.Value, progress), loadMessageGenerator);

            DomObserver.WhenMounted(d.Container, () =>
            {
                o1.ObserveFutureChanges(DoRefresh);
                o2.ObserveFutureChanges(DoRefresh);
                o3.ObserveFutureChanges(DoRefresh);
                o4.ObserveFutureChanges(DoRefresh);
                o5.ObserveFutureChanges(DoRefresh);
                o6.ObserveFutureChanges(DoRefresh);
                o7.ObserveFutureChanges(DoRefresh);
                o8.ObserveFutureChanges(DoRefresh);
                o9.ObserveFutureChanges(DoRefresh);
                o10.ObserveFutureChanges(DoRefresh);

                DomObserver.WhenRemoved(d.Container, () =>
                {
                    o1.StopObserving(DoRefresh);
                    o2.StopObserving(DoRefresh);
                    o3.StopObserving(DoRefresh);
                    o4.StopObserving(DoRefresh);
                    o5.StopObserving(DoRefresh);
                    o6.StopObserving(DoRefresh);
                    o7.StopObserving(DoRefresh);
                    o8.StopObserving(DoRefresh);
                    o9.StopObserving(DoRefresh);
                    o10.StopObserving(DoRefresh);
                });
            });

            void DoRefresh<T>(T val)
            {
                d.Refresh();
            }

            return d;
        }

    }
}
