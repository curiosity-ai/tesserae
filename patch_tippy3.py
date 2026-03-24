content = open("Tesserae/src/Helpers/Tippy.cs").read()

import re

# now refactor the old ShowFor overloads to simply map to the new one!
# wait, wait, the old ShowFor that takes an HTMLElement doesn't have a hostComponent to pass, it just takes an HTMLElement.
# But for now let's just use it inside the IComponentExtensions where it makes sense, we can leave the old ShowFor in place since we added a new overload!
# let's just make sure it's valid code
