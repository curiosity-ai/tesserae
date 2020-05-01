using System;
using static Retyped.dom;
using static Tesserae.UI;

namespace Tesserae.Components
{
    /// <summary>
    /// A Layer is a technical component that does not have specific Design guidance.
    /// 
    /// Layers are used to render content outside of a DOM tree, at the end of the document.This allows content to escape traditional boundaries caused by "overflow: hidden" css rules and keeps it on the top without using z-index rules.This is useful for example in
    /// ContextualMenu and Tooltip scenarios, where the content should always overlay everything else.
    /// </summary>
    public class Layer : ComponentBase<Layer, HTMLDivElement>
    {
        protected IComponent _content;
        protected HTMLElement _contentHtml;
        protected HTMLElement _renderedContent;

        private LayerHost _host;

        private bool _isVisible;
        public Layer()
        {
            InnerElement = Div(_("tss-layer-base"));
        }

        public LayerHost Host
        {
            get => _host;
            set
            {
                if (IsVisible) Hide();
                _host = value;
                if (IsVisible) Show();
            }
        }

        public virtual IComponent Content
        {
            get => _content;
            set
            {
                _content = value;
                if (IsVisible)
                {
                    Hide();
                    Show();
                }
            }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (value) Show();
                else Hide();
            }
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }

        public virtual void Show()
        {
            if (_content is object || _contentHtml is object)
            {
                if (_host is null)
                {
                    _renderedContent = Div(_("tss-layer tss-fade"), BuildRenderedContent());
                    _renderedContent.style.zIndex = Layers.PushLayer(_renderedContent);
                    document.body.appendChild(_renderedContent);
                    window.requestAnimationFrame((_) => _renderedContent?.classList.add("tss-show"));
                }
                else
                {
                    _renderedContent = BuildRenderedContent();
                    _host.InnerElement.appendChild(_renderedContent);
                }

                _isVisible = true;
            }
        }

        public virtual void Hide(Action onHidden = null)
        {
            if (_renderedContent is object)
            {
                if (_host == null)
                {
                    Layers.PopLayer(_renderedContent);
                    _renderedContent.classList.remove("tss-show");
                    var tr = _renderedContent;
                    window.setTimeout((_) => { document.body.removeChild(tr); onHidden?.Invoke(); }, 150);
                }
                else
                {
                    _host.InnerElement.removeChild(_renderedContent);
                }
                _renderedContent = null;
                _isVisible = false;
            }
        }

        protected virtual HTMLElement BuildRenderedContent()
        {
            if (_contentHtml is object) return _contentHtml;
            return Div(_("tss-layer-content"), _content.Render());
        }
    }
}