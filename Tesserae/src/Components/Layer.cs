using System;
using Bridge;
using static Tesserae.UI;
using static Retyped.dom;
using System.Collections.Generic;

namespace Tesserae.Components
{
    public static class Layers
    {
        private static int CurretZIndex = 1000;
        private static HashSet<HTMLElement> CurrentLayers = new HashSet<HTMLElement>();
        public static string PushLayer(HTMLElement element)
        {
            if (CurrentLayers.Add(element))
            {

                CurretZIndex += 10;
                return CurretZIndex.ToString();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public static string AboveCurrent()
        {
            return (CurretZIndex + 5).ToString();
        }

        public static void PopLayer(HTMLElement element)
        {
            if (CurrentLayers.Remove(element))
            {
                CurretZIndex -= 10;
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
        private HTMLElement _renderedContent;

        private LayerHost _Host;

        private bool _IsVisible;
        public Layer()
        {
            InnerElement = Div(_("tss-layer-base"));
        }

        public LayerHost Host
        {
            get { return _Host; }
            set
            {
                if (value != _Host)
                {
                    if (IsVisible) Hide();
                    _Host = value;
                    if (IsVisible) Show();
                }
            }
        }

        public virtual IComponent Content
        {
            get { return _content; }
            set
            {
                if (value != _content)
                {
                    _content = value;
                    if (IsVisible)
                    {
                        Hide();
                        Show();
                    }
                }
            }
        }


        public bool IsVisible
        {
            get { return _IsVisible; }
            set
            {
                if (value != IsVisible)
                {
                    if (value) Show();
                    else Hide();
                }
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
                if (_Host == null)
                {
                    _renderedContent = Div(_("tss-layer fade"), BuildRenderedContent());
                    _renderedContent.style.zIndex = Layers.PushLayer(_renderedContent);
                    document.body.appendChild(_renderedContent);
                    window.requestAnimationFrame((_) => _renderedContent?.classList.add("show"));
                }
                else
                {
                    _renderedContent = BuildRenderedContent();
                    _Host.InnerElement.appendChild(_renderedContent);
                }

                _IsVisible = true;
            }
        }

        public virtual void Hide()
        {
            if (_renderedContent is object)
            {
                if (_Host == null)
                {
                    Layers.PopLayer(_renderedContent);
                    _renderedContent.classList.remove("show");
                    var tr = _renderedContent;
                    window.setTimeout((_) => document.body.removeChild(tr), 150);
                    
                }
                else
                {
                    _Host.InnerElement.removeChild(_renderedContent);
                }
                _renderedContent = null;
                _IsVisible = false;
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
            layer.Content = content;
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
