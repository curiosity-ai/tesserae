using System;
using Bridge;
using static Tesserae.HTML.HtmlUtil;
using static Tesserae.HTML.HtmlAttributes;
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
        #region Fields

        protected IComponent _Content;
        protected HTMLElement _ContentHtml;
        private HTMLElement _RenderedContent;

        private LayerHost _Host;

        private bool _IsVisible;

        #endregion

        #region Properties
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

        //protected HTMLElement ContentHtml
        //{
        //    get { return _ContentHtml; }
        //    set
        //    {
        //        if (value != _ContentHtml)
        //        {
        //            _ContentHtml = value;
        //            if (IsVisible)
        //            {
        //                Hide();
        //                Show();
        //            }
        //        }
        //    }
        //}

        public virtual IComponent Content
        {
            get { return _Content; }
            set
            {
                if (value != _Content)
                {
                    _Content = value;
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

        #endregion

        public Layer()
        {
            InnerElement = Div(_("tss-layer-base"));
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }

        protected virtual HTMLElement BuildRenderedContent()
        {
            if (_ContentHtml is object) return _ContentHtml;
            return Div(_("tss-layer-content"), _Content.Render());
        }

        public virtual void Show()
        {
            if (_Content is object || _ContentHtml is object)
            {
                if (_Host == null)
                {
                    _RenderedContent = Div(_("tss-layer fade"), BuildRenderedContent());
                    _RenderedContent.style.zIndex = Layers.PushLayer(_RenderedContent);
                    document.body.appendChild(_RenderedContent);
                    window.requestAnimationFrame((_) => _RenderedContent?.classList.add("show"));
                }
                else
                {
                    _RenderedContent = BuildRenderedContent();
                    _Host.InnerElement.appendChild(_RenderedContent);
                }

                _IsVisible = true;
            }
        }

        public virtual void Hide()
        {
            if (_RenderedContent is object)
            {
                if (_Host == null)
                {
                    Layers.PopLayer(_RenderedContent);
                    _RenderedContent.classList.remove("show");
                    var tr = _RenderedContent;
                    window.setTimeout((_) => document.body.removeChild(tr), 150);
                    
                }
                else
                {
                    _Host.InnerElement.removeChild(_RenderedContent);
                }
                _RenderedContent = null;
                _IsVisible = false;
            }
        }
    }

    public static class LayerExtensions
    {
        //public static Layer Content(this Layer layer, HTMLElement content)
        //{
        //    layer.ContentHtml = content;
        //    return layer;
        //}
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
