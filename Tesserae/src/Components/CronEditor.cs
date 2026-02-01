using H5;
using System;
using System.Collections.Generic;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.CronEditor")]
    public sealed class CronEditor : ComponentBase<CronEditor, HTMLDivElement>, IObservableComponent<(string cron, bool enabled)>
    {
        private string _cron;
        private bool _enabled;
        private bool _isCustom;
        private bool _isOpen;
        private bool _daysEnabled = true;
        private bool _showEnableCheckbox = true;
        private int _minuteInterval = 60;
        private SettableObservable<(string cron, bool enabled)> _observable = new SettableObservable<(string cron, bool enabled)>();

        private HTMLElement _descContainer;
        private HTMLElement _editorContainer;

        private Dropdown _frequencyDropdown;
        private Dropdown _timeDropdown;
        private Stack _daysStack;
        private List<CheckBox> _dayCheckBoxes;
        private CheckBox _enableCheckBox;

        private TextBox _customCronInput;
        private Button _switchToCustomButton;
        private Button _switchToSimpleButton;

        private ComponentEventHandler<CronEditor> _onChange;

        public (string cron, bool enabled) Value
        {
            get => (_cron, _enabled);
            set
            {
                if (_cron != value.cron || _enabled != value.enabled)
                {
                    _cron = value.cron;
                    _enabled = value.enabled;
                    _enableCheckBox.IsChecked = _enabled;
                    _observable.Value = value;
                    _onChange?.Invoke(this);
                    RenderDescription();
                    if (_isOpen)
                    {
                        UpdateEditorState();
                    }
                }
            }
        }

        public CronEditor(string initialCron = "0 12 * * *", bool initialEnabled = true)
        {
            _cron = initialCron;
            _enabled = initialEnabled;
            _observable.Value = (_cron, _enabled);
            InnerElement = Div(_("tss-cron-editor"));

            _descContainer = Div(_("tss-cron-desc"));
            _descContainer.onclick = (e) => Open();

            _editorContainer = Div(_("tss-cron-open"));
            _editorContainer.style.display = "none";

            _enableCheckBox = CheckBox("Enable").Checked(_enabled).OnChange((s, e) => EnabledChanged());

            InitializeSimpleEditor();
            InitializeCustomEditor();

            InnerElement.appendChild(_descContainer);
            InnerElement.appendChild(_editorContainer);
            RenderDescription();
        }

        public CronEditor DaysEnabled(bool enabled = true)
        {
            _daysEnabled = enabled;
            if (_daysStack != null)
            {
                if (enabled) _daysStack.Show();
                else _daysStack.Collapse();
            }
            return this;
        }

        public CronEditor ShowEnableCheckbox(bool visible)
        {
            _showEnableCheckbox = visible;
            RenderDescription();
            return this;
        }

        public CronEditor MinuteInterval(int interval)
        {
            _minuteInterval = interval;
            RefreshTimeDropdown();
            return this;
        }

        public CronEditor OnChange(ComponentEventHandler<CronEditor> onChange)
        {
            _onChange += onChange;
            return this;
        }

        public IObservable<(string cron, bool enabled)> AsObservable()
        {
            return _observable;
        }

        private void EnabledChanged()
        {
            _enabled = _enableCheckBox.IsChecked;
            _observable.Value = (_cron, _enabled);
            _onChange?.Invoke(this);
            RenderDescription();
            if (_isOpen)
            {
                UpdateEditorState();
            }
        }

        private void InitializeSimpleEditor()
        {
            _frequencyDropdown = Dropdown().Items(DropdownItem("daily").Selected(), DropdownItem("weekly"), DropdownItem("monthly")).Class("tss-cron-inline-dropdown");
            _frequencyDropdown.OnChange((s, e) => UpdateCronFromSimple());

            _timeDropdown = Dropdown().Class("tss-cron-inline-dropdown");
            RefreshTimeDropdown();
            _timeDropdown.OnChange((s, e) => UpdateCronFromSimple());

            _dayCheckBoxes = new List<CheckBox>();
            var days = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

            for (int i = 0; i < 7; i++)
            {
                var cb = CheckBox(days[i]);
                cb.OnChange((s, e) => UpdateCronFromSimple());
                _dayCheckBoxes.Add(cb);
            }

            // Fix: Pass IComponent objects to Children, not Render()ed elements
            _daysStack = HStack().Children(_dayCheckBoxes.Cast<IComponent>().ToArray()).Class("tss-cron-days");
            if (!_daysEnabled) _daysStack.Collapse();

            _switchToCustomButton = Button().SetIcon(UIcons.Pencil).Class("tss-cron-btn-icon").NoBorder().NoBackground()
                                    .OnClick((s, e) => SwitchToCustom());
        }

        private void RefreshTimeDropdown()
        {
            var items = new List<Dropdown.Item>();
            for (int h = 0; h < 24; h++)
            {
                for (int m = 0; m < 60; m += _minuteInterval)
                {
                    string timeStr = DateTime.Today.AddHours(h).AddMinutes(m).ToString("hh:mm tt");
                    string val = $"{m} {h}";
                    items.Add(DropdownItem(timeStr).SetData(val));
                }
            }
            _timeDropdown.Items(items.ToArray());
        }

        private void InitializeCustomEditor()
        {
            _customCronInput = TextBox().Class("tss-cron-custom-input");
            _customCronInput.OnChange((s, e) =>
            {
                _cron = _customCronInput.Text;
                _onChange?.Invoke(this);
                RenderDescription();
            });

            _switchToSimpleButton = Button("Back to simple schedule editor").SetIcon(UIcons.AngleRight).OnClick((s, e) => SwitchToSimple());
        }

        private void Open()
        {
            if (_isOpen) return;
            _isOpen = true;
            _descContainer.style.display = "none";
            _editorContainer.style.display = "flex";

            UpdateEditorState();
        }

        private void Close()
        {
            if (!_isOpen) return;
            _isOpen = false;
            _descContainer.style.display = "";
            _editorContainer.style.display = "none";
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }

        private void UpdateEditorState()
        {
            _editorContainer.innerHTML = "";

            var parsed = ParseCron(_cron);

            if (parsed.IsValid && !_isCustom)
            {
                _frequencyDropdown.SelectedItems.FirstOrDefault()?.SelectedIf(false);

                var timeVal = $"{parsed.Minute} {parsed.Hour}";

                var items = new List<Dropdown.Item>();
                int closestDiff = int.MaxValue;
                string bestVal = timeVal;

                int targetMins = parsed.Hour * 60 + parsed.Minute;

                for (int h = 0; h < 24; h++)
                {
                    for (int m = 0; m < 60; m += _minuteInterval)
                    {
                        string timeStr = DateTime.Today.AddHours(h).AddMinutes(m).ToString("hh:mm tt");
                        string val = $"{m} {h}";
                        var item = DropdownItem(timeStr).SetData(val);

                        int currentMins = h * 60 + m;
                        int diff = Math.Abs(targetMins - currentMins);
                        if (diff < closestDiff)
                        {
                            closestDiff = diff;
                            bestVal = val;
                        }

                        items.Add(item);
                    }
                }

                foreach(var item in items)
                {
                    if ((string)item.Data == bestVal)
                    {
                        item.Selected();
                        break;
                    }
                }
                _timeDropdown.Items(items.ToArray());

                if (_daysEnabled)
                {
                    for (int i = 0; i < 7; i++)
                    {
                        int cronDay = (i + 1) % 7;
                        bool check = parsed.AllDays || parsed.DaysOfWeek.Contains(cronDay);
                        _dayCheckBoxes[i].IsChecked = check;
                    }
                }

                var row = Div(_("tss-cron-row"));
                row.appendChild(Span(_("tss-cron-label"), TextBlock("Scheduled ").Render()));
                row.appendChild(_frequencyDropdown.Render());
                row.appendChild(Span(_("tss-cron-label"), TextBlock(" at ").Render()));
                row.appendChild(_timeDropdown.Render());
                row.appendChild(Span(_("tss-cron-label"), TextBlock(" UTC").Render()));

                var wrappedRow = Button().ReplaceContent(HStack().AlignItemsCenter().NoDefaultMargin().Children(Icon(UIcons.AngleUp).PR(16), Icon(UIcons.Clock).PR(8), Raw(row)));
                
                wrappedRow.OnClick(() => Close());

                _editorContainer.appendChild(wrappedRow.Render());

                if (_daysEnabled)
                {
                    _editorContainer.appendChild(_daysStack.Render());
                }

                var actions = Div(_("tss-cron-actions"));
                actions.appendChild(_switchToCustomButton.Render());
                _editorContainer.appendChild(actions);
            }
            else
            {
                _isCustom = true;
                _customCronInput.Text = _cron;

                var row = Div(_("tss-cron-row"));
                row.appendChild(_customCronInput.Render());

                var actions = Div(_("tss-cron-actions"));
                actions.appendChild(_switchToSimpleButton.Render());

                _editorContainer.appendChild(row);
                _editorContainer.appendChild(actions);
            }
        }

        private void SwitchToCustom()
        {
            _isCustom = true;
            UpdateEditorState();
        }

        private void SwitchToSimple()
        {
            var minWidth = _editorContainer.getBoundingClientRect().As<DOMRect>().width;
            _cron = _customCronInput.Text;
            
            _customCronInput.MinWidth(minWidth.px());

            var parsed = ParseCron(_cron);
            if (parsed.IsValid)
            {
                _isCustom = false;
                UpdateEditorState();
            }
            else
            {
                _isCustom = false;
                if (!parsed.IsValid)
                {
                    _cron = "0 0 * * *";
                }
                UpdateEditorState();
                _observable.Value = (_cron, _enabled);
                _onChange?.Invoke(this);
                RenderDescription();
            }
        }

        private void UpdateCronFromSimple()
        {
            var timeItem = _timeDropdown.SelectedItems.FirstOrDefault();
            if (timeItem == null) return;

            var parts = ((string)timeItem.Data).Split(' ');
            int m = int.Parse(parts[0]);
            int h = int.Parse(parts[1]);

            var days = new List<string>();
            if (_daysEnabled)
            {
                if (_dayCheckBoxes.All(c => c.IsChecked))
                {
                    days.Add("*");
                }
                else
                {
                    for (int i = 0; i < 7; i++)
                    {
                        if (_dayCheckBoxes[i].IsChecked)
                        {
                             int cronDay = (i + 1) % 7;
                             days.Add(cronDay.ToString());
                        }
                    }
                }
            }
            else
            {
                days.Add("*");
            }

            string dayPart = days.Contains("*") || days.Count == 0 ? "*" : string.Join(",", days);

            _cron = $"{m} {h} * * {dayPart}";

            _observable.Value = (_cron, _enabled);
            _onChange?.Invoke(this);
            RenderDescription();
        }

        private void RenderDescription()
        {
            var parsed = ParseCron(_cron);
            
            var text = $"Scheduled at {_cron}";

            if (parsed.IsValid)
            {
                string time = DateTime.Today.AddHours(parsed.Hour).AddMinutes(parsed.Minute).ToString("hh:mm tt");
                string days = "daily";
                if (!parsed.AllDays)
                {
                    var names = new[] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
                    var selectedNames = parsed.DaysOfWeek.OrderBy(d => d).Select(d => names[d]);
                    days = "on " + string.Join(", ", selectedNames);
                }

                text  = $"Scheduled {days} at {time} UTC";
            }

            if (!_enabled)
            {
                text = "Disabled (" + text + ")";
            }

            var stack = HStack().AlignItemsCenter().NoDefaultMargin();
            if (_showEnableCheckbox)
            {
                stack.Add(_enableCheckBox);
            }

            stack.Add(Icon(UIcons.AngleDown).PR(16));
            stack.Add(Icon(UIcons.Clock).PR(8));
            stack.Add(TextBlock(text));

            var wrappedRow = Button().ReplaceContent(stack);
                
            _descContainer.RemoveChildElements();
            _descContainer.appendChild(wrappedRow.Render());
        }

        private CronStruct ParseCron(string cron)
        {
            var res = new CronStruct();
            try
            {
                var parts = cron.Trim().Split(new[]{' '}, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 5) return res;

                if (!int.TryParse(parts[0], out res.Minute)) return res;
                if (!int.TryParse(parts[1], out res.Hour)) return res;

                if (parts[2] != "*" || parts[3] != "*") return res;

                string dow = parts[4];
                res.DaysOfWeek = new List<int>();
                if (dow == "*")
                {
                    res.AllDays = true;
                }
                else
                {
                    foreach(var d in dow.Split(','))
                    {
                        if (int.TryParse(d, out int val))
                        {
                            if (val >= 0 && val <= 7)
                            {
                                if (val == 7) val = 0;
                                res.DaysOfWeek.Add(val);
                            }
                        }
                        else
                        {
                            return res;
                        }
                    }
                }

                res.IsValid = true;
                res.IsDaily = true;
            }
            catch
            {
                res.IsValid = false;
            }
            return res;
        }

        private class CronStruct
        {
            public int Minute;
            public int Hour;
            public List<int> DaysOfWeek;
            public bool AllDays;
            public bool IsDaily;
            public bool IsValid;
        }
    }
}
