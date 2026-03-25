import re

content = open("Tesserae/src/Extensions/IComponentExtensions.cs").read()

pattern = r"        public static T Tooltip<T>\(this T component, TippyConfig config\) where T : IComponent\s*\{\s*Tippy\.ShowFor\(component, config, out var hide\);\s*return component;\s*\}"

new_method = """        public static T Tooltip<T>(this T component, TippyConfig config) where T : IComponent
        {
            Tippy.ShowFor(component, config, out var hide);
            return component;
        }

        public static T Tooltip<T>(this T component, Action<TippyConfig> configAction) where T : IComponent
        {
            var config = new TippyConfig();
            configAction(config);
            Tippy.ShowFor(component, config, out var hide);
            return component;
        }"""

content = re.sub(pattern, new_method, content, flags=re.DOTALL)
open("Tesserae/src/Extensions/IComponentExtensions.cs", "w").write(content)
