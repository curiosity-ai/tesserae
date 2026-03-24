import re

content = open("Tesserae/src/Helpers/Tippy.cs").read()

config_class = """    public class TippyConfig
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
    }

"""

# Insert right before Tippy static class
content = content.replace('    [H5.Name("tss.tippy")]\n    public static class Tippy', config_class + '    [H5.Name("tss.tippy")]\n    public static class Tippy')

open("Tesserae/src/Helpers/Tippy.cs", "w").write(content)
