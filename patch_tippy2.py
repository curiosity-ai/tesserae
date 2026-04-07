content = open("Tesserae/src/Helpers/Tippy.cs").read()

new_showfor = """
        public static void ShowFor(IComponent hostComponent, TippyConfig config, out Action hide)
        {
            var rendered = hostComponent.Render();

            if (!rendered.IsMounted())
            {
                hide = () => { };
                return; //Only show tooltips for mounted objects
            }

            var appendTo = config.AppendToBody ? document.body : GetAppendToTarget(rendered);

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

            var (element, _) = Stack.GetCorrectItemToApplyStyle(hostComponent);

            Action onHiddenInternal = () =>
            {
                if (element.HasOwnProperty("_tippy"))
                {
                    H5.Script.Write("{0}._tippy.destroy();", element);
                }
                config.OnHiddenCallback?.Invoke();
            };

            var onHide = config.OnHide ?? (() => true);
            hide = onHiddenInternal;

            //Remove previous tooltips
            if (element.HasOwnProperty("_tippy"))
            {
                H5.Script.Write("{0}._tippy.destroy();", element);
            }

            var placement = CheckDeadZone(config.Placement, appendTo);
            var onClickOutside = config.OnClickOutside ?? _doNothing;

            if (config.Animation == TooltipAnimation.None)
            {
                H5.Script.Write("tippy({0}, { content: {1}, interactive: {2}, interactiveBorder: 8, placement: {3}, appendTo: {4}, maxWidth: {5}, onHidden: {6}, delay: [{7},{8}], arrow: {9}, theme: {10}, hideOnClick: {11}, onHide: {12}, onClickOutside: {13}, followCursor: {14} });",
                                        element, renderedTooltip, config.Interactive, placement.ToString(), config.AppendToBody ? document.body.As<object>() : appendTo.As<object>(), config.MaxWidth, onHiddenInternal, config.DelayShow, config.DelayHide, config.Arrow, config.Theme, config.HideOnClick, onHide, onClickOutside, config.FollowCursor);
            }
            else
            {
                H5.Script.Write("tippy({0}, { content: {1}, interactive: {2}, interactiveBorder: 8, placement: {3}, animation: {4},  appendTo: {5}, maxWidth: {6}, onHidden: {7}, delay: [{8},{9}], arrow: {10}, theme: {11}, hideOnClick : {12}, onHide: {13}, onClickOutside: {14}, followCursor: {15} });",
                                        element, renderedTooltip, config.Interactive, placement.ToString(), config.Animation.ToString(), config.AppendToBody ? document.body.As<object>() : appendTo.As<object>(), config.MaxWidth, onHiddenInternal, config.DelayShow, config.DelayHide, config.Arrow, config.Theme, config.HideOnClick, onHide, onClickOutside, config.FollowCursor);
            }

            H5.Script.Write("{0}._tippy.show();", element);

            hostComponent.WhenRemoved(() =>
            {
                onHiddenInternal();
            });
        }
"""

content = content.replace("        private static Action<TippyInstance, MouseEvent> _doNothing = (_,__) => { };", "        private static Action<TippyInstance, MouseEvent> _doNothing = (_,__) => { };\n" + new_showfor)
open("Tesserae/src/Helpers/Tippy.cs", "w").write(content)
