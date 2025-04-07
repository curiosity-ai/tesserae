using H5;
using System;
using System.Collections.Generic;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [Name("tss.Toast")]
    public class Toast : Layer<Toast>
    {
        public static Position DefaultPosition { get; set; } = Position.TopRight;

        private Type     _type = Type.Information;
        private Position _pos { get; set; } = DefaultPosition;
        private bool     _banner = false;
        private bool     _showHideButton;

        private bool _dismissOnClick = true;
        private bool _overwrite      = true;

        private Position _simPos
        {
            get
            {
                if (_banner) return Position.TopCenter; //All banners count towards the same "equivalent position"

                switch (_pos)
                {
                    case Position.TopRight:     return Position.TopRight;
                    case Position.TopLeft:      return Position.TopLeft;
                    case Position.BottomRight:  return Position.BottomRight;
                    case Position.BottomLeft:   return Position.BottomLeft;
                    case Position.BottomFull:   return Position.BottomCenter;
                    case Position.BottomCenter: return Position.BottomCenter;
                    case Position.TopFull:      return Position.TopCenter;
                    case Position.TopCenter:    return Position.TopCenter;
                }
                return _pos;
            }
        }

        private                 IComponent                        _title;
        private                 IComponent                        _message;
        private                 double                            _height    = 0;
        private static readonly Dictionary<Position, List<Toast>> OpenToasts = new Dictionary<Position, List<Toast>>();


        private          int            _timeoutDuration = 5000;
        private          double         _timeoutHandle   = 0;
        private readonly HTMLDivElement _toastContainer  = Div(_("tss-toast-container"));

        public Toast TopRight()
        {
            _pos = Position.TopRight;
            return this;
        }

        public Toast TopCenter()
        {
            _pos = Position.TopCenter;
            return this;
        }

        public Toast TopLeft()
        {
            _pos = Position.TopLeft;
            return this;
        }

        public Toast BottomRight()
        {
            _pos = Position.BottomRight;
            return this;
        }

        public Toast BottomCenter()
        {
            _pos = Position.BottomCenter;
            return this;
        }

        public Toast BottomLeft()
        {
            _pos = Position.BottomLeft;
            return this;
        }

        public Toast TopFull()
        {
            _pos = Position.TopFull;
            return this;
        }

        public Toast BottomFull()
        {
            _pos = Position.BottomFull;
            return this;
        }

        public Toast Banner(bool showHideButton = true)
        {
            _banner         = true;
            _showHideButton = showHideButton;

            if (_pos != Position.TopFull && _pos != Position.BottomFull)
            {
                _pos = Position.TopFull;
            }

            return this;
        }

        public Toast Duration(TimeSpan timeSpan)
        {
            _timeoutDuration = (int)timeSpan.TotalMilliseconds;
            ResetTimeout();
            return this;
        }

        public void Success(IComponent title, IComponent message)
        {
            _type    = Type.Success;
            _title   = title;
            _message = message;
            Fire();
        }
        public void Information(IComponent title, IComponent message)
        {
            _type    = Type.Information;
            _title   = title;
            _message = message;
            Fire();
        }
        public void Warning(IComponent title, IComponent message)
        {
            _type    = Type.Warning;
            _title   = title;
            _message = message;
            Fire();
        }

        public void Error(IComponent title, IComponent message)
        {
            _type    = Type.Error;
            _title   = title;
            _message = message;
            Fire();
        }

        public void Success(IComponent     message) => Success(null, message);
        public void Information(IComponent message) => Information(null, message);
        public void Warning(IComponent     message) => Warning(null, message);
        public void Error(IComponent       message) => Error(null, message);

        public void Success(string title, string message)
        {
            _type    = Type.Success;
            _title   = string.IsNullOrEmpty(title) ? null : TextBlock(title, textSize: TextSize.Medium, textWeight: TextWeight.SemiBold);
            _message = TextBlock(message, textSize: TextSize.Small);
            Fire();
        }
        public void Information(string title, string message)
        {
            _type    = Type.Information;
            _title   = string.IsNullOrEmpty(title) ? null : TextBlock(title, textSize: TextSize.Medium, textWeight: TextWeight.SemiBold);
            _message = TextBlock(message, textSize: TextSize.Small);
            Fire();
        }
        public void Warning(string title, string message)
        {
            _type    = Type.Warning;
            _title   = string.IsNullOrEmpty(title) ? null : TextBlock(title, textSize: TextSize.Medium, textWeight: TextWeight.SemiBold);
            _message = TextBlock(message, textSize: TextSize.Small);
            Fire();
        }
        public void Error(string title, string message)
        {
            _type    = Type.Error;
            _title   = string.IsNullOrEmpty(title) ? null : TextBlock(title, textSize: TextSize.Medium, textWeight: TextWeight.SemiBold);
            _message = TextBlock(message, textSize: TextSize.Small);
            Fire();
        }

        public void Success(string     message) => Success(null, message);
        public void Information(string message) => Information(null, message);
        public void Warning(string     message) => Warning(null, message);
        public void Error(string       message) => Error(null, message);

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

        public Toast NoDismiss(bool value = true)
        {
            _dismissOnClick = !value;
            return this;
        }

        public Toast NoOverwrite(bool value = true)
        {
            _overwrite = !value;
            return this;
        }

        private void UpdateContainer()
        {
            _contentHtml.className = "tss-toast tss-toast-" + _type + " tss-toast-" + _pos;

            if (_title is object)
            {
                var newTitle = Div(_("tss-toast-title"), _title.Render());

                if (_toastContainer.children.TryGetFirst(c => c.className.Contains("tss-toast-title"), out var currentTitle))
                {
                    _toastContainer.replaceChild(newTitle, currentTitle);
                }
                else
                {
                    if (_toastContainer.children.Any())
                    {
                        _toastContainer.insertBefore(newTitle, _toastContainer.firstChild);
                    }
                    else
                    {
                        _toastContainer.appendChild(newTitle);
                    }
                }
            }
            else
            {
                if (_toastContainer.children.TryGetFirst(c => c.className.Contains("tss-toast-title"), out var currentTitle))
                {
                    _toastContainer.removeChild(currentTitle);
                }
            }

            if (_message is object)
            {
                var newMessage = Div(_("tss-toast-message"), _message.Render());

                if (_toastContainer.children.TryGetFirst(c => c.className.Contains("tss-toast-message"), out var currentMessage))
                {
                    _toastContainer.replaceChild(newMessage, currentMessage);
                }
                else
                {
                    _toastContainer.appendChild(newMessage);
                }
            }
            else
            {
                if (_toastContainer.children.TryGetFirst(c => c.className.Contains("tss-toast-message"), out var currentMessage))
                {
                    _toastContainer.removeChild(currentMessage);
                }
            }

            _toastContainer.onmouseenter = (e) =>
            {
                ClearTimeout();
            };

            _toastContainer.onmouseleave = (e) =>
            {
                ResetTimeout();
            };

            if (_dismissOnClick)
            {
                _toastContainer.onclick = (e) =>
                {
                    ClearTimeout();
                    RemoveAndHide();
                };
            }
            else
            {
                _toastContainer.onclick = null;
            }

            foreach (var kv in OpenToasts)
            {
                kv.Value.Remove(this);
            }

            if (!OpenToasts.TryGetValue(_simPos, out var list))
            {
                list                = new List<Toast>();
                OpenToasts[_simPos] = list;
            }

            if (!list.Contains(this))
            {
                list.Add(this);
            }

            RefreshPositioning();
            ResetTimeout();
        }

        private void Fire()
        {
            if (_contentHtml is object && _contentHtml.IsMounted())
            {
                UpdateContainer();
            }
            else
            {
                _contentHtml = Div(_("tss-toast tss-toast-" + _type + " tss-toast-" + _pos), _toastContainer);

                Script.Write("{0}.replaceChildren()", _toastContainer); // clear all children

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

                if (_dismissOnClick)
                {
                    _toastContainer.onclick = (e) =>
                    {
                        ClearTimeout();
                        RemoveAndHide();
                    };
                }

                _toastContainer.onmouseleave = (e) =>
                {
                    ResetTimeout();
                };

                if (!OpenToasts.TryGetValue(_simPos, out var list))
                {
                    list                = new List<Toast>();
                    OpenToasts[_simPos] = list;
                }

                var textContent = _toastContainer.textContent;

                foreach (var otherToast in list.ToArray())
                {
                    if (otherToast._toastContainer.textContent == textContent)
                    {
                        if (_overwrite)
                        {
                            otherToast.RemoveAndHide();
                        }
                        else if (_banner && otherToast._banner)
                        {
                            otherToast.RemoveAndHide();
                        }
                    }
                }

                list.Add(this);

                RefreshPositioning();

                if (_banner)
                {
                    ShowAsBanner();
                }
                else
                {
                    Show();
                }

                ResetTimeout();
            }
        }

        private void ShowAsBanner()
        {
            _renderedContent = BuildRenderedContent();
            var captured = _renderedContent;

            if (_showHideButton)
            {
                var btn = Button().SetIcon(UIcons.Cross).OnClick(() => Hide()).NoMinSize().NoPadding().Class("tss-banner-hide-button");
                _renderedContent.querySelector(".tss-toast-container").As<HTMLElement>().appendChild(btn.Render());
            }

            _renderedContent.classList.add("tss-banner");

            if (_pos == Position.BottomFull)
            {
                _renderedContent.classList.add("tss-banner-bottom");
            }

            document.body.appendChild(_renderedContent);
            var rect = _renderedContent.querySelector(".tss-toast-container").As<HTMLElement>().getBoundingClientRect().As<DOMRect>();
            var h    = rect.height + "px";
            document.body.style.setProperty("height", $"calc(100vh - {h})", "important");

            if (_pos == Position.BottomFull)
            {
                document.body.style.setProperty("margin-top", "0", "important");
            }
            else
            {
                document.body.style.setProperty("margin-top", h, "important");
            }
            document.body["tssBannerActive"] = captured;

            DomObserver.WhenRemoved(_renderedContent, () =>
            {
                if (document.body["tssBannerActive"] == captured)
                {
                    document.body.style.height       = "";
                    document.body.style.marginTop    = "";
                    document.body["tssBannerActive"] = null;
                }
            });
        }

        private static void RefreshPositioning()
        {
            foreach (var kv in OpenToasts)
            {
                double sum = 0;

                foreach (var t in kv.Value)
                {
                    t.Measure();

                    if (t._banner) continue;

                    switch (kv.Key)
                    {
                        case Position.TopRight:
                        case Position.TopCenter:
                        case Position.TopLeft:
                        case Position.TopFull:
                            t._toastContainer.style.marginTop    = $"{sum + 16}px";
                            t._toastContainer.style.marginBottom = null;
                            break;
                        case Position.BottomRight:
                        case Position.BottomCenter:
                        case Position.BottomLeft:
                        case Position.BottomFull:
                            t._toastContainer.style.marginTop    = null;
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
            if (_timeoutHandle != 0)
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

        public void Remove()
        {
            ClearTimeout();
            RemoveAndHide();
        }

        private void RemoveAndHide()
        {
            OpenToasts[_simPos].Remove(this);

            switch (_simPos)
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
                    _toastContainer.style.marginBottom = "0px";
                    break;
            }
            Hide();
            RefreshPositioning();
        }

        [Enum(Emit.StringName)] //Don't change the emit type
        [Name("tss.Toast.Type")]
        public enum Type
        {
            [Name("success")]     Success,
            [Name("information")] Information,
            [Name("warning")]     Warning,
            [Name("error")]       Error
        }


        [Enum(Emit.StringName)] //Don't change the emit type 
        [H5.Name("tss.Toast.Position")]
        public enum Position
        {
            [Name("topright")]     TopRight,
            [Name("topcenter")]    TopCenter,
            [Name("topleft")]      TopLeft,
            [Name("bottomright")]  BottomRight,
            [Name("bottomcenter")] BottomCenter,
            [Name("bottomleft")]   BottomLeft,
            [Name("topfull")]      TopFull,
            [Name("bottomfull")]   BottomFull
        }
    }
}