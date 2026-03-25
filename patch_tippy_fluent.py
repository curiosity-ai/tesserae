import re

content = open("Tesserae/src/Helpers/Tippy.cs").read()

config_class_pattern = r"    public class TippyConfig\s*\{.*?\s*public TippyConfig\(\) \{\}\s*\}"

new_config_class = """    public class TippyConfig
    {
        public IComponent Header { get; set; }
        public IComponent Content { get; set; }
        public IComponent Footer { get; set; }

        public TooltipAnimation Animation { get; set; } = TooltipAnimation.None;
        public TooltipPlacement Placement { get; set; } = TooltipPlacement.Top;
        public int DelayShow { get; set; } = 0;
        public int DelayHide { get; set; } = 0;
        public int MaxWidth { get; set; } = 350;
        public bool Arrow { get; set; } = false;
        public string Theme { get; set; } = null;
        public bool Interactive { get; set; } = true;
        public bool HideOnClick { get; set; } = true;
        public Action OnHiddenCallback { get; set; } = null;
        public Func<bool> OnHide { get; set; } = null;
        public Action<TippyInstance, MouseEvent> OnClickOutside { get; set; } = null;
        public bool AppendToBody { get; set; } = true;
        public bool FollowCursor { get; set; } = false;

        public TippyConfig() {}

        public TippyConfig SetHeader(IComponent header) { Header = header; return this; }
        public TippyConfig SetContent(IComponent content) { Content = content; return this; }
        public TippyConfig SetFooter(IComponent footer) { Footer = footer; return this; }
        public TippyConfig SetAnimation(TooltipAnimation animation) { Animation = animation; return this; }
        public TippyConfig SetPlacement(TooltipPlacement placement) { Placement = placement; return this; }
        public TippyConfig SetDelay(int show, int hide) { DelayShow = show; DelayHide = hide; return this; }
        public TippyConfig SetMaxWidth(int maxWidth) { MaxWidth = maxWidth; return this; }
        public TippyConfig SetArrow(bool arrow) { Arrow = arrow; return this; }
        public TippyConfig SetTheme(string theme) { Theme = theme; return this; }
        public TippyConfig SetInteractive(bool interactive) { Interactive = interactive; return this; }
        public TippyConfig SetHideOnClick(bool hideOnClick) { HideOnClick = hideOnClick; return this; }
        public TippyConfig SetOnHiddenCallback(Action onHiddenCallback) { OnHiddenCallback = onHiddenCallback; return this; }
        public TippyConfig SetOnHide(Func<bool> onHide) { OnHide = onHide; return this; }
        public TippyConfig SetOnClickOutside(Action<TippyInstance, MouseEvent> onClickOutside) { OnClickOutside = onClickOutside; return this; }
        public TippyConfig SetAppendToBody(bool appendToBody) { AppendToBody = appendToBody; return this; }
        public TippyConfig SetFollowCursor(bool followCursor) { FollowCursor = followCursor; return this; }
    }"""

content = re.sub(config_class_pattern, new_config_class, content, flags=re.DOTALL)
open("Tesserae/src/Helpers/Tippy.cs", "w").write(content)
