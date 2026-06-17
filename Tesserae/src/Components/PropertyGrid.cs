using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// Marks a property with a friendly label for the <see cref="PropertyGrid{T}"/>. Attribute support is
    /// best-effort (it depends on reflection metadata being available); the fluent config API is always honored.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    [H5.Name("tss.PropertyGridLabelAttribute")]
    public sealed class PropertyGridLabelAttribute : Attribute
    {
        /// <summary>The display label.</summary>
        public string Label { get; }
        /// <summary>Initializes a new instance of the <see cref="PropertyGridLabelAttribute"/> class.</summary>
        public PropertyGridLabelAttribute(string label) { Label = label; }
    }

    /// <summary>Marks a property with a helper description for the <see cref="PropertyGrid{T}"/>.</summary>
    [AttributeUsage(AttributeTargets.Property)]
    [H5.Name("tss.PropertyGridDescriptionAttribute")]
    public sealed class PropertyGridDescriptionAttribute : Attribute
    {
        /// <summary>The description text.</summary>
        public string Description { get; }
        /// <summary>Initializes a new instance of the <see cref="PropertyGridDescriptionAttribute"/> class.</summary>
        public PropertyGridDescriptionAttribute(string description) { Description = description; }
    }

    /// <summary>Sets the display order of a property in the <see cref="PropertyGrid{T}"/> (lower comes first).</summary>
    [AttributeUsage(AttributeTargets.Property)]
    [H5.Name("tss.PropertyGridOrderAttribute")]
    public sealed class PropertyGridOrderAttribute : Attribute
    {
        /// <summary>The sort order.</summary>
        public int Order { get; }
        /// <summary>Initializes a new instance of the <see cref="PropertyGridOrderAttribute"/> class.</summary>
        public PropertyGridOrderAttribute(int order) { Order = order; }
    }

    /// <summary>Marks a property as read-only in the <see cref="PropertyGrid{T}"/>.</summary>
    [AttributeUsage(AttributeTargets.Property)]
    [H5.Name("tss.PropertyGridReadOnlyAttribute")]
    public sealed class PropertyGridReadOnlyAttribute : Attribute { }

    /// <summary>Hides a property from the <see cref="PropertyGrid{T}"/>.</summary>
    [AttributeUsage(AttributeTargets.Property)]
    [H5.Name("tss.PropertyGridIgnoreAttribute")]
    public sealed class PropertyGridIgnoreAttribute : Attribute { }

    /// <summary>Renders a string property as a multi-line <see cref="TextArea"/> in the <see cref="PropertyGrid{T}"/>.</summary>
    [AttributeUsage(AttributeTargets.Property)]
    [H5.Name("tss.PropertyGridMultilineAttribute")]
    public sealed class PropertyGridMultilineAttribute : Attribute { }

    /// <summary>Per-property display / behaviour options resolved from attributes and the fluent config API.</summary>
    [H5.Name("tss.PropertyGridFieldOptions")]
    public sealed class PropertyGridFieldOptions
    {
        /// <summary>Overrides the field label.</summary>
        public string Label;
        /// <summary>A helper description shown beneath the editor.</summary>
        public string Description;
        /// <summary>Explicit display order (lower comes first).</summary>
        public int? Order;
        /// <summary>Whether the field is read-only.</summary>
        public bool ReadOnly;
        /// <summary>Whether the field is hidden.</summary>
        public bool Ignore;
        /// <summary>Whether a string field is rendered multi-line.</summary>
        public bool Multiline;
        /// <summary>A validation function returning an error message (or null/empty when valid) for the current value.</summary>
        public Func<object, string> Validate;
    }

    /// <summary>
    /// A metadata-driven property editor. Given a typed object it reflects over its public properties and
    /// auto-generates a two-way-bound editing form, mapping each property type to an existing Tesserae input
    /// (string → TextBox/TextArea, numbers → NumberPicker, bool → Toggle, enum → Dropdown, DateTime →
    /// DateTimePicker, Color → ColorPicker, nested objects → a recursive grouped <see cref="Expander"/> section).
    /// Edits flow straight back onto the bound object and are surfaced via <see cref="AsObservable"/> /
    /// <see cref="OnChange"/>. Per-property label/description/order/read-only overrides come from attributes or the
    /// fluent config API, and validation integrates with the existing <see cref="Validator"/>.
    /// </summary>
    [H5.Name("tss.PropertyGrid")]
    public sealed class PropertyGrid<T> : IComponent
    {
        private readonly T                       _instance;
        private readonly Stack                   _stack;
        private readonly SettableObservable<T>   _observable;
        private readonly Dictionary<string, PropertyGridFieldOptions> _config = new Dictionary<string, PropertyGridFieldOptions>();
        private          Validator               _validator;
        private          Action<T>               _onChange;
        private          bool                    _built;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyGrid{T}"/> class bound to <paramref name="instance"/>.
        /// </summary>
        public PropertyGrid(T instance)
        {
            _instance   = instance;
            _observable = new SettableObservable<T>(instance);
            _stack      = VStack().WS().Class("tss-propertygrid");
        }

        /// <summary>Returns an observable that fires (with the bound instance) whenever any field is edited.</summary>
        public IObservable<T> AsObservable() => _observable;

        /// <summary>Registers a callback invoked (with the bound instance) whenever any field is edited.</summary>
        public PropertyGrid<T> OnChange(Action<T> onChange) { _onChange += onChange; return this; }

        /// <summary>Wires the grid's validatable editors to the supplied <see cref="Validator"/>.</summary>
        public PropertyGrid<T> WithValidator(Validator validator) { _validator = validator; return this; }

        private PropertyGridFieldOptions Options(string propertyName)
        {
            if (!_config.TryGetValue(propertyName, out var opts))
            {
                opts = new PropertyGridFieldOptions();
                _config[propertyName] = opts;
            }
            return opts;
        }

        /// <summary>Overrides the label shown for a property.</summary>
        public PropertyGrid<T> Label(string propertyName, string label) { Options(propertyName).Label = label; return this; }

        /// <summary>Sets a helper description shown beneath a property's editor.</summary>
        public PropertyGrid<T> Description(string propertyName, string description) { Options(propertyName).Description = description; return this; }

        /// <summary>Sets the display order of a property (lower comes first).</summary>
        public PropertyGrid<T> Order(string propertyName, int order) { Options(propertyName).Order = order; return this; }

        /// <summary>Marks one or more properties as read-only.</summary>
        public PropertyGrid<T> ReadOnly(params string[] propertyNames) { foreach (var p in propertyNames) Options(p).ReadOnly = true; return this; }

        /// <summary>Hides one or more properties.</summary>
        public PropertyGrid<T> Ignore(params string[] propertyNames) { foreach (var p in propertyNames) Options(p).Ignore = true; return this; }

        /// <summary>Renders a string property as a multi-line text area.</summary>
        public PropertyGrid<T> Multiline(string propertyName) { Options(propertyName).Multiline = true; return this; }

        /// <summary>
        /// Adds a validation rule for a property. The function receives the property's current value and returns an
        /// error message (or null/empty when valid). Integrates with the configured <see cref="Validator"/>.
        /// </summary>
        public PropertyGrid<T> Validate(string propertyName, Func<object, string> validate) { Options(propertyName).Validate = validate; return this; }

        private void RaiseChanged()
        {
            _observable.Update(_ => { });
            _onChange?.Invoke(_instance);
        }

        private void EnsureBuilt()
        {
            if (_built) return;
            _built = true;
            BuildObject(_instance, typeof(T), _stack);
        }

        private void BuildObject(object instance, Type type, Stack target)
        {
            if (instance == null) return;

            var fields = new List<(PropertyInfo prop, PropertyGridFieldOptions opts, int order)>();
            var props  = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            int index  = 0;

            foreach (var prop in props)
            {
                if (!prop.CanRead) continue;
                if (prop.GetIndexParameters().Length > 0) continue;

                var opts = ResolveOptions(prop);
                if (opts.Ignore) continue;
                if (!prop.CanWrite) opts.ReadOnly = true;

                fields.Add((prop, opts, opts.Order ?? (1000 + index)));
                index++;
            }

            foreach (var f in fields.OrderBy(f => f.order))
            {
                var row = BuildField(instance, f.prop, f.opts);
                if (row != null) target.Add(row);
            }
        }

        private PropertyGridFieldOptions ResolveOptions(PropertyInfo prop)
        {
            // Start from any fluent config, then layer best-effort attribute values where the config did not specify them.
            var opts = _config.TryGetValue(prop.Name, out var cfg) ? cfg : new PropertyGridFieldOptions();

            try
            {
                foreach (var attr in prop.GetCustomAttributes(true))
                {
                    if (attr is PropertyGridLabelAttribute la && opts.Label == null)             opts.Label = la.Label;
                    else if (attr is PropertyGridDescriptionAttribute da && opts.Description == null) opts.Description = da.Description;
                    else if (attr is PropertyGridOrderAttribute oa && !opts.Order.HasValue)       opts.Order = oa.Order;
                    else if (attr is PropertyGridReadOnlyAttribute)                               opts.ReadOnly = true;
                    else if (attr is PropertyGridIgnoreAttribute)                                 opts.Ignore = true;
                    else if (attr is PropertyGridMultilineAttribute)                              opts.Multiline = true;
                }
            }
            catch
            {
                // Reflection metadata for custom attributes may be unavailable in some builds — config still applies.
            }

            return opts;
        }

        private IComponent BuildField(object instance, PropertyInfo prop, PropertyGridFieldOptions opts)
        {
            var labelText    = opts.Label ?? Prettify(prop.Name);
            var propType     = prop.PropertyType;
            var underlying   = Nullable.GetUnderlyingType(propType);
            var effective    = underlying ?? propType;
            var currentValue = SafeGet(prop, instance);

            // Nested object -> recursive grouped section.
            if (IsNestedObject(effective))
            {
                if (currentValue == null)
                {
                    return WithDescription(UI.Label(labelText).SetContent(TextBlock("(not set)").Secondary()), opts.Description);
                }

                var nested = VStack().WS();
                BuildObject(currentValue, effective, nested);
                return Expander(labelText, nested).WS();
            }

            IComponent editor;

            if (effective == typeof(string))
            {
                editor = BuildStringEditor(instance, prop, opts);
            }
            else if (effective == typeof(bool))
            {
                editor = BuildBoolEditor(instance, prop, opts);
            }
            else if (effective.IsEnum)
            {
                editor = BuildEnumEditor(instance, prop, effective, opts, currentValue);
            }
            else if (effective == typeof(Color))
            {
                editor = BuildColorEditor(instance, prop, opts, currentValue);
            }
            else if (effective == typeof(DateTime))
            {
                editor = BuildDateEditor(instance, prop, opts, currentValue);
            }
            else if (IsInteger(effective) || IsFloating(effective))
            {
                editor = BuildNumberEditor(instance, prop, effective, opts, currentValue);
            }
            else
            {
                // Fallback: read-only string projection of the value.
                editor = TextBox(currentValue?.ToString() ?? "").ReadOnly().WS();
            }

            return WithDescription(UI.Label(labelText).SetContent(editor), opts.Description);
        }

        private IComponent WithDescription(IComponent field, string description)
        {
            if (string.IsNullOrEmpty(description)) return field;
            return VStack().WS().Children(field, TextBlock(description).Tiny().Secondary().PB(4));
        }

        private IComponent BuildStringEditor(object instance, PropertyInfo prop, PropertyGridFieldOptions opts)
        {
            var value = SafeGet(prop, instance) as string ?? "";

            if (opts.Multiline)
            {
                var ta = TextArea(value).WS();
                if (opts.ReadOnly) { ta.IsReadOnly = true; }
                else
                {
                    ta.AsObservable().Subscribe(v => { SafeSet(prop, instance, v); RaiseChanged(); }, fireImmediately: false);
                    ApplyValidation(ta, opts, () => SafeGet(prop, instance));
                }
                return ta;
            }

            var tb = TextBox(value).WS();
            if (opts.ReadOnly) { tb.IsReadOnly = true; }
            else
            {
                tb.AsObservable().Subscribe(v => { SafeSet(prop, instance, v); RaiseChanged(); }, fireImmediately: false);
                ApplyValidation(tb, opts, () => SafeGet(prop, instance));
            }
            return tb;
        }

        private IComponent BuildBoolEditor(object instance, PropertyInfo prop, PropertyGridFieldOptions opts)
        {
            var value  = SafeGet(prop, instance);
            var toggle = Toggle().Checked(value is bool b && b);

            if (opts.ReadOnly)
            {
                toggle.Disabled();
            }
            else
            {
                toggle.AsObservable().Subscribe(v => { SafeSet(prop, instance, v); RaiseChanged(); }, fireImmediately: false);
            }
            return toggle;
        }

        private IComponent BuildEnumEditor(object instance, PropertyInfo prop, Type enumType, PropertyGridFieldOptions opts, object currentValue)
        {
            var dropdown = Dropdown().Single().WS();
            var values   = Enum.GetValues(enumType);
            var items    = new List<Dropdown.Item>();

            for (int i = 0; i < values.Length; i++)
            {
                var ev   = values.GetValue(i);
                var item = DropdownItem(ev.ToString()).SetData(ev).SelectedIf(currentValue != null && currentValue.Equals(ev));
                items.Add(item);
            }

            dropdown.Items(items.ToArray());

            if (opts.ReadOnly)
            {
                dropdown.Disabled();
            }
            else
            {
                dropdown.Attach(dd =>
                {
                    var sel = dd.SelectedItems.FirstOrDefault();
                    if (sel != null)
                    {
                        SafeSet(prop, instance, sel.GetDataAs<object>());
                        RaiseChanged();
                    }
                });
                ApplyValidation(dropdown, opts, () => SafeGet(prop, instance));
            }
            return dropdown;
        }

        private IComponent BuildColorEditor(object instance, PropertyInfo prop, PropertyGridFieldOptions opts, object currentValue)
        {
            var picker = ColorPicker(currentValue as Color ?? Color.FromArgb(0, 0, 0));

            if (opts.ReadOnly)
            {
                picker.Render().setAttribute("disabled", "");
            }
            else
            {
                picker.OnChange((s, e) => { SafeSet(prop, instance, s.Color); RaiseChanged(); });
            }
            return picker;
        }

        private IComponent BuildDateEditor(object instance, PropertyInfo prop, PropertyGridFieldOptions opts, object currentValue)
        {
            DateTime? initial = currentValue is DateTime dt ? dt : (DateTime?)null;
            var picker        = DateTimePicker(initial).WS();

            if (opts.ReadOnly)
            {
                picker.Render().setAttribute("readonly", "");
            }
            else
            {
                ((IObservableComponent<DateTime?>)picker).AsObservable().Subscribe(v =>
                {
                    if (v.HasValue) { SafeSet(prop, instance, v.Value); RaiseChanged(); }
                }, fireImmediately: false);
            }
            return picker;
        }

        private IComponent BuildNumberEditor(object instance, PropertyInfo prop, Type effective, PropertyGridFieldOptions opts, object currentValue)
        {
            if (IsInteger(effective))
            {
                var initial = 0;
                try { initial = Convert.ToInt32(currentValue); } catch { }

                var np = NumberPicker(initial).WS();
                if (opts.ReadOnly) { np.Render().setAttribute("readonly", ""); }
                else
                {
                    ((IObservableComponent<int>)np).AsObservable().Subscribe(v => { SafeSet(prop, instance, CoerceNumber(effective, v)); RaiseChanged(); }, fireImmediately: false);
                    ApplyValidation(np, opts, () => SafeGet(prop, instance));
                }
                return np;
            }

            // Floating point -> numeric TextBox (NumberPicker is integer-only).
            var text = currentValue == null ? "" : Convert.ToDouble(currentValue).ToString();
            var tb   = TextBox(text).WS();
            tb.Render().setAttribute("type", "number");
            tb.Render().setAttribute("step", "any");

            if (opts.ReadOnly) { tb.IsReadOnly = true; }
            else
            {
                tb.AsObservable().Subscribe(v =>
                {
                    if (double.TryParse(v, out var d)) { SafeSet(prop, instance, CoerceNumber(effective, d)); RaiseChanged(); }
                }, fireImmediately: false);
                ApplyValidation(tb, opts, () => SafeGet(prop, instance));
            }
            return tb;
        }

        private void ApplyValidation<TEditor>(TEditor editor, PropertyGridFieldOptions opts, Func<object> currentValue) where TEditor : ICanValidate<TEditor>
        {
            if (opts.Validate == null) return;
            editor.Validation(_ => opts.Validate(currentValue()), _validator);
        }

        private static object CoerceNumber(Type effective, double value)
        {
            if (effective == typeof(int))     return (int)value;
            if (effective == typeof(long))    return (long)value;
            if (effective == typeof(short))   return (short)value;
            if (effective == typeof(byte))    return (byte)value;
            if (effective == typeof(float))   return (float)value;
            if (effective == typeof(decimal)) return (decimal)value;
            return value; // double and anything else
        }

        private static object CoerceNumber(Type effective, int value) => CoerceNumber(effective, (double)value);

        private static bool IsInteger(Type t) =>
            t == typeof(int) || t == typeof(long) || t == typeof(short) || t == typeof(byte) ||
            t == typeof(uint) || t == typeof(ulong) || t == typeof(ushort) || t == typeof(sbyte);

        private static bool IsFloating(Type t) => t == typeof(double) || t == typeof(float) || t == typeof(decimal);

        private static bool IsNestedObject(Type t)
        {
            if (t == typeof(string) || t == typeof(Color) || t == typeof(DateTime)) return false;
            if (t.IsEnum || t.IsPrimitive || IsFloating(t)) return false;
            if (typeof(System.Collections.IEnumerable).IsAssignableFrom(t)) return false;
            return t.IsClass || (t.IsValueType && !t.IsPrimitive);
        }

        private object SafeGet(PropertyInfo prop, object instance)
        {
            try { return prop.GetValue(instance); }
            catch { return null; }
        }

        private void SafeSet(PropertyInfo prop, object instance, object value)
        {
            try { if (prop.CanWrite) prop.SetValue(instance, value); }
            catch { }
        }

        private static string Prettify(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;

            var sb = new StringBuilder();
            for (int i = 0; i < name.Length; i++)
            {
                var c = name[i];
                if (i > 0 && char.IsUpper(c) && !char.IsUpper(name[i - 1])) sb.Append(' ');
                sb.Append(i == 0 ? char.ToUpper(c) : c);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public HTMLElement Render()
        {
            EnsureBuilt();
            return _stack.Render();
        }
    }
}
