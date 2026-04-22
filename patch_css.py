import re

with open("Tesserae/h5/assets/css/tss.common.css", "r") as f:
    content = f.read()

# We need to change the shadow colors in dark mode.
# Look for .tss-dark-mode { ... } and insert/modify the shadow variables.
dark_mode_pattern = re.compile(r'(\.tss-dark-mode\s*\{)(.*?)(^\})', re.MULTILINE | re.DOTALL)
match = dark_mode_pattern.search(content)

if match:
    dark_mode_content = match.group(2)

    # Check if we have shadow variables, and if so, replace them
    # Let's adjust all shadow variables for dark mode
    shadows_to_add = """
    --tss-shadow-color-from: rgba(0, 0, 0, 0.4);
    --tss-shadow-color-to: rgba(0, 0, 0, 0.3);
    --tss-shadow-sm: 0 1px 2px 0 rgba(0, 0, 0, 0.3);
    --tss-shadow-sm-hover: 0 1px 2px 0 rgba(0, 0, 0, 0.4);
    --tss-shadow: 0 1px 3px 0 rgba(0, 0, 0, 0.4), 0 1px 2px 0 rgba(0, 0, 0, 0.3);
    --tss-shadow-hover: 0 1px 3px 0 rgba(0, 0, 0, 0.5), 0 1px 2px 0 rgba(0, 0, 0, 0.4);
    --tss-shadow-md: 0 4px 6px -1px rgba(0, 0, 0, 0.4), 0 2px 4px -1px rgba(0, 0, 0, 0.3);
    --tss-shadow-md-hover: 0 4px 6px -1px rgba(0, 0, 0, 0.5), 0 2px 4px -1px rgba(0, 0, 0, 0.4);
    --tss-shadow-lg: 0 10px 15px -3px rgba(0, 0, 0, 0.4), 0 4px 6px -2px rgba(0, 0, 0, 0.3);
    --tss-shadow-lg-hover: 0 10px 15px -3px rgba(0, 0, 0, 0.5), 0 4px 6px -2px rgba(0, 0, 0, 0.4);
    --tss-shadow-xl: 0 20px 25px -5px rgba(0, 0, 0, 0.4), 0 10px 10px -5px rgba(0, 0, 0, 0.3);
    --tss-shadow-xl-hover: 0 20px 25px -5px rgba(0, 0, 0, 0.5), 0 10px 10px -5px rgba(0, 0, 0, 0.4);
"""

    # Remove existing shadow variables from dark mode if they exist, to avoid duplication
    dark_mode_content = re.sub(r'\s*--tss-shadow-color-from:.*?;', '', dark_mode_content)
    dark_mode_content = re.sub(r'\s*--tss-shadow-color-to:.*?;', '', dark_mode_content)

    new_dark_mode_content = dark_mode_content + shadows_to_add

    new_content = content[:match.start(2)] + new_dark_mode_content + content[match.end(2):]

    with open("Tesserae/h5/assets/css/tss.common.css", "w") as f:
        f.write(new_content)
    print("Patched css")
else:
    print("Could not find dark mode")
