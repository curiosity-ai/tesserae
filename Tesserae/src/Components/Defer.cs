using Retyped;
using System;
using System.Threading.Tasks;
using static Tesserae.UI;

namespace Tesserae.Components
{
    public class Defer : IComponent
    {
        public IComponent LoadMessage { get; }
        public Func<Task<IComponent>> AsyncGenerator { get; }

        internal dom.HTMLElement InnerElement;
        private bool FirstRender;

        public Defer(Func<Task<IComponent>> asyncGenerator, IComponent loadMessage = null)
        {
            LoadMessage = loadMessage ?? TextBlock("loading...").XSmall();
            AsyncGenerator = asyncGenerator;
            FirstRender = true;
            InnerElement = DIV(LoadMessage.Render());
        }

        public void Refresh()
        {
            FirstRender = false;
            TriggerRefresh();
        }

        public dom.HTMLElement Render()
        {
            if (FirstRender)
            {
                FirstRender = false;
                TriggerRefresh();
            }
            return InnerElement;
        }

        private void TriggerRefresh()
        {
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