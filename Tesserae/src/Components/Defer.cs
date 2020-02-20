using Retyped;
using System;
using System.Threading.Tasks;
using Tesserae.HTML;
using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class Defer : IComponent
    {
        public IComponent LoadMessage { get; }
        private Func<Task<IComponent>> _asyncGenerator;

        internal dom.HTMLElement InnerElement;
        private bool NeedsRefresh;

        public Defer(Func<Task<IComponent>> asyncGenerator, IComponent loadMessage = null)
        {
            LoadMessage = loadMessage ?? TextBlock("loading...").XSmall();
            _asyncGenerator = asyncGenerator;
            NeedsRefresh = true;
            InnerElement = DIV(LoadMessage.Render());
        }

        private double _refreshTimeout;
        public void Refresh()
        {
            NeedsRefresh = true;
            window.clearTimeout(_refreshTimeout);
            _refreshTimeout = window.setTimeout((t) => TriggerRefresh(), 1);
        }

        public dom.HTMLElement Render()
        {
            DomMountedObserver.NotifyWhenMounted(InnerElement, () => TriggerRefresh());
            return InnerElement;
        }

        private void TriggerRefresh()
        {
            if (!NeedsRefresh) return;
            NeedsRefresh = false;
            ClearChildren(InnerElement);
            InnerElement.appendChild(LoadMessage.Render());
            var task = _asyncGenerator();
            task.ContinueWith(r =>
            {
                ClearChildren(InnerElement);
                if (r.IsCompleted)
                {
                    InnerElement.appendChild(r.Result.Render());
                }
                else
                {
                    InnerElement.appendChild(TextBlock("Error rendering async element").Danger());
                    InnerElement.appendChild(TextBlock(r.Exception.ToString()).XSmall());
                }
            }).FireAndForget();
        }

        public static Defer Observe<T1>(Observable<T1> o1, Func<T1, Task<IComponent>> asyncGenerator, IComponent loadMessage = null)
        {
            var d = new Defer(() => asyncGenerator(o1.Value), loadMessage);
            o1.ObserveLazy((v) => d.Refresh());
            return d;
        }

        public static Defer Observe<T1, T2>(Observable<T1> o1, Observable<T2> o2, Func<T1, T2, Task<IComponent>> asyncGenerator, IComponent loadMessage = null)
        {
            var d = new Defer(() => asyncGenerator(o1.Value, o2.Value), loadMessage);
            o1.ObserveLazy((v) => d.Refresh());
            o2.ObserveLazy((v) => d.Refresh());
            return d;
        }

        public static Defer Observe<T1, T2, T3>(Observable<T1> o1, Observable<T2> o2, Observable<T3> o3, Func<T1, T2, T3, Task<IComponent>> asyncGenerator, IComponent loadMessage = null)
        {
            var d = new Defer(() => asyncGenerator(o1.Value, o2.Value, o3.Value), loadMessage);
            o1.ObserveLazy((v) => d.Refresh());
            o2.ObserveLazy((v) => d.Refresh());
            o3.ObserveLazy((v) => d.Refresh());
            return d;
        }

        public static Defer Observe<T1, T2, T3, T4>(Observable<T1> o1, Observable<T2> o2, Observable<T3> o3, Observable<T4> o4, Func<T1, T2, T3, T4, Task<IComponent>> asyncGenerator, IComponent loadMessage = null)
        {
            var d = new Defer(() => asyncGenerator(o1.Value, o2.Value, o3.Value, o4.Value), loadMessage);
            o1.ObserveLazy((v) => d.Refresh());
            o2.ObserveLazy((v) => d.Refresh());
            o3.ObserveLazy((v) => d.Refresh());
            o4.ObserveLazy((v) => d.Refresh());
            return d;
        }

        public static Defer Observe<T1, T2, T3, T4, T5>(Observable<T1> o1, Observable<T2> o2, Observable<T3> o3, Observable<T4> o4, Observable<T5> o5, Func<T1, T2, T3, T4, T5, Task<IComponent>> asyncGenerator, IComponent loadMessage = null)
        {
            var d = new Defer(() => asyncGenerator(o1.Value, o2.Value, o3.Value, o4.Value, o5.Value), loadMessage);
            o1.ObserveLazy((v) => d.Refresh());
            o2.ObserveLazy((v) => d.Refresh());
            o3.ObserveLazy((v) => d.Refresh());
            o4.ObserveLazy((v) => d.Refresh());
            o5.ObserveLazy((v) => d.Refresh());
            return d;
        }

        public static Defer Observe<T1, T2, T3, T4, T5, T6>(Observable<T1> o1, Observable<T2> o2, Observable<T3> o3, Observable<T4> o4, Observable<T5> o5, Observable<T6> o6, Func<T1, T2, T3, T4, T5, T6, Task<IComponent>> asyncGenerator, IComponent loadMessage = null)
        {
            var d = new Defer(() => asyncGenerator(o1.Value, o2.Value, o3.Value, o4.Value, o5.Value, o6.Value), loadMessage);
            o1.ObserveLazy((v) => d.Refresh());
            o2.ObserveLazy((v) => d.Refresh());
            o3.ObserveLazy((v) => d.Refresh());
            o4.ObserveLazy((v) => d.Refresh());
            o5.ObserveLazy((v) => d.Refresh());
            o6.ObserveLazy((v) => d.Refresh());
            return d;
        }

        public static Defer Observe<T1, T2, T3, T4, T5, T6, T7>(Observable<T1> o1, Observable<T2> o2, Observable<T3> o3, Observable<T4> o4, Observable<T5> o5, Observable<T6> o6, Observable<T7> o7, Func<T1, T2, T3, T4, T5, T6, T7, Task<IComponent>> asyncGenerator, IComponent loadMessage = null)
        {
            var d = new Defer(() => asyncGenerator(o1.Value, o2.Value, o3.Value, o4.Value, o5.Value, o6.Value, o7.Value), loadMessage);
            o1.ObserveLazy((v) => d.Refresh());
            o2.ObserveLazy((v) => d.Refresh());
            o3.ObserveLazy((v) => d.Refresh());
            o4.ObserveLazy((v) => d.Refresh());
            o5.ObserveLazy((v) => d.Refresh());
            o6.ObserveLazy((v) => d.Refresh());
            o7.ObserveLazy((v) => d.Refresh());
            return d;
        }

        public static Defer Observe<T1, T2, T3, T4, T5, T6, T7, T8>(Observable<T1> o1, Observable<T2> o2, Observable<T3> o3, Observable<T4> o4, Observable<T5> o5, Observable<T6> o6, Observable<T7> o7, Observable<T8> o8, Func<T1, T2, T3, T4, T5, T6, T7, T8, Task<IComponent>> asyncGenerator, IComponent loadMessage = null)
        {
            var d = new Defer(() => asyncGenerator(o1.Value, o2.Value, o3.Value, o4.Value, o5.Value, o6.Value, o7.Value, o8.Value), loadMessage);
            o1.ObserveLazy((v) => d.Refresh());
            o2.ObserveLazy((v) => d.Refresh());
            o3.ObserveLazy((v) => d.Refresh());
            o4.ObserveLazy((v) => d.Refresh());
            o5.ObserveLazy((v) => d.Refresh());
            o6.ObserveLazy((v) => d.Refresh());
            o7.ObserveLazy((v) => d.Refresh());
            o8.ObserveLazy((v) => d.Refresh());
            return d;
        }

        public static Defer Observe<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Observable<T1> o1, Observable<T2> o2, Observable<T3> o3, Observable<T4> o4, Observable<T5> o5, Observable<T6> o6, Observable<T7> o7, Observable<T8> o8, Observable<T9> o9, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, Task<IComponent>> asyncGenerator, IComponent loadMessage = null)
        {
            var d = new Defer(() => asyncGenerator(o1.Value, o2.Value, o3.Value, o4.Value, o5.Value, o6.Value, o7.Value, o8.Value, o9.Value), loadMessage);
            o1.ObserveLazy((v) => d.Refresh());
            o2.ObserveLazy((v) => d.Refresh());
            o3.ObserveLazy((v) => d.Refresh());
            o4.ObserveLazy((v) => d.Refresh());
            o5.ObserveLazy((v) => d.Refresh());
            o6.ObserveLazy((v) => d.Refresh());
            o7.ObserveLazy((v) => d.Refresh());
            o8.ObserveLazy((v) => d.Refresh());
            o9.ObserveLazy((v) => d.Refresh());
            return d;
        }

        public static Defer Observe<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Observable<T1> o1, Observable<T2> o2, Observable<T3> o3, Observable<T4> o4, Observable<T5> o5, Observable<T6> o6, Observable<T7> o7, Observable<T8> o8, Observable<T9> o9, Observable<T10> o10, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, Task<IComponent>> asyncGenerator, IComponent loadMessage = null)
        {
            var d = new Defer(() => asyncGenerator(o1.Value, o2.Value, o3.Value, o4.Value, o5.Value, o6.Value, o7.Value, o8.Value, o9.Value, o10.Value), loadMessage);
            o1.ObserveLazy((v) => d.Refresh());
            o2.ObserveLazy((v) => d.Refresh());
            o3.ObserveLazy((v) => d.Refresh());
            o4.ObserveLazy((v) => d.Refresh());
            o5.ObserveLazy((v) => d.Refresh());
            o6.ObserveLazy((v) => d.Refresh());
            o7.ObserveLazy((v) => d.Refresh());
            o8.ObserveLazy((v) => d.Refresh());
            o9.ObserveLazy((v) => d.Refresh());
            o10.ObserveLazy((v) => d.Refresh());
            return d;
        }
    }


    //Generator code:

    //var sb = new StringBuilder();
    //for(int i = 1; i <= 10; i++)
    //{
    //	var t = string.Join(", ", Enumerable.Range(1, i).Select(a => $"T{a}"));
    //	var ot = string.Join(", ", Enumerable.Range(1, i).Select(a => $"Observable<T{a}> o{a}"));
    //	var vt = string.Join(", ", Enumerable.Range(1, i).Select(a => $"o{a}.Value"));
    //	sb.AppendLine($"public static Defer Observe<{t}>({ot}, Func<{t}, Task<IComponent>> asyncGenerator, IComponent loadMessage = null)");
    //	sb.AppendLine("{");
    //	sb.AppendLine($"    var d = new Defer(() => asyncGenerator({vt}), loadMessage);");
    //	for(int j = 1; j <= i; j++)
    //	{
    //		sb.AppendLine($"    o{j}.ObserveLazy((v) => d.Refresh());");
    //	}
    //	sb.AppendLine("   return d;");
    //	sb.AppendLine("}").AppendLine();
    //}
    //Console.WriteLine(sb.ToString());
}