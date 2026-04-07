import re
content = open("Tesserae/src/Extensions/IComponentExtensions.cs").read()

pattern_action = r"        public static T Tooltip<T>\(this T component, Action<TippyConfig> configAction\) where T : IComponent\s*\{\s*var config = new TippyConfig\(\);\s*configAction\(config\);\s*Tippy\.ShowFor\(component, config, out var hide\);\s*return component;\s*\}"

new_action = """        public static T Tooltip<T>(this T component, Action<TippyConfig> configAction) where T : IComponent
        {
            var config = new TippyConfig();
            configAction(config);
            return component.Tooltip(config);
        }"""

content = re.sub(pattern_action, new_action, content, flags=re.DOTALL)
open("Tesserae/src/Extensions/IComponentExtensions.cs", "w").write(content)
