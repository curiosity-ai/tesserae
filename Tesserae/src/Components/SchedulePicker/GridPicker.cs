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
    [H5.Name("tss.GridPicker")]
    public sealed class GridPicker : IComponent
    {
        //public enum ScheduleState
        //{
        //    Active,
        //    Inactive,
        //    HalfActive
        //}

        //public static string[] GetWeekDays { get; } = new[]
        //{
        //    "Monday".t(),
        //    "Tuesday".t(),
        //    "Wednesday".t(),
        //    "Thursday".t(),
        //    "Friday".t(),
        //    "Saturday".t(),
        //    "Sunday".t(),
        //};

        private readonly Stack           _stack         = VStack();
        private readonly int             _columns;
        private readonly int             _rows;
        private readonly int             _stateCount;
        private readonly int[][] _states;
        private readonly Action<Button, int, int> _formatState;
        private readonly Button[][]        _buttons;
        private GridPicker() { }

        public GridPicker(string[] columnNames, string[] rowNames, int states, int[][] initialStates, Action<Button, int, int> formatState)
        {
            _columns        = columnNames.Length;
            _rows           = rowNames.Length;
            _stateCount     = states;
            _states         = initialStates;
            _formatState    = formatState;
            var gridElements = new List<IComponent>();

            gridElements.Add(TextBlock());

            for (var row = 0; row < _rows; row++)
            {
                gridElements.Add(TextBlock(rowNames[row]).TextRight());
            }
            _buttons = Enumerable.Range(0, _rows).Select(_ => new Button[_columns]).ToArray();

            for (var column = 0; column < _columns; column++)
            {
                gridElements.Add(TextBlock(columnNames[column]).TextCenter());

                for (var row = 0; row < _rows; row++)
                {
                    var columnIndex = column;
                    var rowIndex = row;

                    var btn = Button().LessPadding().H(25).WS().Class("tss-gridpicker-button").OnClick(() =>
                    {
                        NextState(columnIndex, rowIndex);
                    });

                    MakeDraggable(btn, column, row);

                    gridElements.Add(btn);
                    _buttons[row][column] = btn;
                }
            }

            UpdateButtonState();

            _stack.Add(Grid(
                    Enumerable.Range(0, _columns + 1).Select(d => 1.fr()).ToArray(),
                    Enumerable.Range(0, _rows + 1).Select(d => 1.fr()).ToArray())
               .WS().PT(16).PB(16).Gap(2.px()).FlowColumn().Children(gridElements));
        }

        public int[][] GetState()
        {
            return _states.Select(t => t.ToArray()).ToArray();
        }


        private void NextState(int column, int row)
        {
            var current = _states[row][column];
            var newIndex = (current + 1) % _stateCount;
            _states[row][column] = newIndex;
            UpdateButtonState();
        }

        private void SetButtonState(Button btn, int state, int previousState)
        {
            var btnElement = btn.Render();
            var classList = btnElement.classList;
            _formatState(btn, state, previousState);
        }


        private void ApplyDrag()
        {
            for (var row = _draggingRowMin; row <= _draggingRowMax; row++)
            {
                var sr = _states[row];
                for (var column = _draggingColumnMin; column <= _draggingColumnMax; column++)
                {
                    sr[column] = _draggingState;
                }
            }

            UpdateButtonState();
        }


        private void UpdateButtonState()
        {
            bool isDragging = _dragSource == this;

            for (var row = 0; row < _rows; row++)
            {
                var br = _buttons[row];
                var sr = _states[row];
                for (var column = 0; column < _columns; column++)
                {
                    if (isDragging && _draggingColumnMin <= column && _draggingColumnMax >= column && _draggingRowMin <= row && _draggingRowMax >= row)
                    {
                        SetButtonState(br[column], _draggingState, sr[column]);
                    }
                    else
                    {
                        SetButtonState(br[column], sr[column], -1);
                    }
                }
            }
        }

        private static GridPicker _dragSource;

        private static int _draggingColumnStart   = -1;
        private static int _draggingRowStart      = -1;
        private static int _draggingColumnEnd     = -1;
        private static int _draggingRowEnd        = -1;
        private static int _draggingColumnMin     = -1;
        private static int _draggingRowMin        = -1;
        private static int _draggingColumnMax     = -1;
        private static int _draggingRowMax        = -1;
        private static int _draggingState         = -1;


        private void MakeDraggable(Button component, int column, int row)
        {
            var element = component.Render();
            element.draggable = true;
            element.style.userSelect = "none";

            element.ondragstart += (e) =>
            {
                _dragSource = this;

                _draggingColumnStart = column;
                _draggingRowStart    = row;
                _draggingColumnEnd   = column;
                _draggingRowEnd      = row;
                _draggingColumnMin   = column;
                _draggingRowMin      = row;
                _draggingColumnMax   = column;
                _draggingRowMax      = row;
                _draggingState       = _states[row][column];

                UpdateButtonState();

                e.As<DragEvent>().dataTransfer.effectAllowed = "move";
                e.As<DragEvent>().dataTransfer.setData("text/html", element.outerHTML);
            };

            element.ondragend += (e) =>
            {
                TryEndDrag();
            };

            element.ondragover += (e) =>
            {
                if (_dragSource == this)
                {
                    StopEvent(e); //Need to stop the event to make this a drop target
                    e.As<DragEvent>().dataTransfer.dropEffect = "move";
                }
            };

            element.ondragenter += (e) =>
            {
                if (_dragSource == this)
                {
                    if (_draggingColumnStart >= 0)
                    {
                        StopEvent(e); //Need to stop the event to make this a drop target

                        _draggingColumnEnd = column;
                        _draggingRowEnd = row;
                        _draggingColumnMin = Math.Min(_draggingColumnStart, _draggingColumnEnd);
                        _draggingRowMin = Math.Min(_draggingRowStart, _draggingRowEnd);
                        _draggingColumnMax = Math.Max(_draggingColumnStart, _draggingColumnEnd);
                        _draggingRowMax = Math.Max(_draggingRowStart, _draggingRowEnd);

                        UpdateButtonState();
                    }
                }
            };

            element.ondragleave += (e) =>
            {
                console.log("ondragleave", column, row);
            };

            element.ondrop += (e) =>
            {
                TryEndDrag();
            };
        }

        private void TryEndDrag()
        {
            if (_dragSource == this)
            {
                ApplyDrag();

                _dragSource = null;

                _draggingState = -1;
                _draggingColumnStart = -1;
                _draggingRowStart = -1;
                _draggingColumnEnd = -1;
                _draggingRowEnd = -1;
                _draggingColumnMin = -1;
                _draggingRowMin = -1;
                _draggingColumnMax = -1;
                _draggingRowMax = -1;

                UpdateButtonState();
            }
        }

        public HTMLElement Render()
        {
            return _stack.Render();
        }
    }
}