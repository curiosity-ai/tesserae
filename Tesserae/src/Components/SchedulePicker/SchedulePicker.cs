using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TNT;
using static TNT.T;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{

    public static class DateExt
    {
        public static int ToWeekStartMonday(this DayOfWeek dayOfWeekUS)
        {
            return ((((int)dayOfWeekUS - 1) % 7) + 7) % 7;
        }

        public static string Humanize(this DateTimeOffset date)
        {
            var delta = date - DateTimeOffset.UtcNow;

            if (delta.TotalSeconds < 30)
                return "right now".t();
            else if (delta.TotalMinutes < 2)
                return t($"in {delta.TotalSeconds:n0} seconds");
            else if (delta.TotalMinutes <= 2)
                return "in a minute".t();
            else if (delta.TotalHours < 1)
                return t($"in {delta.TotalMinutes:n0} minutes");
            else if (delta.TotalHours <= 1)
                return "in an hour".t();
            else if (delta.TotalHours <= 1.5)
                return "in about an hour".t();
            else if (delta.TotalDays < 1)
                return t($"in {delta.TotalHours:n0} hours");
            else if (delta.TotalDays <= 1)
                return "tomorrow".t();
            return t($"on {date:dddd MMM dd, yyyy} at {date:h} o'clock");
        }
    }



    [H5.Name("tss.SchedulePicker")]
    public sealed class SchedulePicker : IComponent
    {

        public enum ScheduleState
        {
            Active,
            Inactive,
            HalfActive
        }

        public static string[] GetWeekDays { get; } = new[]
        {
            "Monday".t(),
            "Tuesday".t(),
            "Wednesday".t(),
            "Thursday".t(),
            "Friday".t(),
            "Saturday".t(),
            "Sunday".t(),
        };
        private readonly Stack           _stack         = VStack();
        private const    int             _weekDayNumber = 7;
        private const    int             _hoursPerDay   = 24;
        private const    int             _totalHours    = _weekDayNumber * _hoursPerDay;
        private          ScheduleState[] _scheduleStates;
        private          string          _activeTooltip     { get; set; } = null;
        private          string          _inactiveTooltip   { get; set; } = null;
        private          string          _halfActiveTooltip { get; set; } = null;
        private readonly TextBlock       _currentState;
        private readonly TextBlock       _nextStateChange;
        private readonly Button[]        _buttons;
        private SchedulePicker() { }
        public SchedulePicker(ScheduleState initialState = ScheduleState.Active)
        {
            _scheduleStates = Enumerable.Range(0, _weekDayNumber * _hoursPerDay + 1).Select(_ => initialState).ToArray();

            var gridElements = new List<IComponent>();
            var tmpButtons = new List<Button>();

            gridElements.Add(TextBlock());

            for (var hour = 0; hour < _hoursPerDay; hour++)
            {
                gridElements.Add(TextBlock($"{hour:00}:00 - {hour + 1:00}:00"));
            }

            var weekDays = GetWeekDays;

            for (var day = 0; day < _weekDayNumber; day++)
            {
                gridElements.Add(TextBlock(weekDays[day]));

                for (var hour = 0; hour < _hoursPerDay; hour++)
                {
                    var day1 = day;
                    var hour1 = hour;

                    var btn = Button().H(25).WS().Class("tss-schedule-selector-button").OnClick(() =>
                    {
                        ToggleState(day1, hour1);
                    });
                    MakeDraggable(btn, day, hour);


                    gridElements.Add(btn);
                    tmpButtons.Add(btn);
                }
            }
            _buttons = tmpButtons.ToArray();

            _currentState = TextBlock();
            _nextStateChange = TextBlock();

            UpdateButtonState();

            var grid1 = Grid(
                    Enumerable.Range(0, _weekDayNumber + 1).Select(d => 1.fr()).ToArray(),
                    Enumerable.Range(0, _hoursPerDay + 1).Select(d => 1.fr()).ToArray())
               .WS().PT(16).PB(16).Gap(2.px()).FlowColumn().Children(gridElements);
            _stack.Add(HStack().Children(_currentState, _nextStateChange.PL(10.px())));
            _stack.Add(grid1);
        }


        public SchedulePicker ActiveTooltip(string tooltip)
        {
            _activeTooltip = tooltip;
            return this;
        }
        public SchedulePicker InactiveTooltip(string tooltip)
        {
            _inactiveTooltip = tooltip;
            return this;
        }
        public SchedulePicker HalfActiveTooltip(string tooltip)
        {
            _halfActiveTooltip = tooltip;
            return this;
        }

        public ScheduleState CurrentState()
        {
            var now = DateTimeOffset.Now;

            var day = now.DayOfWeek.ToWeekStartMonday();
            var hour = now.Hour;
            return _scheduleStates[GetIndex(day, hour)];
        }

        public (DateTimeOffset changeDateTime, ScheduleState changeTo)? NextStateChange()
        {
            var now = DateTimeOffset.Now;
            var currentDay = now.DayOfWeek.ToWeekStartMonday();
            var currentHour = now.Hour;
            var currentState = _scheduleStates[GetIndex(currentDay, currentHour)];

            var currentIndex = GetIndex(currentDay, currentHour);

            for (var i = 0; i < _totalHours; i++)
            {
                if (currentState != _scheduleStates[(currentIndex + i) % _totalHours])
                {
                    var nextDt = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, 0, 0, offset: TimeSpan.Zero).AddHours(i);

                    return (nextDt, _scheduleStates[(currentIndex + i) % _totalHours]);
                }
            }
            return null;

        }

//        private void SetDraggedRange()
//        {
//            var dayMin = Math.Min(currentDragStartDay,   currentDragEndDay);
//            var hourMin = Math.Min(currentDragStartHour, currentDragEndHour);
//
//            var dayMax = Math.Max(currentDragStartDay,   currentDragEndDay);
//            var hourMax = Math.Max(currentDragStartHour, currentDragEndHour);
//
//            console.log(dayMin, hourMin, " :: ", dayMax, hourMax);
//
//            for (var day = dayMin; day <= dayMax; day++)
//            {
//                for (var hour = hourMin; hour <= hourMax; hour++)
//                {
//                    _scheduleStates[GetIndex(day, hour)] = currentDragStartState; //firing multiple times is fine due to the defer in the observable list
//                }
//            }
//        }


        private void ToggleState(int day, int hour)
        {
            _scheduleStates[GetIndex(day, hour)] = (ScheduleState)(((int)_scheduleStates[GetIndex(day, hour)] + 1) % Enum.GetNames(typeof(ScheduleState)).Length);
            UpdateButtonState();
        }

        private int GetIndex(int day, int hour)
        {
            var i = (_hoursPerDay * day) + hour;
            if (i < 0) throw new ArgumentException();
            if (i > (_weekDayNumber * _hoursPerDay)) throw new ArgumentException();
            return i;
        }

        private void SetButtonState(Button btn, ScheduleState state, bool hover)
        {
            var btnElement = btn.Render();

            if (hover)
            {
                btnElement.classList.add("tss-schedule-selector-button-hover");
            }
            else
            {
                btnElement.classList.remove("tss-schedule-selector-button-hover");
            }

            switch (state)
            {
                case ScheduleState.Active:
                {
                    btnElement.classList.remove("half-active-selector-button");
                    btnElement.classList.add("active-selector-button");


                    if (_activeTooltip is object)
                    {
                        btn.Tooltip(_activeTooltip);
                    }
                    break;
                }
                case ScheduleState.Inactive:
                {
                    btnElement.classList.remove("half-active-selector-button");
                    btnElement.classList.remove("active-selector-button");

                    if (_inactiveTooltip is object)
                    {
                        btn.Tooltip(_inactiveTooltip);
                    }
                    break;
                }
                case ScheduleState.HalfActive:
                {
                    btnElement.classList.add("half-active-selector-button");
                    btnElement.classList.remove("active-selector-button");

                    if (_halfActiveTooltip is object)
                    {
                        btn.Tooltip(_halfActiveTooltip);
                    }
                    break;
                }
                default: throw new ArgumentOutOfRangeException();
            }
        }


        private void ApplyDrag()
        {
            for (var day = currentDayMin; day <= currentDayMax; day++)
            {
                for (var hour = currentHourMin; hour < currentHourMax; hour++)
                {
                    _scheduleStates[GetIndex(day, hour)] = currentDragStartState;
                }
            }
            UpdateButtonState();
            _currentState.Text($"Current state: {Enum.GetName(typeof(SchedulePicker.ScheduleState), CurrentState())}");
            var nextChange = NextStateChange();

            _nextStateChange.Text(nextChange.HasValue
                ? $"Next change: {nextChange.Value.changeDateTime.Humanize()} to {Enum.GetName(typeof(SchedulePicker.ScheduleState), nextChange.Value.changeTo)}"
                : "");
        }


        private void UpdateButtonState()
        {
            for (var day = 0; day < _weekDayNumber; day++)
            {
                for (var hour = 0; hour < _hoursPerDay; hour++)
                {
                    var index = GetIndex(day, hour);

                    if (currentDayMin <= day && currentDayMax >= day && currentHourMin <= hour && currentHourMax >= hour)
                    {
                        SetButtonState(_buttons[index], currentDragStartState, hover: true);
                    }
                    else
                    {
                        SetButtonState(_buttons[index], _scheduleStates[index], hover: false);
                    }
                }
            }
            _currentState.Text($"Current state: {Enum.GetName(typeof(SchedulePicker.ScheduleState), CurrentState())}");
            var nextChange = NextStateChange();

            _nextStateChange.Text(nextChange.HasValue
                ? $"Next change: {nextChange.Value.changeDateTime.Humanize()} to {Enum.GetName(typeof(SchedulePicker.ScheduleState), nextChange.Value.changeTo)}"
                : "");
        }

        private static SchedulePicker _dragSource;
//        private static IComponent     _beingDragged;

        private static ScheduleState currentDragStartState = ScheduleState.Inactive;
        private static int           currentDragStartDay   = -1;
        private static int           currentDragStartHour  = -1;
        private static int           currentDragEndDay     = -1;
        private static int           currentDragEndHour    = -1;

        private static int currentDayMin  = -1;
        private static int currentHourMin = -1;
        private static int currentDayMax  = -1;
        private static int currentHourMax = -1;

        private void MakeDraggable(Button component, int day, int hour)
        {
            var element = component.Render();
            element.draggable = true;
            element.style.userSelect = "none";

            element.ondragstart += (e) =>
            {
                console.log("ondragstart", day, hour);

                currentDragStartDay = day;
                currentDragStartHour = hour;
                currentDragEndDay = day;
                currentDragEndHour = hour;
                currentDragStartState = _scheduleStates[GetIndex(day, hour)];
                currentDayMin = Math.Min(currentDragStartDay,   currentDragEndDay);
                currentHourMin = Math.Min(currentDragStartHour, currentDragEndHour);
                currentDayMax = Math.Max(currentDragStartDay,   currentDragEndDay);
                currentHourMax = Math.Max(currentDragStartHour, currentDragEndHour);

                UpdateButtonState();

                _dragSource = this;
                e.As<DragEvent>().dataTransfer.effectAllowed = "move";
                e.As<DragEvent>().dataTransfer.setData("text/html", element.outerHTML);
            };

            element.ondragend += (e) =>
            {
                console.log("ondragend", day, hour);

                _dragSource = null;
            };

            element.ondragover += (e) =>
            {
                if (_dragSource == this)
                {
                    StopEvent(e); //Need to stop the event to make this a drop target

                    console.log("ondragover", day, hour);
//                    if (_list.IndexOf(component) < _list.IndexOf(_beingDragged))
//                    {
//                        element.classList.add("drag-is-over-top");
//                    }
//                    else
//                    {
//                        element.classList.add("drag-is-over-bottom");
//                    }

                    e.As<DragEvent>().dataTransfer.dropEffect = "move";
                }
            };

            element.ondragenter += (e) =>
            {
                console.log("ondragenter", day, hour);

                if (currentDragStartDay >= 0)
                {
                    StopEvent(e); //Need to stop the event to make this a drop target

                    currentDragEndDay = day;
                    currentDragEndHour = hour;
                    currentDayMin = Math.Min(currentDragStartDay,   currentDragEndDay);
                    currentHourMin = Math.Min(currentDragStartHour, currentDragEndHour);
                    currentDayMax = Math.Max(currentDragStartDay,   currentDragEndDay);
                    currentHourMax = Math.Max(currentDragStartHour, currentDragEndHour);
                    UpdateButtonState();
                }
            };

            element.ondragleave += (e) =>
            {
                console.log("ondragleave", day, hour);

//                if (_beingDragged is object && _beingDraggedKey == _key)
//                {
//                    element.classList.remove("drag-is-over-top");
//                    element.classList.remove("drag-is-over-bottom");
//                }
            };

            element.ondrop += (e) =>
            {
                console.log("drop", day, hour);

                ApplyDrag();

                currentDragStartState = ScheduleState.Inactive;
                currentDragStartDay = -1;
                currentDragStartHour = -1;
                currentDragEndDay = -1;
                currentDragEndHour = -1;
                currentDayMin = -1;
                currentHourMin = -1;
                currentDayMax = -1;
                currentHourMax = -1;


//                if (_beingDragged is object )
//                {
//                    element.classList.remove("drag-is-over-top");
//                    element.classList.remove("drag-is-over-bottom");
//                    var index = _list.IndexOf(component);
//                    if (index >= 0)
//                    {
//                        _list.Remove(_beingDragged);
//                        _list.Insert(index, _beingDragged);
//
//                        base.Clear();
//                        foreach (var item in _list)
//                        {
//                            base.Add(item);
//                        }
//                        RaiseChanged();
//                    }
//                }
            };
        }


        public HTMLElement Render()
        {
            return _stack.Render();
        }
    }
}