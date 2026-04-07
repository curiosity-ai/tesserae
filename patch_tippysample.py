import re

with open('./Tesserae.Tests/src/Samples/Utilities/TippySample.cs', 'r') as f:
    content = f.read()

content = content.replace('using static Tesserae.UI;', 'using Tesserae;\nusing static Tesserae.UI;')

with open('./Tesserae.Tests/src/Samples/Utilities/TippySample.cs', 'w') as f:
    f.write(content)
