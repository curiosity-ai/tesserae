using System;
using System.Collections.Generic;
using H5.Core;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.TaskBoard")]
    public class TaskBoard : IComponent, IHasMarginPadding
    {
        private readonly HTMLElement _container;
        private readonly Stack _stack;
        private readonly List<TaskBoardColumn> _columns = new List<TaskBoardColumn>();
        private bool _isRowMode = false;

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

            var sortable = new Sortable(_stack.Render(), new SortableOptions()
            {
                animation = 150,
                ghostClass = "tss-taskboard-column-ghost",
                filter = ".tss-sortable-disable",
                direction = "horizontal"
            });
        }

        public string Margin { get => _stack.Margin; set => _stack.Margin = value; }
        public string Padding { get => _stack.Padding; set => _stack.Padding = value; }

        public TaskBoard AddColumn(TaskBoardColumn column)
        {
            _columns.Add(column);
            _stack.Add(column);
            return this;
        }

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

            var sortable = new Sortable(_cardsContainer.Render(), new SortableOptions()
            {
                animation = 150,
                group = _groupName,
                ghostClass = "tss-taskboard-card-ghost"
            });
        }

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
            }
        }

        public HTMLElement Render() => _container;
    }

    [H5.Name("tss.TaskBoardCard")]
    public class TaskBoardCard : IComponent
    {
        private readonly HTMLElement _container;
        private readonly Stack _layout;
        private HTMLElement _header;
        private HTMLElement _footer;

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

        public HTMLElement Render() => _container;
    }
}
