using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.Calendar")]
    public class Calendar : ComponentBase<Calendar, HTMLDivElement>, IObservableComponent<DateTime>
    {
        private DateTime _viewDate;
        private DateTime _selectedDate;
        private readonly SettableObservable<DateTime> _observable;

        public Calendar(DateTime? initialDate = null)
        {
            _selectedDate = initialDate ?? DateTime.Today;
            _viewDate = new DateTime(_selectedDate.Year, _selectedDate.Month, 1);
            _observable = new SettableObservable<DateTime>(_selectedDate);
            InnerElement = Div(_("tss-calendar"));
            Rebuild();
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set { _selectedDate = value; _observable.Value = value; Rebuild(); }
        }

        private void Rebuild()
        {
            ClearChildren(InnerElement);

            // Header
            var prevBtn = Button().SetIcon(UIcons.AngleLeft).NoBorder().NoBackground().OnClick(() => { _viewDate = _viewDate.AddMonths(-1); Rebuild(); });
            var nextBtn = Button().SetIcon(UIcons.AngleRight).NoBorder().NoBackground().OnClick(() => { _viewDate = _viewDate.AddMonths(1); Rebuild(); });
            var title = Div(_("tss-calendar-title", text: _viewDate.ToString("MMMM yyyy")));
            var header = Div(_("tss-calendar-header"), prevBtn.Render(), title, nextBtn.Render());
            InnerElement.appendChild(header);

            // Grid
            var grid = Div(_("tss-calendar-grid"));
            string[] dayLabels = { "Su", "Mo", "Tu", "We", "Th", "Fr", "Sa" };
            foreach (var label in dayLabels)
            {
                grid.appendChild(Div(_("tss-calendar-day-header", text: label)));
            }

            var firstDayOfMonth = new DateTime(_viewDate.Year, _viewDate.Month, 1);
            var startOfGrid = firstDayOfMonth.AddDays(-(int)firstDayOfMonth.DayOfWeek);

            for (int i = 0; i < 42; i++)
            {
                var date = startOfGrid.AddDays(i);
                var dayEl = Div(_("tss-calendar-day", text: date.Day.ToString()));

                if (date.Month != _viewDate.Month) dayEl.classList.add("tss-outside");
                if (date.Date == DateTime.Today) dayEl.classList.add("tss-today");
                if (date.Date == _selectedDate.Date) dayEl.classList.add("tss-selected");

                dayEl.onclick = (e) =>
                {
                    StopEvent(e);
                    SelectedDate = date;
                    RaiseOnChange(ev: null);
                };

                grid.appendChild(dayEl);
            }

            InnerElement.appendChild(grid);
        }

        public IObservable<DateTime> AsObservable() => _observable;

        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }
}
