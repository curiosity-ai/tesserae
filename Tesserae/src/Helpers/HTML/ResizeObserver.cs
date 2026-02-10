using System;
using System.Collections.Generic;
using H5;
using Tesserae;
using static H5.Core.dom;

namespace Tesserae
{
    [H5.Name("tss.ResizeObserver")]
    public class ResizeObserver
    {
        public  Action<Event> OnResizeElement { get; set; }
        public  Action        OnResize        { get; set; }
        private object        resizeObserver;
        public ResizeObserver()
        {
            Action<Event[]> resize = DoResize;
            resizeObserver = Script.Write<object>("new ResizeObserver(entries => {0}(entries));", resize);
        }

        public void Observe(HTMLElement element)
        {
            Script.Write("{0}.observe({1})", resizeObserver, element);
        }

        public void StopObserving(HTMLElement element)
        {
            Script.Write("{0}.unobserve(element)", resizeObserver);
        }

        public void Disconnect()
        {
            Script.Write("{0}.disconnect()", resizeObserver);
        }

        private void DoResize(Event[] entries)
        {
            if (OnResizeElement != null)
            {
                for (int i = 0; i < entries.Length; i++)
                {
                    Event entry = entries[i];
                    OnResizeElement(entry);
                }
            }

            OnResize?.Invoke();
        }

        public static float GetHeight(HTMLElement element)
        {
            var height = window.getComputedStyle(element).height;

            if (string.IsNullOrEmpty(height))
            {
                // 2019-10-04 DWR: I've seen height be returned as a blank string, which will fail at float.parse, so return zero instead
                return 0;
            }
            return float.Parse(height.Replace("px", ""));
        }

        public static float GetWidth(HTMLElement element)
        {
            var width = window.getComputedStyle(element).width;

            if (string.IsNullOrEmpty(width))
            {
                // 2019-10-04 DWR: I presume that if height can be blank (see GetHeight) then width can be too, so include the same safety check
                return 0;
            }
            return float.Parse(width.Replace("px", ""));
        }
    }
}