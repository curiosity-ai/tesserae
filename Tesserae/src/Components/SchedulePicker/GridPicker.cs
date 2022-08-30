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
        private readonly Stack _stack = VStack().Class("tss-gridpicker");
        private readonly int _columns;
        private readonly int _rows;
        private readonly int _stateCount;
        private int[][] _states;
        private readonly Action<Button, int, int> _formatState;
        private readonly Button[][] _buttons;
        private event ComponentEventHandler<GridPicker, Event> Changed;

        public bool IsDragging => _dragSource == this;

        public GridPicker(string[] columnNames, string[] rowNames, int states, int[][] initialStates, Action<Button, int, int> formatState, UnitSize[] columns = null, UnitSize rowHeight = null)
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
                var rowText = TextBlock(rowNames[row]).Class("tss-gridpicker-row-title").HS().WS();
                if(rowHeight is object)
                {
                    rowText.Render().style.lineHeight = rowHeight.ToString();
                }
                gridElements.Add(rowText);
            }

            _buttons = Enumerable.Range(0, _rows).Select(_ => new Button[_columns]).ToArray();

            for (var column = 0; column < _columns; column++)
            {
                gridElements.Add(TextBlock(columnNames[column]).WS().Class("tss-gridpicker-column-title"));

                for (var row = 0; row < _rows; row++)
                {
                    var columnIndex = column;
                    var rowIndex = row;

                    var btn = Button().HS().WS().Class("tss-gridpicker-button").OnClick(() =>
                    {
                        NextState(columnIndex, rowIndex);
                    });

                    ConfigureButton(btn, column, row);

                    gridElements.Add(btn);
                    _buttons[row][column] = btn;
                }
            }

            UpdateAll();

            if(columns is object)
            {
                if(columns.Length < _columns)
                {
                    columns = columns.Concat(Enumerable.Repeat(columns.Last(), _columns - columns.Length + 1)).ToArray();
                }
            }
            else
            {
                columns = Enumerable.Range(0, _columns + 1).Select(d => 1.fr()).ToArray();
            }

            if (rowHeight is null) rowHeight = 1.fr();

            _stack.Add(Grid(
                    columns,
                    Enumerable.Range(0, _rows + 1).Select(d => rowHeight).ToArray())
                .AlignItemsCenter()
               .WS().PT(16).PB(16).Gap(2.px()).FlowColumn().Children(gridElements));
        }

        public int[][] GetState()
        {
            return _states.Select(t => t.ToArray()).ToArray();
        }

        public GridPicker SetState(int[][] state)
        {
            _states = state;
            UpdateAll();
            if (_hoverColumn >= 0) HoverState(_hoverColumn, _hoverRow);
            return this;
        }

        private void RaiseOnChange(Event ev) => Changed?.Invoke(this, ev);

        public GridPicker OnChange(ComponentEventHandler<GridPicker, Event> onChange)
        {
            Changed += onChange;
            return this;
        }

        private void NextState(int column, int row)
        {
            var current = _states[row][column];
            var newIndex = (current + 1) % _stateCount;
            _states[row][column] = newIndex;

            UpdateSingle(column, row);
        }

        private void UpdateSingle(int column, int row)
        {
            if (_draggingColumnMin <= column && _draggingColumnMax >= column && _draggingRowMin <= row && _draggingRowMax >= row)
            {
                _formatState(_buttons[row][column], _draggingState, _states[row][column]);
            }
            else
            {
                _formatState(_buttons[row][column], _states[row][column], -1);
            }

            
            if (!IsDragging)
            {
                RaiseOnChange(null);
            }
        }

        private void HoverState(int column, int row)
        {
            var current = _states[row][column];
            var newIndex = (current + 1) % _stateCount;

            if (_draggingColumnMin <= column && _draggingColumnMax >= column && _draggingRowMin <= row && _draggingRowMax >= row)
            {
                _formatState(_buttons[row][column], _draggingState, _states[row][column]);
            }
            else
            {
                _formatState(_buttons[row][column], newIndex, _states[row][column]);
            }
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
        }


        private void UpdateAll()
        {
            for (var row = 0; row < _rows; row++)
            {
                var br = _buttons[row];
                var sr = _states[row];
                for (var column = 0; column < _columns; column++)
                {
                    if (IsDragging && _draggingColumnMin <= column && _draggingColumnMax >= column && _draggingRowMin <= row && _draggingRowMax >= row)
                    {
                        _formatState(br[column], _draggingState, sr[column]);
                    }
                    else
                    {
                        _formatState(br[column], sr[column], -1);
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
        private int _hoverColumn = -1;
        private int _hoverRow = -1;

        private void ConfigureButton(Button component, int column, int row)
        {
            var element = component.Render();
            element.draggable = true;
            element.style.userSelect = "none";

            element.onmouseenter += (e) =>
            {
                _hoverColumn = column;
                _hoverRow = row;
                HoverState(column, row);
            };

            element.onmouseleave += (e) =>
            {
                _hoverColumn = -1;
                _hoverRow = -1;
                UpdateSingle(column, row);
            };

            element.ondragstart += (e) =>
            {
                NextState(column, row);

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

                UpdateAll();

                e.As<DragEvent>().dataTransfer.effectAllowed = "move";
                var nothing = DIV();
                e.As<DragEvent>().dataTransfer.setDragImage(nothing, 0, 0);
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

                        UpdateAll();
                    }
                }
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

                UpdateAll();

                RaiseOnChange(null);
            }
        }

        public HTMLElement Render()
        {
            return _stack.Render();
        }
    }
}