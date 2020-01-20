using Retyped;
using System;
using System.Threading.Tasks;
using Tesserae.HTML;
using static Tesserae.UI;

namespace Tesserae.Components
{
    public class Defer : IComponent
    {
        public IComponent LoadMessage { get; }
        public Func<Task<IComponent>> AsyncGenerator { get; }

        internal dom.HTMLElement InnerElement;
        private bool NeedsRefresh;

        public Defer(Func<Task<IComponent>> asyncGenerator, IComponent loadMessage = null)
        {
            LoadMessage = loadMessage ?? TextBlock("loading...").XSmall();
            AsyncGenerator = asyncGenerator;
            NeedsRefresh = true;
            InnerElement = DIV(LoadMessage.Render());
        }

        public void Refresh()
        {
            NeedsRefresh = true;
            TriggerRefresh();
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
            var task = AsyncGenerator();
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
    }
}