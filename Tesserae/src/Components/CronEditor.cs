using H5;
using System;
using System.Collections.Generic;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.CronEditor")]
    public sealed class CronEditor : ComponentBase<CronEditor, HTMLDivElement>, IObservableComponent<string>
    {
        private string _cron;
        private bool _isCustom;
        private bool _isOpen;
        private bool _daysEnabled = true;
        private int _minuteInterval = 60;
        private SettableObservable<string> _observable = new SettableObservable<string>();

        private HTMLElement _descContainer;
        private HTMLElement _editorContainer;

        private Dropdown _frequencyDropdown;
        private Dropdown.Item[] _frequencyItems;
        private Dropdown _timeDropdown;
        private Dropdown _dayOfMonthDropdown;
        private Dropdown.Item[] _dayOfMonthItems;
        private Stack _domContainer;
        private Stack _daysStack;
        private List<CheckBox> _dayCheckBoxes;

        private TextBox _customCronInput;
        private Button _switchToCustomButton;
        private Button _switchToSimpleButton;

        private ComponentEventHandler<CronEditor> _onChange;

        public string Value
        {
            get => _cron;
            set
            {
                if (_cron != value)
                {
                    _cron = value;
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

        public CronEditor(string initialCron = "0 12 * * *")
        {
            _cron = initialCron;
            _observable.Value = _cron;
            InnerElement = Div(_("tss-cron-editor"));

            _descContainer = Div(_("tss-cron-desc"));
            _descContainer.onclick = (e) => Open();

            _editorContainer = Div(_("tss-cron-open"));
            _editorContainer.style.display = "none";

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

        public IObservable<string> AsObservable()
        {
            return _observable;
        }

        private void InitializeSimpleEditor()
        {
            _frequencyItems = new[] { DropdownItem("daily").Selected(), DropdownItem("weekly"), DropdownItem("monthly") };
            _frequencyDropdown = Dropdown().Items(_frequencyItems).Class("tss-cron-inline-dropdown");
            _frequencyDropdown.OnChange((s, e) => UpdateCronFromSimple());

            _timeDropdown = Dropdown().Class("tss-cron-inline-dropdown");
            RefreshTimeDropdown();
            _timeDropdown.OnChange((s, e) => UpdateCronFromSimple());

            var domItems = new List<Dropdown.Item>();
            for (int i = 1; i <= 31; i++)
            {
                domItems.Add(DropdownItem(i.ToString()).SetData(i));
            }
            _dayOfMonthItems = domItems.ToArray();
            _dayOfMonthDropdown = Dropdown().Items(_dayOfMonthItems).Class("tss-cron-inline-dropdown");
            _dayOfMonthDropdown.OnChange((s, e) => UpdateCronFromSimple());

            _domContainer = HStack().NoDefaultMargin().AlignItemsCenter().Children(TextBlock(" on day ").Class("tss-cron-label").PR(4), _dayOfMonthDropdown);

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
            _editorContainer.style.width = "";
            _descContainer.style.display = "";
            _editorContainer.style.display = "none";
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }

        private bool _showingSimple = false;

        private void UpdateEditorState()
        {
            var parsed = ParseCron(_cron);
            bool shouldShowSimple = parsed.IsValid && !_isCustom;

            if (shouldShowSimple && _showingSimple)
            {
                UpdateSimpleControls(parsed);
                return;
            }

            if (!shouldShowSimple && !_showingSimple && _editorContainer.hasChildNodes())
            {
                 // Already showing custom
                 _customCronInput.Text = _cron;
                 return;
            }

            _editorContainer.innerHTML = "";
            _showingSimple = shouldShowSimple;

            if (shouldShowSimple)
            {
                UpdateSimpleControls(parsed);

                var row = Div(_("tss-cron-row"));
                row.appendChild(Span(_("tss-cron-label"), TextBlock("Scheduled ").Render()));
                row.appendChild(_frequencyDropdown.Render());

                row.appendChild(_domContainer.Render());

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
                _customCronInput.OnClick((s, e) => StopEvent(e));
                row.appendChild(_customCronInput.Render());

                var wrappedRow = Button().ReplaceContent(HStack().AlignItemsCenter().NoDefaultMargin().Children(Icon(UIcons.AngleUp).PR(16), Icon(UIcons.Clock).PR(8), Raw(row)));
                wrappedRow.OnClick(() => Close());

                _editorContainer.appendChild(wrappedRow.Render());

                var actions = Div(_("tss-cron-actions"));
                actions.appendChild(_switchToSimpleButton.Render());

                _editorContainer.appendChild(actions);
            }
        }

        private void UpdateSimpleControls(CronStruct parsed)
        {
            string freq = "daily";
            if (parsed.DayOfMonth != -1) freq = "monthly";
            else if (!parsed.AllDays) freq = "weekly";

            var currentFreq = _frequencyDropdown.SelectedItems.FirstOrDefault()?.Text;
            if (currentFreq != freq)
            {
                foreach(var item in _frequencyItems)
                {
                    if (item.Text == freq) item.Selected();
                    else item.SelectedIf(false);
                }
            }

            if (freq == "monthly")
            {
                _domContainer.Show();
            }
            else
            {
                _domContainer.Collapse();
            }

            if (freq == "weekly" && _daysEnabled)
            {
                _daysStack.Show();
            }
            else
            {
                _daysStack.Collapse();
            }

            if (parsed.DayOfMonth != -1)
            {
                var currentDom = _dayOfMonthDropdown.SelectedItems.FirstOrDefault()?.Data;
                if (currentDom == null || (int)currentDom != parsed.DayOfMonth)
                {
                    foreach(var item in _dayOfMonthItems)
                    {
                        if ((int)item.Data == parsed.DayOfMonth)
                        {
                            item.Selected();
                            break;
                        }
                    }
                }
            }

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
        }

        private void SwitchToCustom()
        {
            var rect = _editorContainer.getBoundingClientRect().As<DOMRect>();
            _editorContainer.style.width = rect.width.px().ToString();

            _isCustom = true;
            UpdateEditorState();
        }

        private void SwitchToSimple()
        {
            _editorContainer.style.width = "";

            _cron = _customCronInput.Text;
            
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
                _observable.Value = _cron;
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

            string domPart = "*";
            string dayPart = "*";

            var freq = _frequencyDropdown.SelectedItems.FirstOrDefault()?.Text ?? "daily";

            if (freq == "monthly")
            {
                 var domItem = _dayOfMonthDropdown.SelectedItems.FirstOrDefault();
                 int dom = domItem != null ? (int)domItem.Data : 1;
                 domPart = dom.ToString();
            }
            else if (freq == "weekly")
            {
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
                dayPart = days.Contains("*") || days.Count == 0 ? "*" : string.Join(",", days);
            }

            _cron = $"{m} {h} {domPart} * {dayPart}";

            _observable.Value = _cron;
            _onChange?.Invoke(this);
            RenderDescription();
            if (_isOpen) UpdateEditorState();
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

            var wrappedRow = Button().ReplaceContent(HStack().AlignItemsCenter().NoDefaultMargin().Children(Icon(UIcons.AngleDown).PR(16), Icon(UIcons.Clock).PR(8), TextBlock(text)));
                
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

                if (parts[3] != "*") return res;

                if (parts[2] == "*")
                {
                    res.DayOfMonth = -1;
                }
                else if (int.TryParse(parts[2], out int dom) && dom >= 1 && dom <= 31)
                {
                    res.DayOfMonth = dom;
                }
                else
                {
                    return res;
                }

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
            public int DayOfMonth;
            public List<int> DaysOfWeek;
            public bool AllDays;
            public bool IsDaily;
            public bool IsValid;
        }
    }
}
