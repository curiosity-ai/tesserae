import re

content = open("Tesserae/src/Extensions/IComponentExtensions.cs").read()

component_method_pattern = r"public static T Tooltip<T>\(this T component, IComponent tooltip, bool interactive = false, TooltipAnimation animation = TooltipAnimation\.None, TooltipPlacement placement = TooltipPlacement\.Top, int delayShow = 250, int delayHide = 0, bool appendToBody = true, bool followCursor = false, int maxWidth = 350, bool hideOnClick = true, bool arrow = false, string theme = null, IComponent parent = null\) where T : IComponent\s*\{.*?\}(?=\s*///\s*<summary>Sets the tab index for the component\.</summary>)"

new_component_method = """public static T Tooltip<T>(this T component, IComponent tooltip, bool interactive = false, TooltipAnimation animation = TooltipAnimation.None, TooltipPlacement placement = TooltipPlacement.Top, int delayShow = 250, int delayHide = 0, bool appendToBody = true, bool followCursor = false, int maxWidth = 350, bool hideOnClick = true, bool arrow = false, string theme = null, IComponent parent = null) where T : IComponent
        {
            if (tooltip is null)
                return component;

            var rendered = component.Render();
            var marker = new object();
            rendered["tooltipMarker"] = marker;

            Action<Event> attachTooltip = null;
            attachTooltip = (Event e) =>
            {
                rendered.removeEventListener("mouseenter", attachTooltip);

                if (rendered["tooltipMarker"] != marker) return;

                var config = new TippyConfig
                {
                    Content = tooltip,
                    Interactive = interactive,
                    Animation = animation,
                    Placement = placement,
                    DelayShow = delayShow,
                    DelayHide = delayHide,
                    AppendToBody = appendToBody,
                    FollowCursor = followCursor,
                    MaxWidth = maxWidth,
                    HideOnClick = hideOnClick,
                    Arrow = arrow,
                    Theme = theme
                };

                Tippy.ShowFor(component, config, out var hide);

                // Keep tooltip mounting logic alive across reconnects if a parent was provided
                if (parent is null) parent = component;

                parent.WhenRemoved(() =>
                {
                    hide();
                    if (rendered["tooltipMarker"] != marker) return;
                    rendered.addEventListener("mouseenter", attachTooltip);
                });
            };

            rendered.addEventListener("mouseenter", attachTooltip);
            return component;
        }"""

content = re.sub(component_method_pattern, new_component_method, content, flags=re.DOTALL)
open("Tesserae/src/Extensions/IComponentExtensions.cs", "w").write(content)
