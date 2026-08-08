using Transpose;
using System;
using System.Collections.Generic;
using System.Linq;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A Toast component for displaying non-blocking notifications to the user.
    /// </summary>
    [Name("tss.Toast")]
    public class Toast : Layer<Toast>
    {
        /// <summary>
        /// Gets or sets the default position for toasts.
        /// </summary>
        public static Position DefaultPosition { get; set; } = Position.TopRight;

        private Type     _type = Type.Information;
        private Position _pos { get; set; } = DefaultPosition;
        private bool     _banner = false;
        private bool     _showHideButton;

        private bool _dismissOnClick = true;
        private bool _overwrite      = false;

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

        private                 Banner                            _content;
        private                 Banner                            _hookedContent;
        private                 Action                            _hookedCallerHandler;
        private                 double                            _height    = 0;
        private static readonly Dictionary<Position, List<Toast>> OpenToasts = new Dictionary<Position, List<Toast>>();


        private          int            _timeoutDuration = 5000;
        private          double         _timeoutHandle   = 0;
        private readonly HTMLDivElement _toastContainer  = Div(Att("tss-toast-container"));

        /// <summary>
        /// Adds a CSS class to the toast container.
        /// </summary>
        /// <param name="className">The class name.</param>
        /// <returns>The current instance of the type.</returns>
        public Toast Class(string className)
        {
            _toastContainer.classList.add(className);
            return this;
        }

        /// <summary>
        /// Removes a CSS class from the toast container.
        /// </summary>
        /// <param name="className">The class name.</param>
        /// <returns>The current instance of the type.</returns>
        public Toast RemoveClass(string className)
        {
            _toastContainer.classList.remove(className);
            return this;
        }

        /// <summary>
        /// Sets the toast position to top-right.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public Toast TopRight()
        {
            _pos = Position.TopRight;
            return this;
        }

        /// <summary>
        /// Sets the toast position to top-center.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public Toast TopCenter()
        {
            _pos = Position.TopCenter;
            return this;
        }

        /// <summary>
        /// Sets the toast position to top-left.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public Toast TopLeft()
        {
            _pos = Position.TopLeft;
            return this;
        }

        /// <summary>
        /// Sets the toast position to bottom-right.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public Toast BottomRight()
        {
            _pos = Position.BottomRight;
            return this;
        }

        /// <summary>
        /// Sets the toast position to bottom-center.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public Toast BottomCenter()
        {
            _pos = Position.BottomCenter;
            return this;
        }

        /// <summary>
        /// Sets the toast position to bottom-left.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public Toast BottomLeft()
        {
            _pos = Position.BottomLeft;
            return this;
        }

        /// <summary>
        /// Sets the toast position to top-full width.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public Toast TopFull()
        {
            _pos = Position.TopFull;
            return this;
        }

        /// <summary>
        /// Sets the toast position to bottom-full width.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public Toast BottomFull()
        {
            _pos = Position.BottomFull;
            return this;
        }

        /// <summary>
        /// Displays the toast as a banner at the top or bottom of the page.
        /// </summary>
        /// <param name="showHideButton">Whether to show a button to hide the banner.</param>
        /// <returns>The current instance of the type.</returns>
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

        /// <summary>
        /// Sets the duration for which the toast is visible.
        /// </summary>
        /// <param name="timeSpan">The duration.</param>
        /// <returns>The current instance of the type.</returns>
        public Toast Duration(TimeSpan timeSpan)
        {
            _timeoutDuration = (int)timeSpan.TotalMilliseconds;
            ResetTimeout();
            return this;
        }

        /// <summary>
        /// Displays a success toast.
        /// </summary>
        /// <param name="title">The title of the toast.</param>
        /// <param name="message">The message content of the toast.</param>
        /// <returns>The current instance of the type.</returns>
        public Toast Success(IComponent title, IComponent message) => ShowAs(Type.Success, title, message);

        /// <summary>
        /// Displays an information toast.
        /// </summary>
        /// <param name="title">The title of the toast.</param>
        /// <param name="message">The message content of the toast.</param>
        /// <returns>The current instance of the type.</returns>
        public Toast Information(IComponent title, IComponent message) => ShowAs(Type.Information, title, message);

        /// <summary>
        /// Displays a warning toast.
        /// </summary>
        /// <param name="title">The title of the toast.</param>
        /// <param name="message">The message content of the toast.</param>
        /// <returns>The current instance of the type.</returns>
        public Toast Warning(IComponent title, IComponent message) => ShowAs(Type.Warning, title, message);

        /// <summary>
        /// Displays an error toast.
        /// </summary>
        /// <param name="title">The title of the toast.</param>
        /// <param name="message">The message content of the toast.</param>
        /// <returns>The current instance of the type.</returns>
        public Toast Error(IComponent title, IComponent message) => ShowAs(Type.Error, title, message);

        /// <summary>
        /// Displays a success toast with only a message.
        /// </summary>
        /// <param name="message">The message content of the toast.</param>
        /// <returns>The current instance of the type.</returns>
        public Toast Success(IComponent     message) => Success(null, message);

        /// <summary>
        /// Displays an information toast with only a message.
        /// </summary>
        /// <param name="message">The message content of the toast.</param>
        /// <returns>The current instance of the type.</returns>
        public Toast Information(IComponent message) => Information(null, message);

        /// <summary>
        /// Displays a warning toast with only a message.
        /// </summary>
        /// <param name="message">The message content of the toast.</param>
        /// <returns>The current instance of the type.</returns>
        public Toast Warning(IComponent     message) => Warning(null, message);

        /// <summary>
        /// Displays an error toast with only a message.
        /// </summary>
        /// <param name="message">The message content of the toast.</param>
        /// <returns>The current instance of the type.</returns>
        public Toast Error(IComponent       message) => Error(null, message);

        /// <summary>
        /// Displays a success toast with string content.
        /// </summary>
        /// <param name="title">The title of the toast.</param>
        /// <param name="message">The message content of the toast.</param>
        /// <returns>The current instance of the type.</returns>
        public Toast Success(string title, string message) => ShowAs(Type.Success, title, message);

        /// <summary>
        /// Displays an information toast with string content.
        /// </summary>
        /// <param name="title">The title of the toast.</param>
        /// <param name="message">The message content of the toast.</param>
        /// <returns>The current instance of the type.</returns>
        public Toast Information(string title, string message) => ShowAs(Type.Information, title, message);

        /// <summary>
        /// Displays a warning toast with string content.
        /// </summary>
        /// <param name="title">The title of the toast.</param>
        /// <param name="message">The message content of the toast.</param>
        /// <returns>The current instance of the type.</returns>
        public Toast Warning(string title, string message) => ShowAs(Type.Warning, title, message);

        /// <summary>
        /// Displays an error toast with string content.
        /// </summary>
        /// <param name="title">The title of the toast.</param>
        /// <param name="message">The message content of the toast.</param>
        /// <returns>The current instance of the type.</returns>
        public Toast Error(string title, string message) => ShowAs(Type.Error, title, message);

        /// <summary>
        /// Displays a success toast with string message.
        /// </summary>
        /// <param name="message">The message content of the toast.</param>
        /// <returns>The current instance of the type.</returns>
        public Toast Success(string     message) => Success(null, message);

        /// <summary>
        /// Displays an information toast with string message.
        /// </summary>
        /// <param name="message">The message content of the toast.</param>
        /// <returns>The current instance of the type.</returns>
        public Toast Information(string message) => Information(null, message);

        /// <summary>
        /// Displays a warning toast with string message.
        /// </summary>
        /// <param name="message">The message content of the toast.</param>
        /// <returns>The current instance of the type.</returns>
        public Toast Warning(string     message) => Warning(null, message);

        /// <summary>
        /// Displays an error toast with string message.
        /// </summary>
        /// <param name="message">The message content of the toast.</param>
        /// <returns>The current instance of the type.</returns>
        public Toast Error(string       message) => Error(null, message);

        /// <summary>
        /// Floats the given <see cref="Tesserae.Banner"/> over the page as a toast: the same strip that
        /// renders inline, positioned and timed out by the toast instead. Everything the banner carries -
        /// its tone, its icon tile, its badge, its action - comes along.
        /// <para>
        /// The banner's dismiss button is hooked to this toast's own hiding, chained after whatever handler
        /// the caller already set. A toast asked not to dismiss (<see cref="NoDismiss"/>), or a banner shown
        /// as an edge-to-edge banner with its hide button turned off, gets no dismiss button at all.
        /// </para>
        /// </summary>
        /// <param name="banner">The banner to show.</param>
        /// <returns>The current instance of the type.</returns>
        public Toast Show(Banner banner)
        {
            if (banner is null) return this;

            _type    = TypeFor(banner.CurrentStyle);
            _content = banner;

            Fire();

            return this;
        }

        /// <summary>
        /// Gets the <see cref="Tesserae.Banner"/> this toast is showing, so a caller that used one of the
        /// string helpers can still reach the strip it built - to add an action to it, say.
        /// </summary>
        public Banner Content => _content;

        private Toast ShowAs(Type type, string title, string message)
        {
            _type    = type;
            _content = BuildBanner(type).SetTitle(title).SetText(message);

            Fire();

            return this;
        }

        private Toast ShowAs(Type type, IComponent title, IComponent message)
        {
            _type    = type;
            _content = BuildBanner(type).SetTitle(title).SetText(message);

            Fire();

            return this;
        }

        private static Banner BuildBanner(Type type) => new Banner().Style(StyleFor(type));

        private static BannerStyle StyleFor(Type type)
        {
            switch (type)
            {
                case Type.Success: return BannerStyle.Success;
                case Type.Warning: return BannerStyle.Warning;
                case Type.Error:   return BannerStyle.Danger;
                default:           return BannerStyle.Primary;
            }
        }

        private static Type TypeFor(BannerStyle style)
        {
            switch (style)
            {
                case BannerStyle.Success: return Type.Success;
                case BannerStyle.Warning: return Type.Warning;
                case BannerStyle.Danger:  return Type.Error;
                default:                  return Type.Information;
            }
        }

        /// <summary>
        /// Sets the width of the toast.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <returns>The current instance of the type.</returns>
        public Toast Width(UnitSize width)
        {
            _toastContainer.style.width = width.ToString();
            return this;
        }

        /// <summary>
        /// Sets the height of the toast.
        /// </summary>
        /// <param name="height">The height.</param>
        /// <returns>The current instance of the type.</returns>
        public Toast Height(UnitSize height)
        {
            _toastContainer.style.height = height.ToString();
            return this;
        }

        /// <summary>
        /// Prevents the toast from being dismissed when clicked.
        /// </summary>
        /// <param name="value">Whether to prevent dismissal on click.</param>
        /// <returns>The current instance of the type.</returns>
        public Toast NoDismiss(bool value = true)
        {
            _dismissOnClick = !value;
            return this;
        }

        /// <summary>
        /// Prevents the toast from being overwritten by a new toast with the same content.
        /// </summary>
        /// <param name="value">Whether to prevent overwriting.</param>
        /// <returns>The current instance of the type.</returns>
        public Toast Overwrite(bool value = true)
        {
            _overwrite = value;
            return this;
        }

        private void UpdateContainer()
        {
            _contentHtml.className = "tss-toast tss-toast-" + _type + " tss-toast-" + _pos;

            RenderContent();

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
                _contentHtml = Div(Att("tss-toast tss-toast-" + _type + " tss-toast-" + _pos), _toastContainer);

                RenderContent();

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

        /// <summary>
        /// Puts the banner - the one the caller passed, or the one a string helper built - into the toast's
        /// container, and hooks its dismiss button to this toast's own hiding.
        /// </summary>
        private void RenderContent()
        {
            Script.Write("{0}.replaceChildren()", _toastContainer); // clear all children

            if (_content is null) return;

            HookDismiss();

            _toastContainer.appendChild(_content.Render());
        }

        /// <summary>
        /// A toast is dismissed by the toast, not by the strip inside it, so the banner's dismiss button
        /// hides the whole layer - after whatever the caller's own handler does. Whether there is a button
        /// at all follows the toast's settings: an edge-to-edge banner asked to show one, or an ordinary
        /// toast that can be dismissed at all.
        /// </summary>
        private void HookDismiss()
        {
            // The caller's own handler is read once per banner: re-firing a toast (a SavingToast stepping
            // from "saving" to "saved") runs this again, and wrapping the wrapper would hide twice.
            if (!ReferenceEquals(_hookedContent, _content))
            {
                _hookedContent       = _content;
                _hookedCallerHandler = _content.DismissHandler;
            }

            var wanted = _banner ? _showHideButton : _dismissOnClick;

            if (!wanted && _hookedCallerHandler is null)
            {
                _content.OnDismiss(null, hide: false);
                return;
            }

            var callerHandler = _hookedCallerHandler;

            _content.OnDismiss(() =>
            {
                callerHandler?.Invoke();
                Hide();
            }, hide: false);
        }

        private void ShowAsBanner()
        {
            _renderedContent = BuildRenderedContent();
            var captured = _renderedContent;

            _renderedContent.classList.add("tss-toast-fullwidth");

            if (_pos == Position.BottomFull)
            {
                _renderedContent.classList.add("tss-toast-fullwidth-bottom");
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

        private static double _positioningTimeout;

        private static void RefreshPositioning()
        {
            window.clearTimeout(_positioningTimeout);
            _positioningTimeout = window.setTimeout(_ =>
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
                                t._toastContainer.style.marginTop = $"{sum + 16}px";
                                t._toastContainer.style.marginBottom = null;
                                break;
                            case Position.BottomRight:
                            case Position.BottomCenter:
                            case Position.BottomLeft:
                            case Position.BottomFull:
                                t._toastContainer.style.marginTop = null;
                                t._toastContainer.style.marginBottom = $"{sum + 16}px";
                                break;
                        }

                        sum += t._height + 16;
                    }
                }
            },15);
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

        /// <summary>
        /// Hides the toast.
        /// </summary>
        /// <param name="onHidden">An optional action to perform when the toast is hidden.</param>
        public override void Hide(Action onHidden = null)
        {
            ClearTimeout();
            RemoveAndHide(onHidden);
        }

        /// <summary>
        /// Removes the toast.
        /// </summary>
        public void Remove()
        {
            Hide();
        }

        private void RemoveAndHide(Action onHidden = null)
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
            base.Hide(onHidden);
            RefreshPositioning();
        }

        /// <summary>
        /// The type of toast.
        /// </summary>
        [Enum(Emit.StringName)] //Don't change the emit type
        [Name("tss.Toast.Type")]
        public enum Type
        {
            /// <summary>A success toast.</summary>
            [Name("success")]     Success,
            /// <summary>An information toast.</summary>
            [Name("information")] Information,
            /// <summary>A warning toast.</summary>
            [Name("warning")]     Warning,
            /// <summary>An error toast.</summary>
            [Name("error")]       Error
        }

        /// <summary>
        /// The position of the toast on the screen.
        /// </summary>
        [Enum(Emit.StringName)] //Don't change the emit type 
        [Transpose.Name("tss.Toast.Position")]
        public enum Position
        {
            /// <summary>Top-right position.</summary>
            [Name("topright")]     TopRight,
            /// <summary>Top-center position.</summary>
            [Name("topcenter")]    TopCenter,
            /// <summary>Top-left position.</summary>
            [Name("topleft")]      TopLeft,
            /// <summary>Bottom-right position.</summary>
            [Name("bottomright")]  BottomRight,
            /// <summary>Bottom-center position.</summary>
            [Name("bottomcenter")] BottomCenter,
            /// <summary>Bottom-left position.</summary>
            [Name("bottomleft")]   BottomLeft,
            /// <summary>Top-full width position.</summary>
            [Name("topfull")]      TopFull,
            /// <summary>Bottom-full width position.</summary>
            [Name("bottomfull")]   BottomFull
        }
    }
}