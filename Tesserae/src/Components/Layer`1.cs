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
        protected IComponent     _content;
        protected HTMLDivElement _contentHtml;
        protected HTMLElement    _renderedContent;
        private   LayerHost      _host;
        private   bool           _isVisible;
        private   bool           _isTransparent;

        /// <summary>
        /// Initializes a new instance of the <see cref="Layer{T}"/> class.
        /// </summary>
        protected Layer() => InnerElement = Div(_("tss-layer-base"));

        private Action<MouseEvent> _onLayerClick;

        /// <summary>
        /// Gets or sets the host that will contain this layer. If null, the layer will be hosted in the document body.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the content to be displayed in the layer.
        /// </summary>
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

        /// <summary>
        /// Gets a value indicating whether this layer is currently the topmost layer.
        /// </summary>
        public bool IsTopmost => int.Parse(_renderedContent.style.zIndex) == Layers.CurrentZIndex();

        /// <summary>
        /// Gets or sets a value indicating whether the layer is visible.
        /// </summary>
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (value) Show();
                else Hide();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the layer background should be transparent.
        /// </summary>
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

        /// <summary>
        /// Gets or sets a value indicating whether the layer should animate when it is shown.
        /// </summary>
        public bool AnimateOnShow { get; set; } = true;

        /// <summary>
        /// Renders the component.
        /// </summary>
        /// <returns>The rendered HTML element.</returns>
        public override HTMLElement Render() => InnerElement;

        /// <summary>
        /// Shows the layer.
        /// </summary>
        /// <returns>The component itself, for chaining.</returns>
        public virtual T Show()
        {
            if (_content is object || _contentHtml is object)
            {
                if (_host is null)
                {
                    if (_renderedContent is object && _renderedContent.IsMounted())
                    {
                        _renderedContent.style.zIndex = Layers.PushLayer(_renderedContent);
                        window.requestAnimationFrame((_) => _renderedContent?.classList.add("tss-show"));
                    }
                    else
                    {
                        var oldLayer = _renderedContent; //Remove any previous host

                        _renderedContent              = Div(_("tss-layer tss-fade"), BuildRenderedContent());
                        if (!AnimateOnShow)
                        {
                            _renderedContent.classList.add("tss-fade-instant");
                        }
                        _renderedContent.style.zIndex = Layers.PushLayer(_renderedContent);
                        document.body.appendChild(_renderedContent);
                        window.requestAnimationFrame((_) => _renderedContent?.classList.add("tss-show"));

                        oldLayer?.remove();
                    }
                }
                else
                {
                    _renderedContent = BuildRenderedContent();
                    _host.InnerElement.appendChild(_renderedContent);
                }

                _isVisible = true;

                if (!_contentHtml.classList.contains("tss-toast"))
                {
                    Tippy.HideAll();
                }
            }
            return (T)this;
        }

        /// <summary>
        /// Hides the layer.
        /// </summary>
        /// <param name="onHidden">An optional action to execute when the layer has been hidden.</param>
        public virtual void Hide(Action onHidden = null)
        {
            if (_renderedContent is object)
            {
                if (_host == null)
                {
                    _renderedContent.classList.remove("tss-show");
                    var tr = _renderedContent;

                    window.setTimeout((_) =>
                    {
                        document.body.removeChild(tr);
                        onHidden?.Invoke();
                    }, 150);
                }
                else
                {
                    _host.InnerElement.removeChild(_renderedContent);
                }
                _renderedContent = null;
                _isVisible       = false;
            }
        }

        /// <summary>
        /// Sets the action to be executed when the layer background is clicked.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        public void OnBackgroundClick(Action<MouseEvent> action)
        {
            _onLayerClick = action;
        }

        /// <summary>
        /// Builds the HTML element that represents the content of the layer.
        /// </summary>
        /// <returns>The rendered content element.</returns>
        protected virtual HTMLElement BuildRenderedContent()
        {
            if (_contentHtml is object) return _contentHtml;

            var div = Div(_("tss-layer-content"), _content.Render());

            if (_isTransparent)
            {
                div.classList.add("tss-layer-content-transparent");
            }

            if (_onLayerClick is object)
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