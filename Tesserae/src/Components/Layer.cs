using System;
using System.Collections.Generic;
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
    public static class Layers
    {
        private static int CurrentZIndex = 1000;
        private static HashSet<HTMLElement> CurrentLayers = new HashSet<HTMLElement>();
        public static string PushLayer(HTMLElement element)
        {
            if (CurrentLayers.Add(element))
            {

                CurrentZIndex += 10;
                return CurrentZIndex.ToString();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public static string AboveCurrent()
        {
            return (CurrentZIndex + 5).ToString();
        }

        public static void PopLayer(HTMLElement element)
        {
            if (CurrentLayers.Remove(element))
            {
                CurrentZIndex -= 10;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }


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

    public static class LayerExtensions
    {
        public static T Content<T>(this T layer, IComponent content) where T : Layer
        {
            //Fix for a strange bug with Bridge, where layer.Content is not the overloaded property from the Modal class
            if(layer is Modal modal)
            {
                modal.Content = content;
            }
            else
            {
                layer.Content = content;
            }
            return layer;
        }

        public static T Visible<T>(this T layer) where T : Layer
        {
            layer.IsVisible = true;
            return layer;
        }

        public static T Host<T>(this T layer, LayerHost host) where T : Layer
        {
            layer.Host = host;
            return layer;
        }
    }
}
