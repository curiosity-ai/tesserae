using Retyped;
using System;
using System.Threading.Tasks;
using static Tesserae.UI;

namespace Tesserae.Components
{
    public class Defer : IComponent
    {
        public Func<Task<IComponent>> AsyncGenerator { get; }

        private dom.HTMLElement InnerElement;
        private bool FirstRender;

        public Defer(Func<Task<IComponent>> asyncGenerator, IComponent loadMessage = null)
        {
            loadMessage = loadMessage ?? TextBlock("loading...").XSmall();
            AsyncGenerator = asyncGenerator;
            FirstRender = true;
            InnerElement = DIV(loadMessage.Render());
        }

        public dom.HTMLElement Render()
        {
            if (FirstRender)
            {
                FirstRender = false;
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
            return InnerElement;
        }
    }
}