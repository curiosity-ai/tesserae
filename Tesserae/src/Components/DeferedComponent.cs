using System;
using System.Threading.Tasks;
using Tesserae.HTML;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae.Components
{
    // This class has a different name than the static method in the UI class due to a bug in the bridge compiler that ends up calling the wrong constructor.
    // It is also internal to Tesserae to hide it from the compiler, which then exposes only the IDefer interface
    internal sealed class DeferedComponent : IDefer
    {
        private readonly Func<Task<IComponent>> _asyncGenerator;
        private TextBlock _defaultLoadingMessageIfAny;
        private bool _needsRefresh, _waitForComponentToBeMountedBeforeFullyInitiatingRender, _renderHasBeenCalled;
        private double _refreshTimeout;
        private int _delayInMs = 1;
        private DeferedComponent(Func<Task<IComponent>> asyncGenerator, IComponent loadMessage, TextBlock defaultLoadingMessageIfAny)
        {
            if (loadMessage is null)
                throw new ArgumentNullException(nameof(loadMessage));

            _asyncGenerator = asyncGenerator ?? throw new ArgumentNullException(nameof(asyncGenerator));
            _defaultLoadingMessageIfAny = defaultLoadingMessageIfAny;
            _needsRefresh = true;
            _waitForComponentToBeMountedBeforeFullyInitiatingRender = true; // 2020-07-02 DWR: This has only just become configurable and the default value matches the previous behaviour - wait for DomObserver.WhenMounted in Render() before calling TriggerRefresh()
            _renderHasBeenCalled = false;
            Container = DIV(loadMessage.Render());
        }

        internal static DeferedComponent Create(Func<Task<IComponent>> asyncGenerator, IComponent loadMessage)
        {
            if (asyncGenerator is null)
                throw new ArgumentNullException(nameof(asyncGenerator));

            TextBlock defaultLoadingMessage;
            if (loadMessage is null)
            {
                defaultLoadingMessage = TextBlock().XSmall();
                loadMessage = defaultLoadingMessage;
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
            window.clearTimeout(_refreshTimeout);
            _refreshTimeout = window.setTimeout(
                t => TriggerRefresh(),
                _delayInMs
            );
        }

        /// <summary>
        /// The milliseconds must be a value of at least one, trying to disable Debounce by passing a zero (or negative) value is not supported
        public IDefer Debounce(int milliseconds)
        {
            if (_delayInMs <= 0)
                throw new ArgumentOutOfRangeException(nameof(milliseconds), "must be a positive value");

            _delayInMs = milliseconds;
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
                    DomObserver.WhenMounted(Container, TriggerRefresh);
                }
                else
                {
                    // This approach should only be used if it is expected that the element is going to be mounted immediately (to continue the example above, this may be important for things like height calculations - you want the element to be mounterd so that
                    // you get accurate results for things like that). It's technically less "safe" than using DomObserver.WhenMounted but it's also a lot cheaper if many items are being rendered. This is not the default behaviour, it has to be opted into via a
                    // call to the DoNotWaitForComponentMountingBeforeRendering method.
                    setTimeout(_ => TriggerRefresh(), 1);
                }
                _renderHasBeenCalled = true;
            }
            return Container;
        }

        internal HTMLElement Container { get; }

        private void TriggerRefresh()
        {
            if (!_needsRefresh)
                return;
            
            _needsRefresh = false;

            window.setTimeout(
                _ =>
                {
                    if (_defaultLoadingMessageIfAny is object)
                    {
                        _defaultLoadingMessageIfAny.Text = "loading...";
                    }
                },
                1_000
            );

            var container = ScrollBar.GetCorrectContainer(Container);
            _asyncGenerator()
                .ContinueWith(r =>
                {
                    _defaultLoadingMessageIfAny = null;
                    ClearChildren(container);
                    if (r.IsCompleted)
                    {
                        container.appendChild(r.Result.Render());
                    }
                    else
                    {
                        container.appendChild(TextBlock("Error rendering async element").Danger());
                        container.appendChild(TextBlock(r.Exception.ToString()).XSmall());
                    }
                })
                .FireAndForget();
        }

        internal static DeferedComponent Observe<T1>(IObservable<T1> o1, Func<T1, Task<IComponent>> asyncGenerator, IComponent loadMessage = null)
        {
            var d = Create(() => asyncGenerator(o1.Value), loadMessage);
            o1.ObserveFutureChanges(_ => d.Refresh());
            return d;
        }

        internal static DeferedComponent Observe<T1, T2>(IObservable<T1> o1, IObservable<T2> o2, Func<T1, T2, Task<IComponent>> asyncGenerator, IComponent loadMessage = null)
        {
            var d = Create(() => asyncGenerator(o1.Value, o2.Value), loadMessage);
            o1.ObserveFutureChanges(_ => d.Refresh());
            o2.ObserveFutureChanges(_ => d.Refresh());
            return d;
        }

        internal static DeferedComponent Observe<T1, T2, T3>(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, Func<T1, T2, T3, Task<IComponent>> asyncGenerator, IComponent loadMessage = null)
        {
            var d = Create(() => asyncGenerator(o1.Value, o2.Value, o3.Value), loadMessage);
            o1.ObserveFutureChanges(_ => d.Refresh());
            o2.ObserveFutureChanges(_ => d.Refresh());
            o3.ObserveFutureChanges(_ => d.Refresh());
            return d;
        }

        internal static DeferedComponent Observe<T1, T2, T3, T4>(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, IObservable<T4> o4, Func<T1, T2, T3, T4, Task<IComponent>> asyncGenerator, IComponent loadMessage = null)
        {
            var d = Create(() => asyncGenerator(o1.Value, o2.Value, o3.Value, o4.Value), loadMessage);
            o1.ObserveFutureChanges(_ => d.Refresh());
            o2.ObserveFutureChanges(_ => d.Refresh());
            o3.ObserveFutureChanges(_ => d.Refresh());
            o4.ObserveFutureChanges(_ => d.Refresh());
            return d;
        }

        internal static DeferedComponent Observe<T1, T2, T3, T4, T5>(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, IObservable<T4> o4, IObservable<T5> o5, Func<T1, T2, T3, T4, T5, Task<IComponent>> asyncGenerator, IComponent loadMessage = null)
        {
            var d = Create(() => asyncGenerator(o1.Value, o2.Value, o3.Value, o4.Value, o5.Value), loadMessage);
            o1.ObserveFutureChanges(_ => d.Refresh());
            o2.ObserveFutureChanges(_ => d.Refresh());
            o3.ObserveFutureChanges(_ => d.Refresh());
            o4.ObserveFutureChanges(_ => d.Refresh());
            o5.ObserveFutureChanges(_ => d.Refresh());
            return d;
        }

        internal static DeferedComponent Observe<T1, T2, T3, T4, T5, T6>(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, IObservable<T4> o4, IObservable<T5> o5, IObservable<T6> o6, Func<T1, T2, T3, T4, T5, T6, Task<IComponent>> asyncGenerator, IComponent loadMessage = null)
        {
            var d = Create(() => asyncGenerator(o1.Value, o2.Value, o3.Value, o4.Value, o5.Value, o6.Value), loadMessage);
            o1.ObserveFutureChanges(_ => d.Refresh());
            o2.ObserveFutureChanges(_ => d.Refresh());
            o3.ObserveFutureChanges(_ => d.Refresh());
            o4.ObserveFutureChanges(_ => d.Refresh());
            o5.ObserveFutureChanges(_ => d.Refresh());
            o6.ObserveFutureChanges(_ => d.Refresh());
            return d;
        }

        internal static DeferedComponent Observe<T1, T2, T3, T4, T5, T6, T7>(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, IObservable<T4> o4, IObservable<T5> o5, IObservable<T6> o6, IObservable<T7> o7, Func<T1, T2, T3, T4, T5, T6, T7, Task<IComponent>> asyncGenerator, IComponent loadMessage = null)
        {
            var d = Create(() => asyncGenerator(o1.Value, o2.Value, o3.Value, o4.Value, o5.Value, o6.Value, o7.Value), loadMessage);
            o1.ObserveFutureChanges(_ => d.Refresh());
            o2.ObserveFutureChanges(_ => d.Refresh());
            o3.ObserveFutureChanges(_ => d.Refresh());
            o4.ObserveFutureChanges(_ => d.Refresh());
            o5.ObserveFutureChanges(_ => d.Refresh());
            o6.ObserveFutureChanges(_ => d.Refresh());
            o7.ObserveFutureChanges(_ => d.Refresh());
            return d;
        }

        internal static DeferedComponent Observe<T1, T2, T3, T4, T5, T6, T7, T8>(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, IObservable<T4> o4, IObservable<T5> o5, IObservable<T6> o6, IObservable<T7> o7, IObservable<T8> o8, Func<T1, T2, T3, T4, T5, T6, T7, T8, Task<IComponent>> asyncGenerator, IComponent loadMessage = null)
        {
            var d = Create(() => asyncGenerator(o1.Value, o2.Value, o3.Value, o4.Value, o5.Value, o6.Value, o7.Value, o8.Value), loadMessage);
            o1.ObserveFutureChanges(_ => d.Refresh());
            o2.ObserveFutureChanges(_ => d.Refresh());
            o3.ObserveFutureChanges(_ => d.Refresh());
            o4.ObserveFutureChanges(_ => d.Refresh());
            o5.ObserveFutureChanges(_ => d.Refresh());
            o6.ObserveFutureChanges(_ => d.Refresh());
            o7.ObserveFutureChanges(_ => d.Refresh());
            o8.ObserveFutureChanges(_ => d.Refresh());
            return d;
        }

        internal static DeferedComponent Observe<T1, T2, T3, T4, T5, T6, T7, T8, T9>(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, IObservable<T4> o4, IObservable<T5> o5, IObservable<T6> o6, IObservable<T7> o7, IObservable<T8> o8, IObservable<T9> o9, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, Task<IComponent>> asyncGenerator, IComponent loadMessage = null)
        {
            var d = Create(() => asyncGenerator(o1.Value, o2.Value, o3.Value, o4.Value, o5.Value, o6.Value, o7.Value, o8.Value, o9.Value), loadMessage);
            o1.ObserveFutureChanges(_ => d.Refresh());
            o2.ObserveFutureChanges(_ => d.Refresh());
            o3.ObserveFutureChanges(_ => d.Refresh());
            o4.ObserveFutureChanges(_ => d.Refresh());
            o5.ObserveFutureChanges(_ => d.Refresh());
            o6.ObserveFutureChanges(_ => d.Refresh());
            o7.ObserveFutureChanges(_ => d.Refresh());
            o8.ObserveFutureChanges(_ => d.Refresh());
            o9.ObserveFutureChanges(_ => d.Refresh());
            return d;
        }

        internal static DeferedComponent Observe<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, IObservable<T4> o4, IObservable<T5> o5, IObservable<T6> o6, IObservable<T7> o7, IObservable<T8> o8, IObservable<T9> o9, IObservable<T10> o10, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, Task<IComponent>> asyncGenerator, IComponent loadMessage = null)
        {
            var d = Create(() => asyncGenerator(o1.Value, o2.Value, o3.Value, o4.Value, o5.Value, o6.Value, o7.Value, o8.Value, o9.Value, o10.Value), loadMessage);
            o1.ObserveFutureChanges(_ => d.Refresh());
            o2.ObserveFutureChanges(_ => d.Refresh());
            o3.ObserveFutureChanges(_ => d.Refresh());
            o4.ObserveFutureChanges(_ => d.Refresh());
            o5.ObserveFutureChanges(_ => d.Refresh());
            o6.ObserveFutureChanges(_ => d.Refresh());
            o7.ObserveFutureChanges(_ => d.Refresh());
            o8.ObserveFutureChanges(_ => d.Refresh());
            o9.ObserveFutureChanges(_ => d.Refresh());
            o10.ObserveFutureChanges(_ => d.Refresh());
            return d;
        }
    }

    //Generator code:

    //var sb = new StringBuilder(); // For Defer.cs
    //var sb2 = new StringBuilder(); //For UI.Components.cs
    //for(int i = 1; i <= 10; i++)
    //{
    //	var t = string.Join(", ", Enumerable.Range(1, i).Select(a => $"T{a}"));
    //	var ot = string.Join(", ", Enumerable.Range(1, i).Select(a => $"IObservable<T{a}> o{a}"));
    //  var ot2 = string.Join(", ", Enumerable.Range(1, i).Select(a => $"o{a}"));
    //	var vt = string.Join(", ", Enumerable.Range(1, i).Select(a => $"o{a}.Value"));
    //  sb2.AppendLine($"public static Defer Defer<{t}>({ot}, Func<{t}, Task<IComponent>> asyncGenerator, IComponent loadMessage = null) => Components.Defer.Observe({ot2}, asyncGenerator, loadMessage);");
    //	sb.AppendLine($"internal  static Defer Observe<{t}>({ot}, Func<{t}, Task<IComponent>> asyncGenerator, IComponent loadMessage = null)");
    //	sb.AppendLine("{");
    //	sb.AppendLine($"    var d = Create(() => asyncGenerator({vt}), loadMessage);");
    //	for(int j = 1; j <= i; j++)
    //	{
    //		sb.AppendLine($"    o{j}.ObserveFutureChanges(_ => d.Refresh());");
    //	}
    //	sb.AppendLine("   return d;");
    //	sb.AppendLine("}").AppendLine();
    //}
    //Console.WriteLine(sb.ToString());
    //Console.WriteLine(sb2.ToString());
}