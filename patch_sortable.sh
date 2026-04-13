#!/bin/bash
cat << 'PATCH_EOF' > patch.diff
--- Tesserae/src/Helpers/HTML/Sortable.cs
+++ Tesserae/src/Helpers/HTML/Sortable.cs
@@ -23,6 +23,12 @@
                 ? Script.Write<object>("new Sortable({0}, {1})", _element, _options)
                 : Script.Write<object>("new Sortable({0})",      _element);
         }
+
+        public bool Disabled
+        {
+            get => Script.Write<bool>("{0}.option('disabled')", _sortable);
+            set => Script.Write("{0}.option('disabled', {1})", _sortable, value);
+        }
     }

     [ObjectLiteral]
PATCH_EOF
patch -p0 < patch.diff
