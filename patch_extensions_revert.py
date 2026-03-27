import re

content = open("Tesserae/src/Extensions/IComponentExtensions.cs").read()

# We want to replace the Tooltip(TippyConfig) method that just calls Tippy.ShowFor
pattern = r"        public static T Tooltip<T>\(this T component, TippyConfig config\) where T : IComponent\s*\{\s*Tippy\.ShowFor\(component, config, out var hide\);\s*return component;\s*\}"

new_method = """        public static T Tooltip<T>(this T component, TippyConfig config) where T : IComponent
        {
            var rendered = component.Render();
            var marker = new object();
            rendered["tooltipMarker"] = marker;

            Action<H5.Core.dom.Event> attachTooltip = null;
            attachTooltip = (H5.Core.dom.Event e) =>
            {
                rendered.removeEventListener("mouseenter", attachTooltip);

                if (rendered["tooltipMarker"] != marker) return;

                var renderedTooltip = UI.DIV();
                renderedTooltip.classList.add("tss-tippy-container");

                if (config.Header != null)
                {
                    var headerDiv = UI.DIV(config.Header.Render());
                    headerDiv.classList.add("tss-tippy-header");
                    renderedTooltip.appendChild(headerDiv);
                }
                if (config.Content != null)
                {
                    var contentDiv = UI.DIV(config.Content.Render());
                    contentDiv.classList.add("tss-tippy-content-inner");
                    renderedTooltip.appendChild(contentDiv);
                }
                if (config.Footer != null)
                {
                    var footerDiv = UI.DIV(config.Footer.Render());
                    footerDiv.classList.add("tss-tippy-footer");
                    renderedTooltip.appendChild(footerDiv);
                }

                renderedTooltip.style.display      = "block";
                renderedTooltip.style.overflow     = "hidden";
                renderedTooltip.style.textOverflow = "ellipsis";

                if (config.AppendToBody)
                {
                    document.body.appendChild(renderedTooltip);
                }

                var (element, _) = Stack.GetCorrectItemToApplyStyle(component);

                if (element.HasOwnProperty("_tippy"))
                {
                    H5.Script.Write("{0}._tippy.destroy();", element);
                }

                Action onHiddenInternal = () =>
                {
                    if (element.HasOwnProperty("_tippy"))
                    {
                        H5.Script.Write("{0}._tippy.destroy();", element);
                    }
                    config.OnHiddenCallback?.Invoke();
                };

                var appendToTarget = config.AppendToBody ? document.body : GetAppendToTargetFrom(rendered);
                var onHide = config.OnHide ?? (() => true);
                var onClickOutside = config.OnClickOutside ?? _doNothing;

                if (config.Animation == TooltipAnimation.None)
                {
                    H5.Script.Write("tippy({0}, { content: {1}, interactive: {2}, interactiveBorder: 8, placement: {3}, appendTo: {4}, maxWidth: {5}, onHidden: {6}, delay: [{7},{8}], arrow: {9}, theme: {10}, hideOnClick: {11}, onHide: {12}, onClickOutside: {13}, followCursor: {14} });",
                                            element, renderedTooltip, config.Interactive, config.Placement.ToString(), config.AppendToBody ? document.body.As<object>() : appendToTarget.As<object>(), config.MaxWidth, onHiddenInternal, config.DelayShow, config.DelayHide, config.Arrow, config.Theme, config.HideOnClick, onHide, onClickOutside, config.FollowCursor);
                }
                else
                {
                    H5.Script.Write("tippy({0}, { content: {1}, interactive: {2}, interactiveBorder: 8, placement: {3}, animation: {4},  appendTo: {5}, maxWidth: {6}, onHidden: {7}, delay: [{8},{9}], arrow: {10}, theme: {11}, hideOnClick : {12}, onHide: {13}, onClickOutside: {14}, followCursor: {15} });",
                                            element, renderedTooltip, config.Interactive, config.Placement.ToString(), config.Animation.ToString(), config.AppendToBody ? document.body.As<object>() : appendToTarget.As<object>(), config.MaxWidth, onHiddenInternal, config.DelayShow, config.DelayHide, config.Arrow, config.Theme, config.HideOnClick, onHide, onClickOutside, config.FollowCursor);
                }

                H5.Script.Write("{0}._tippy.show();", element);

                var currentTippy = H5.Script.Write<object>("{0}._tippy", element);

                component.WhenRemoved(() =>
                {
                    if (element.HasOwnProperty("_tippy"))
                    {
                        if (currentTippy == H5.Script.Write<object>("{0}._tippy", element))
                        {
                            H5.Script.Write("{0}._tippy.destroy();", element);
                        }
                    }
                    if (rendered["tooltipMarker"] != marker) return;
                    rendered.addEventListener("mouseenter", attachTooltip);
                });
            };

            rendered.addEventListener("mouseenter", attachTooltip);
            return component;
        }

        private static H5.Core.dom.HTMLElement GetAppendToTargetFrom(H5.Core.dom.HTMLElement hostElement)
        {
            var child  = hostElement;
            var parent = child.parentElement;

            while (parent is object && !parent.classList.contains("tippy-content"))
            {
                child  = parent;
                parent = parent.parentElement;
            }

            if (parent is object && parent.classList.contains("tippy-content"))
            {
                return child;
            }

            return document.body;
        }

        private static Action<TippyInstance, H5.Core.dom.MouseEvent> _doNothing = (_,__) => { };"""

content = re.sub(pattern, new_method, content, flags=re.DOTALL)
open("Tesserae/src/Extensions/IComponentExtensions.cs", "w").write(content)
