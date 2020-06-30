using System;
using System.Collections.Generic;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae.Components
{
    public class Toast : Layer<Toast>
    {
        public static Position DefaultPosition { get; set; } = Position.TopRight;

        private Type _type = Type.Information;
        private Position CurrentPosition { get; set; } = DefaultPosition;
        private Position SimplePosition
        {
            get
            {
                switch (CurrentPosition)
                {
                    case Position.TopRight: return Position.TopRight;
                    case Position.TopLeft: return Position.TopLeft;
                    case Position.BottomRight: return Position.BottomRight;
                    case Position.BottomLeft: return Position.BottomLeft;
                    case Position.BottomFull: return Position.BottomCenter;
                    case Position.BottomCenter: return Position.BottomCenter;
                    case Position.TopFull: return Position.TopCenter;
                    case Position.TopCenter: return Position.TopCenter;
                }
                return CurrentPosition;
            }
        }

        private IComponent _title;
        private IComponent _message;
        private double _height = 0;
        private static Dictionary<Position, List<Toast>> OpenToasts = new Dictionary<Position, List<Toast>>();


        private int _timeoutDuration = 5000;
        private double _timeoutHandle = 0;
        private HTMLDivElement _toastContainer = Div(_("tss-toast-container"));

        public Toast   TopRight       () { CurrentPosition = Position.TopRight     ; return this;}
        public Toast   TopCenter      () { CurrentPosition = Position.TopCenter    ; return this;}
        public Toast   TopLeft        () { CurrentPosition = Position.TopLeft      ; return this;}
        public Toast   BottomRight    () { CurrentPosition = Position.BottomRight  ; return this;}
        public Toast   BottomCenter   () { CurrentPosition = Position.BottomCenter ; return this;}
        public Toast   BottomLeft     () { CurrentPosition = Position.BottomLeft   ; return this;}
        public Toast   TopFull        () { CurrentPosition = Position.TopFull      ; return this;}
        public Toast   BottomFull     () { CurrentPosition = Position.BottomFull   ; return this;}

        public Toast Duration(TimeSpan timeSpan) { _timeoutDuration = (int)timeSpan.TotalSeconds; return this;  }

        public void Success(IComponent title, IComponent message)     { _type = Type.Success;     _title = title; _message = message; Fire(); }
        public void Information(IComponent title, IComponent message) { _type = Type.Information; _title = title; _message = message; Fire(); }
        public void Warning(IComponent title, IComponent message)     { _type = Type.Warning;     _title = title; _message = message; Fire(); }
        public void Error(IComponent title, IComponent message)       { _type = Type.Error;       _title = title; _message = message; Fire(); }

        public void Success(IComponent message) => Success(null, message);
        public void Information(IComponent message)  => Information(null, message);
        public void Warning(IComponent message)      => Warning(null, message);
        public void Error(IComponent message) => Error(null, message);

        public void Success(string title, string message)     { _type = Type.Success;     _title = string.IsNullOrEmpty(title) ? null : TextBlock(title).SemiBold().Medium(); _message = TextBlock(message).Small(); Fire(); }
        public void Information(string title, string message) { _type = Type.Information; _title = string.IsNullOrEmpty(title) ? null : TextBlock(title).SemiBold().Medium(); _message = TextBlock(message).Small(); Fire(); }
        public void Warning(string title, string message)     { _type = Type.Warning;     _title = string.IsNullOrEmpty(title) ? null : TextBlock(title).SemiBold().Medium(); _message = TextBlock(message).Small(); Fire(); }
        public void Error(string title, string message)       { _type = Type.Error;       _title = string.IsNullOrEmpty(title) ? null : TextBlock(title).SemiBold().Medium(); _message = TextBlock(message).Small(); Fire(); }

        public void Success(string message)     => Success(null, message);
        public void Information(string message) => Information(null, message);
        public void Warning(string message)     => Warning(null, message);
        public void Error(string message)       => Error(null, message);

        public Toast Width(UnitSize width)
        {
            _toastContainer.style.width = width.ToString();
            return this;
        }

        public Toast Height(UnitSize height)
        {
            _toastContainer.style.height = height.ToString();
            return this;
        }

        private void Fire()
        {
            _contentHtml = Div(_($"tss-toast tss-toast-{_type.ToString().ToLower()} tss-toast-{CurrentPosition.ToString().ToLower()}"), _toastContainer);

            if (_title is object)
            {
                _toastContainer.appendChild(Div(_("tss-toast-title"), _title.Render()));
            }

            if (_message is object)
            {
                _toastContainer.appendChild(Div(_("tss-toast-message"), _message.Render()));
            }

            _toastContainer.onmouseenter = (e) =>
            {
                ClearTimeout();
            };

            _toastContainer.onclick = (e) =>
            {
                ClearTimeout();
                RemoveAndHide();
            };

            _toastContainer.onmouseleave = (e) =>
            {
                ResetTimeout();
            };

            if (!OpenToasts.TryGetValue(SimplePosition, out var list))
            {
                list = new List<Toast>();
                OpenToasts[SimplePosition] = list;
            }

            list.Add(this);

            RefreshPositioning();

            Show();

            ResetTimeout();
        }

        private void RefreshPositioning()
        {
            foreach(var kv in OpenToasts)
            {
                double sum = 0;
                foreach(var t in kv.Value)
                {
                    t.Measure();

                    switch (kv.Key)
                    {
                        case Position.TopRight:
                        case Position.TopCenter:
                        case Position.TopLeft:
                        case Position.TopFull:
                            t._toastContainer.style.marginTop = $"{sum + 16}px";
                            break;
                        case Position.BottomRight:
                        case Position.BottomCenter:
                        case Position.BottomLeft:
                        case Position.BottomFull:
                            t._toastContainer.style.marginBottom = $"{sum + 16}px";
                            break;
                    }
                    
                    sum += t._height + 16;
                }
            }
        }

        private void Measure()
        {
            if (_height == 0)
            {
                var rect = (DOMRect)_toastContainer.getBoundingClientRect();
                _height = rect.height;
            }
        }

        private void ClearTimeout()
        {
            if(_timeoutHandle != 0)
            {
                window.clearTimeout(_timeoutHandle);
                _timeoutHandle = 0;
            }
        }

        private void ResetTimeout()
        {
            ClearTimeout();
            _timeoutHandle = window.setTimeout((_) => RemoveAndHide(), _timeoutDuration);
        }

        private void RemoveAndHide()
        {
            OpenToasts[SimplePosition].Remove(this);
            switch (SimplePosition)
            {
                case Position.TopRight:
                case Position.TopCenter:
                case Position.TopLeft:
                case Position.TopFull:
                    _toastContainer.style.marginTop = "0px";
                    break;
                case Position.BottomRight:
                case Position.BottomCenter:
                case Position.BottomLeft:
                case Position.BottomFull:
                    _toastContainer.style.marginBottom = "100vh";
                    break;
            }
            Hide();
            RefreshPositioning();
        }

        public enum Type
        {
            Success,
            Information,
            Warning,
            Error
        }

        public enum Position
        {
            TopRight,
            TopCenter,
            TopLeft,
            BottomRight,
            BottomCenter,
            BottomLeft,
            TopFull,
            BottomFull
        }
    }
}
