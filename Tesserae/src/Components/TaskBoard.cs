using System;
using System.Collections.Generic;
using H5.Core;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A kanban-style board with named columns and draggable cards.
    /// </summary>
    [H5.Name("tss.TaskBoard")]
    public class TaskBoard : IComponent, IHasMarginPadding
    {
        private readonly HTMLElement _container;
        private readonly Stack _stack;
        private readonly List<TaskBoardColumn> _columns = new List<TaskBoardColumn>();
        private bool _isRowMode = false;
        private readonly Sortable _sortable;
        private bool _isReadOnly = false;
        private Action<SortableEvent> _onColumnDrop;
        private Action<SortableEvent> _onColumnUpdate;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public TaskBoard()
        {
            _stack = Stack().Horizontal().Width(100.percent()).Height(100.percent());
            _stack.Class("tss-taskboard");

            _container = Div(_("tss-taskboard-container", styles: s =>
            {
                s.width = "100%";
                s.height = "100%";
                s.display = "flex";
                s.overflowX = "auto";
                s.overflowY = "hidden";
            }), _stack.Render());

            _sortable = new Sortable(_stack.Render(), new SortableOptions()
            {
                animation = 150,
                ghostClass = "tss-taskboard-column-ghost",
                filter = ".tss-sortable-disable",
                direction = "horizontal",
                onEnd = (e) =>
                {
                    _onColumnDrop?.Invoke(e);
                },
                onUpdate = (e) =>
                {
                    _onColumnUpdate?.Invoke(e);
                }
            });
        }

        /// <summary>
        /// Gets or sets the CSS margin of the component.
        /// </summary>
        public string Margin { get => _stack.Margin; set => _stack.Margin = value; }
        /// <summary>
        /// Gets or sets the CSS padding of the component.
        /// </summary>
        public string Padding { get => _stack.Padding; set => _stack.Padding = value; }

        /// <summary>
        /// Adds the given column to the component.
        /// </summary>
        public TaskBoard AddColumn(TaskBoardColumn column)
        {
            _columns.Add(column);
            _stack.Add(column);
            column.IsReadOnly = _isReadOnly;
            return this;
        }

        /// <summary>
        /// Defines the columns of the grid (track sizes).
        /// </summary>
        public TaskBoard Columns(params TaskBoardColumn[] columns)
        {
            _stack.Clear();
            _columns.Clear();
            foreach (var col in columns)
            {
                AddColumn(col);
            }
            return this;
        }

        /// <summary>
        /// Configures the read only on the component.
        /// </summary>
        public TaskBoard ReadOnly(bool isReadOnly = true)
        {
            _isReadOnly = isReadOnly;
            _sortable.Disabled = isReadOnly;
            foreach (var col in _columns)
            {
                col.IsReadOnly = isReadOnly;
            }
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the column drop event fires.
        /// </summary>
        public TaskBoard OnColumnDrop(Action<SortableEvent> onColumnDrop)
        {
            _onColumnDrop = onColumnDrop;
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the column update event fires.
        /// </summary>
        public TaskBoard OnColumnUpdate(Action<SortableEvent> onColumnUpdate)
        {
            _onColumnUpdate = onColumnUpdate;
            return this;
        }

        /// <summary>
        /// Configures the row mode on the component.
        /// </summary>
        public TaskBoard RowMode(bool isRowMode = true)
        {
            _isRowMode = isRowMode;
            if (isRowMode)
            {
                _stack.Vertical();
                _stack.NoWrap();
                _stack.Class("tss-taskboard-row-mode");
            }
            else
            {
                _stack.Horizontal();
                _stack.NoWrap();
                _stack.RemoveClass("tss-taskboard-row-mode");
            }

            foreach (var col in _columns)
            {
                col.RowMode(isRowMode);
            }
            return this;
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public HTMLElement Render() => _container;
    }

    [H5.Name("tss.TaskBoardColumn")]
    public class TaskBoardColumn : IComponent
    {
        private readonly HTMLElement _container;
        private readonly Stack _headerStack;
        private readonly Stack _cardsContainer;
        private readonly List<TaskBoardCard> _cards = new List<TaskBoardCard>();
        private readonly TextBlock _titleText;
        private string _groupName;
        private readonly Sortable _sortable;
        private bool _isReadOnly = false;
        private Action<SortableEvent> _onCardDrop;
        private Action<SortableEvent> _onCardAdd;
        private Action<SortableEvent> _onCardRemove;
        private Action<SortableEvent> _onCardUpdate;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public TaskBoardColumn(string title, string sortableGroup = "taskboard")
        {
            _groupName = sortableGroup;
            _titleText = TextBlock(title).SemiBold();

            _headerStack = Stack().Horizontal().AlignItemsCenter().JustifyContent(ItemJustify.Between).Padding("8px");
            _headerStack.Add(_titleText);

            _cardsContainer = Stack().Vertical().Padding("4px").Class("tss-taskboard-cards-container");
            _cardsContainer.Style(s =>
            {
                s.overflowY = "auto";
                s.flexGrow = "1";
                s.minHeight = "50px";
            });

            _container = Div(_("tss-taskboard-column", styles: s =>
            {
                s.display = "flex";
                s.flexDirection = "column";
                s.background = "var(--tss-secondary-background-color)";
                s.borderRadius = "4px";
                s.margin = "8px";
                s.width = "300px";
                s.height = "calc(100% - 16px)";
                s.maxHeight = "100%";
                s.flexShrink = "0";
            }), _headerStack.Render(), _cardsContainer.Render());

            _sortable = new Sortable(_cardsContainer.Render(), new SortableOptions()
            {
                animation = 150,
                group = _groupName,
                ghostClass = "tss-taskboard-card-ghost",
                onEnd = (e) =>
                {
                    _onCardDrop?.Invoke(e);
                },
                onAdd = (e) =>
                {
                    _onCardAdd?.Invoke(e);
                },
                onRemove = (e) =>
                {
                    _onCardRemove?.Invoke(e);
                },
                onUpdate = (e) =>
                {
                    _onCardUpdate?.Invoke(e);
                }
            });
        }

        /// <summary>
        /// Returns a value indicating whether the component is read only.
        /// </summary>
        public bool IsReadOnly
        {
            get => _isReadOnly;
            internal set
            {
                _isReadOnly = value;
                _sortable.Disabled = value;
            }
        }

        /// <summary>
        /// Registers a callback invoked when the card drop event fires.
        /// </summary>
        public TaskBoardColumn OnCardDrop(Action<SortableEvent> onCardDrop)
        {
            _onCardDrop = onCardDrop;
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the card add event fires.
        /// </summary>
        public TaskBoardColumn OnCardAdd(Action<SortableEvent> onCardAdd)
        {
            _onCardAdd = onCardAdd;
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the card remove event fires.
        /// </summary>
        public TaskBoardColumn OnCardRemove(Action<SortableEvent> onCardRemove)
        {
            _onCardRemove = onCardRemove;
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the card update event fires.
        /// </summary>
        public TaskBoardColumn OnCardUpdate(Action<SortableEvent> onCardUpdate)
        {
            _onCardUpdate = onCardUpdate;
            return this;
        }

        /// <summary>
        /// Configures the component to cards.
        /// </summary>
        public TaskBoardColumn Cards(params TaskBoardCard[] cards)
        {
            _cardsContainer.Clear();
            _cards.Clear();
            foreach (var card in cards)
            {
                AddCard(card);
            }
            return this;
        }

        /// <summary>
        /// Adds the given card to the component.
        /// </summary>
        public TaskBoardColumn AddCard(TaskBoardCard card)
        {
            _cards.Add(card);
            _cardsContainer.Add(card);
            return this;
        }

        internal void RowMode(bool isRowMode)
        {
            if (isRowMode)
            {
                _container.style.width = "100%";
                _container.style.minWidth = "";
                _container.style.height = "auto";
                _container.style.maxHeight = "none";
                _container.style.flexShrink = "0";

                _cardsContainer.Horizontal();
                _cardsContainer.Wrap();
                _cardsContainer.Class("tss-taskboard-cards-container-row-mode");
            }
            else
            {
                _container.style.width = "300px";
                _container.style.minWidth = "";
                _container.style.height = "calc(100% - 16px)";
                _container.style.maxHeight = "100%";
                _container.style.flexShrink = "0";

                _cardsContainer.Vertical();
                _cardsContainer.NoWrap();
                _cardsContainer.RemoveClass("tss-taskboard-cards-container-row-mode");
            }
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public HTMLElement Render() => _container;
    }

    [H5.Name("tss.TaskBoardCard")]
    public class TaskBoardCard : IComponent
    {
        private readonly HTMLElement _container;
        private readonly Stack _layout;
        private HTMLElement _header;
        private HTMLElement _footer;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public TaskBoardCard(IComponent content)
        {
            _layout = Stack().Vertical().Gap(8.px());
            _layout.Add(content);

            _container = Div(_("tss-taskboard-card", styles: s =>
            {
                s.background = "var(--tss-default-background-color)";
                s.borderRadius = "6px";
                s.boxShadow = "0 1px 3px rgba(0,0,0,0.12), 0 1px 2px rgba(0,0,0,0.24)";
                s.padding = "12px";
                s.margin = "4px";
                s.cursor = "grab";
                s.display = "flex";
                s.flexDirection = "column";
            }), _layout.Render());
        }

        /// <summary>
        /// Configures the component to header.
        /// </summary>
        public TaskBoardCard Header(IComponent headerContent)
        {
            if (_header != null)
            {
                _container.removeChild(_header);
            }

            _header = Div(_("tss-taskboard-card-header", styles: s =>
            {
                s.marginBottom = "8px";
                s.display = "flex";
                s.alignItems = "center";
                s.justifyContent = "space-between";
            }), headerContent.Render());

            _container.insertBefore(_header, _layout.Render());
            return this;
        }

        /// <summary>
        /// Configures the component to footer.
        /// </summary>
        public TaskBoardCard Footer(IComponent footerContent)
        {
            if (_footer != null)
            {
                _container.removeChild(_footer);
            }

            _footer = Div(_("tss-taskboard-card-footer", styles: s =>
            {
                s.marginTop = "12px";
                s.paddingTop = "8px";
                s.borderTop = "1px solid var(--tss-default-border-color)";
                s.display = "flex";
                s.alignItems = "center";
                s.justifyContent = "space-between";
            }), footerContent.Render());

            _container.appendChild(_footer);
            return this;
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public HTMLElement Render() => _container;
    }
}
