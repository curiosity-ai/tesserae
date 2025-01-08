using System;
using System.Threading.Tasks;
using static H5.Core.dom;
using static Tesserae.UI;
using TNT;
using static TNT.T;

namespace Tesserae
{
    // This class has a different name than the static method in the UI class due to a bug in the bridge compiler that ends up calling the wrong constructor.
    // It is also internal to Tesserae to hide it from the compiler, which then exposes only the IDefer interface
    [H5.Name("tss.DC")]
    internal sealed class DeferedComponent : IDefer
    {
        private readonly IComponent             _loadMessage;
        private readonly Func<Task<IComponent>> _asyncGenerator;
        private          TextBlock              _defaultLoadingMessageIfAny;
        private          bool                   _needsRefresh, _waitForComponentToBeMountedBeforeFullyInitiatingRender, _renderHasBeenCalled;
        private          DebouncerWithMaxDelay  _debouncer;
        private          TaskCompletionSource<bool> _refreshCompleted;


        private int id = 0;
        private DeferedComponent(Func<Task<IComponent>> asyncGenerator, IComponent loadMessage, TextBlock defaultLoadingMessageIfAny)
        {
            if (loadMessage is null)
                throw new ArgumentNullException(nameof(loadMessage));

            _loadMessage                                            = loadMessage;
            _asyncGenerator                                         = asyncGenerator ?? throw new ArgumentNullException(nameof(asyncGenerator));
            _defaultLoadingMessageIfAny                             = defaultLoadingMessageIfAny;
            _needsRefresh                                           = true;
            _waitForComponentToBeMountedBeforeFullyInitiatingRender = true; // 2020-07-02 DWR: This has only just become configurable and the default value matches the previous behaviour - wait for DomObserver.WhenMounted in Render() before calling TriggerRefresh()
            _renderHasBeenCalled                                    = false;
            Container                                               = DIV(loadMessage.Render());

            _debouncer = new DebouncerWithMaxDelay(() => TriggerRefresh());

        }

        internal static DeferedComponent Create(Func<Task<IComponent>> asyncGenerator, IComponent loadMessage)
        {
            if (asyncGenerator is null)
                throw new ArgumentNullException(nameof(asyncGenerator));

            TextBlock defaultLoadingMessage;

            if (loadMessage is null)
            {
                defaultLoadingMessage = TextBlock(textSize: TextSize.XSmall).Class("tss-defer-loading-msg");
                loadMessage           = defaultLoadingMessage;
            }
            else
            {
                defaultLoadingMessage = null;
            }
            return new DeferedComponent(asyncGenerator, loadMessage, defaultLoadingMessage);
        }

        internal static DeferedComponent Create(Func<Task<IComponent>> asyncGenerator) => Create(asyncGenerator, null);

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

        /// <summary>
        /// The milliseconds must be a value of at least one, trying to disable Debounce by passing a zero (or negative) value is not supported
        public IDefer Debounce(int delayInMs)
        {
            _debouncer = new DebouncerWithMaxDelay(() => TriggerRefresh(), delayInMs: delayInMs);
            return this;
        }

        public IDefer Debounce(int delayInMs, int maxDelayInMs)
        {
            _debouncer = new DebouncerWithMaxDelay(() => TriggerRefresh(), delayInMs: delayInMs, maxDelayInMs: maxDelayInMs);

            return this;
        }

        /// <summary>
        /// By default, the component will generate an empty container and only start to initiate the data retrieval and full rendering process when it is mounted in the DOM (so that things like height calculations may be performed accurately, which require that
        /// the component exist in its expected location in the DOM) but this can be expensive if rendering many items. If it is known that the component is immediately going to be mounted then this method may be called and the DomObserver.WhenMounted logic will
        /// be bypassed and replaced with a simple setTimeout of very short duration (to allow the immediate rendering of the element to take place).
        /// </summary>
        public IDefer DoNotWaitForComponentMountingBeforeRendering()
        {
            _waitForComponentToBeMountedBeforeFullyInitiatingRender = false;
            return this;
        }

        public HTMLElement Render()
        {
            // 2020-07-02 DWR: Don't repeat the TriggerRefresh-when-ready logic if it's already been performed once for this component - the TriggerRefresh method checks the _needsRefresh flag and so wouldn't initiate any work but we would still be causing work
            // ork for ourselves if we needlessly called it and we're registering additional callbacks that is wasteful (particuarly DomObserver.WhenMounted can be expensive if there are many components registered, so that's the primary case to improve here)
            if (!_renderHasBeenCalled)
            {
                if (_waitForComponentToBeMountedBeforeFullyInitiatingRender)
                {
                    // Wait until we know that the container has been mounted before starting to load the content and render fully - this is the ideal case because we know for sure that we're rendering directly into the DOM and that any height-based calculations,
                    // for example, will give accurate results. However, if we're rendering a lot of items then DomObserver.WhenMounted gets expensive because every time that the DOM has a new item added, the DomObserver has to check whether it affects the mounted
                    // state of any of the elements registered with it (which requires a lot of DOM-walking).
                    DomObserver.WhenMounted(Container, Refresh);
                }
                else
                {
                    // This approach should only be used if it is expected that the element is going to be mounted immediately (to continue the example above, this may be important for things like height calculations - you want the element to be mounterd so that
                    // you get accurate results for things like that). It's technically less "safe" than using DomObserver.WhenMounted but it's also a lot cheaper if many items are being rendered. This is not the default behaviour, it has to be opted into via a
                    // call to the DoNotWaitForComponentMountingBeforeRendering method.
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

            var container = ScrollBar.GetCorrectContainer(Container);

            id++;

            var currentID = id; //Save the last value so we only replace the content if the task that finished is the latest to have been triggered

            window.setTimeout(
                _ =>
                {
                    if (_defaultLoadingMessageIfAny is object)
                    {
                        _defaultLoadingMessageIfAny.Text = "Loading...".t();
                        container.classList.add("tss-defer-with-loading-msg");
                    }

                    if (currentID == id)
                    {
                        ClearChildren(container);
                        container.appendChild(_loadMessage.Render());
                    }
                },
                1_000
            );

            _asyncGenerator()
               .ContinueWith(r =>
                {
                    if (currentID == id)
                    {
                        id++;

                        if (_defaultLoadingMessageIfAny is object)
                        {
                            Container.classList.remove("tss-defer-with-loading-msg");
                        }

                        ClearChildren(container);

                        if (r.IsCompleted)
                        {
                            if (r.Result is object)
                            {
                                container.appendChild(r.Result.Render());
                            }
                        }
                        else
                        {
                            container.appendChild(TextBlock("Error rendering async element").Danger());
                            container.appendChild(TextBlock(r.Exception.ToString()).XSmall());
                        }

                        if (_refreshCompleted is object)
                        {
                            _refreshCompleted.SetResult(r.IsCompleted);
                            _refreshCompleted = null;
                        }
                    }
                })
               .FireAndForget();
        }

        internal static DeferedComponent Observe<T1>(IObservable<T1> o1, Func<T1, Task<IComponent>> asyncGenerator, IComponent loadMessage = null)
        {
            var d = Create(() => asyncGenerator(o1.Value), loadMessage);

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

        internal static DeferedComponent Observe<T1, T2>(IObservable<T1> o1, IObservable<T2> o2, Func<T1, T2, Task<IComponent>> asyncGenerator, IComponent loadMessage = null)
        {
            var d = Create(() => asyncGenerator(o1.Value, o2.Value), loadMessage);

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

        internal static DeferedComponent Observe<T1, T2, T3>(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, Func<T1, T2, T3, Task<IComponent>> asyncGenerator, IComponent loadMessage = null)
        {
            var d = Create(() => asyncGenerator(o1.Value, o2.Value, o3.Value), loadMessage);

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

        internal static DeferedComponent Observe<T1, T2, T3, T4>(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, IObservable<T4> o4, Func<T1, T2, T3, T4, Task<IComponent>> asyncGenerator, IComponent loadMessage = null)
        {
            var d = Create(() => asyncGenerator(o1.Value, o2.Value, o3.Value, o4.Value), loadMessage);

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

        internal static DeferedComponent Observe<T1, T2, T3, T4, T5>(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, IObservable<T4> o4, IObservable<T5> o5, Func<T1, T2, T3, T4, T5, Task<IComponent>> asyncGenerator, IComponent loadMessage = null)
        {
            var d = Create(() => asyncGenerator(o1.Value, o2.Value, o3.Value, o4.Value, o5.Value), loadMessage);

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

        internal static DeferedComponent Observe<T1, T2, T3, T4, T5, T6>(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, IObservable<T4> o4, IObservable<T5> o5, IObservable<T6> o6, Func<T1, T2, T3, T4, T5, T6, Task<IComponent>> asyncGenerator, IComponent loadMessage = null)
        {
            var d = Create(() => asyncGenerator(o1.Value, o2.Value, o3.Value, o4.Value, o5.Value, o6.Value), loadMessage);

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

        internal static DeferedComponent Observe<T1, T2, T3, T4, T5, T6, T7>(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, IObservable<T4> o4, IObservable<T5> o5, IObservable<T6> o6, IObservable<T7> o7, Func<T1, T2, T3, T4, T5, T6, T7, Task<IComponent>> asyncGenerator, IComponent loadMessage = null)
        {
            var d = Create(() => asyncGenerator(o1.Value, o2.Value, o3.Value, o4.Value, o5.Value, o6.Value, o7.Value), loadMessage);

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

        internal static DeferedComponent Observe<T1, T2, T3, T4, T5, T6, T7, T8>(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, IObservable<T4> o4, IObservable<T5> o5, IObservable<T6> o6, IObservable<T7> o7, IObservable<T8> o8, Func<T1, T2, T3, T4, T5, T6, T7, T8, Task<IComponent>> asyncGenerator, IComponent loadMessage = null)
        {
            var d = Create(() => asyncGenerator(o1.Value, o2.Value, o3.Value, o4.Value, o5.Value, o6.Value, o7.Value, o8.Value), loadMessage);

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

        internal static DeferedComponent Observe<T1, T2, T3, T4, T5, T6, T7, T8, T9>(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, IObservable<T4> o4, IObservable<T5> o5, IObservable<T6> o6, IObservable<T7> o7, IObservable<T8> o8, IObservable<T9> o9, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, Task<IComponent>> asyncGenerator, IComponent loadMessage = null)
        {
            var d = Create(() => asyncGenerator(o1.Value, o2.Value, o3.Value, o4.Value, o5.Value, o6.Value, o7.Value, o8.Value, o9.Value), loadMessage);

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

        internal static DeferedComponent Observe<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, IObservable<T4> o4, IObservable<T5> o5, IObservable<T6> o6, IObservable<T7> o7, IObservable<T8> o8, IObservable<T9> o9, IObservable<T10> o10, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, Task<IComponent>> asyncGenerator, IComponent loadMessage = null)
        {
            var d = Create(() => asyncGenerator(o1.Value, o2.Value, o3.Value, o4.Value, o5.Value, o6.Value, o7.Value, o8.Value, o9.Value, o10.Value), loadMessage);

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