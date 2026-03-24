content = open("Tesserae/src/Extensions/IComponentExtensions.cs").read()
# Let's fix the specific lines by string replacement
content = content.replace("Action<MouseEvent> attachTooltip = null;", "Action<Event> attachTooltip = null;")
content = content.replace("attachTooltip = (MouseEvent e) =>", "attachTooltip = (Event e) =>")
content = content.replace("rendered.onmouseenter -= attachTooltip;", "rendered.onmouseenter -= (HTMLElement.onmouseenterFn)(attachTooltip);")
content = content.replace("rendered.onmouseenter += attachTooltip;", "rendered.onmouseenter += (HTMLElement.onmouseenterFn)(attachTooltip);")

open("Tesserae/src/Extensions/IComponentExtensions.cs", "w").write(content)
