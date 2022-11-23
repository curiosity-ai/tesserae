using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// This generic version of Layer should only be used to create derived classes from (such as the ContextMenu, for example). If you require no additional functionality on top of a standard layer then use the non-generic Layer class. The reason for the two classes
    /// is to avoid confusion as this can NOT be derived from and the generic version MUST be derived from. The generic version exists to maintain the type of component in chained calls made on the ComponentBase class that they both are derived from (when the OnClick
    /// method is called on a ContextMenu then you expect a ContextMenu to be returned and not simply a Layer instance).
    /// </summary>
    [H5.Name("tss.LayerT")]
    public abstract class Layer<T> : ComponentBase<T, HTMLDivElement> where T : Layer<T>
    {
        protected IComponent _content;
        protected HTMLDivElement _contentHtml;
        protected HTMLElement _renderedContent;
        private LayerHost _host;
        private bool _isVisible;
        private bool _isTransparent;
        protected Layer() => InnerElement = Div(_("tss-layer-base"));

        private Action<MouseEvent> _onLayerClick;

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

        public bool IsTransparent
        {
            get => _isTransparent;
            set
            {
                _isTransparent = value;
                if (IsVisible)
                {
                    Hide();
                    Show();
                }
            }
        }

        public override HTMLElement Render() => InnerElement;

        public virtual T Show()
        {
            if (_content is object || _contentHtml is object)
            {
                if (_host is null)
                {
                    var oldLayer = _renderedContent; //Remove any previous host

                    _renderedContent = Div(_("tss-layer tss-fade"), BuildRenderedContent());
                    _renderedContent.style.zIndex = Layers.PushLayer(_renderedContent);
                    document.body.appendChild(_renderedContent);
                    window.requestAnimationFrame((_) => _renderedContent?.classList.add("tss-show"));

                    oldLayer?.remove();
                }
                else
                {
                    _renderedContent = BuildRenderedContent();
                    _host.InnerElement.appendChild(_renderedContent);
                }

                _isVisible = true;
                
                Tippy.HideAll();
            }
            return (T)this;
        }

        public virtual void Hide(Action onHidden = null)
        {
            if (_renderedContent is object)
            {
                if (_host == null)
                {
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

        public void OnBackgroundClick(Action<MouseEvent> action)
        {
            _onLayerClick = action;
        }

        protected virtual HTMLElement BuildRenderedContent()
        {
            if (_contentHtml is object) return _contentHtml;

            var div = Div(_("tss-layer-content"), _content.Render());
            if (_isTransparent)
            {
                div.classList.add("tss-layer-content-transparent");
            }

            if(_onLayerClick is object)
            {
                div.onclick += (e) =>
                {
                    _onLayerClick(e);
                };
            }
            return div;
        }
    }
}