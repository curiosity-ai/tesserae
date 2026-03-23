using System;
using System.Collections.Generic;
using H5;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.Plan")]
    public sealed class Plan : IComponent, IHasMarginPadding
    {
        private readonly HTMLElement _innerElement;
        private readonly Card _card;
        private readonly Stack _mainStack;

        // Header
        private readonly Stack _headerStack;
        private readonly TextBlock _title;
        private readonly Stack _headerCommandsStack;

        // Task List
        private readonly Stack _tasksStack;
        private readonly List<Task> _tasks = new List<Task>();

        // Footer Message
        private readonly Stack _footerMessageStack;
        private readonly TextBlock _footerMessage;
        private readonly Stack _footerCommandsStack;

        // Progress
        private readonly Stack _progressStack;
        private readonly ProgressIndicator _progressIndicator;
        private readonly Button _startStopButton;

        public Plan(string title)
        {
            _title = TextBlock(title).SemiBold().MediumPlus();
            _headerCommandsStack = Stack().Horizontal().AlignItems(ItemAlign.Center);

            _headerStack = Stack().Horizontal().JustifyContent(ItemJustify.Between).AlignItems(ItemAlign.Center).Width(100.percent()).Children(
                _title,
                _headerCommandsStack
            );

            _tasksStack = Stack().Vertical().Gap(16.px()).Width(100.percent()).PaddingBottom(16.px()).PaddingTop(16.px());

            _footerMessage = TextBlock("").Small();
            _footerCommandsStack = Stack().Horizontal().AlignItems(ItemAlign.Center);

            _footerMessageStack = Stack().Horizontal().JustifyContent(ItemJustify.Between).AlignItems(ItemAlign.Center).Width(100.percent()).Children(
                _footerMessage,
                _footerCommandsStack
            );

            _progressIndicator = ProgressIndicator().Indeterminated();
            _startStopButton = Button().SetIcon(UIcons.SquareSmall).NoBorder().NoBackground();
            _startStopButton.Render().classList.remove("tss-btn-default");
            _startStopButton.Render().classList.add("tss-btn-icon-only");

            _startStopButton.Render().style.width = "24px";
            _startStopButton.Render().style.height = "24px";
            _startStopButton.Render().style.minWidth = "24px";
            _startStopButton.Render().style.padding = "0";

            var progressContainer = Stack().Horizontal().AlignItems(ItemAlign.Center).Width(100.percent()).Grow().Children(_progressIndicator.Width(100.percent()));

            _progressStack = Stack().Horizontal().AlignItems(ItemAlign.Center).JustifyContent(ItemJustify.Between).Width(100.percent()).Gap(16.px()).Children(
                progressContainer,
                _startStopButton
            );

            _mainStack = Stack().Vertical().Gap(16.px()).Padding(24.px()).Width(100.percent()).Children(
                _headerStack,
                _tasksStack,
                _footerMessageStack,
                _progressStack
            );

            _card = Card(_mainStack).NoPadding();
            _innerElement = _card.Render();
        }

        public HTMLElement Render() => _innerElement;

        public string Margin { get => _innerElement.style.margin; set => _innerElement.style.margin = value; }
        public string Padding { get => _innerElement.style.padding; set => _innerElement.style.padding = value; }

        public Plan Title(string title)
        {
            _title.Text = title;
            return this;
        }

        public Plan HeaderCommands(params IComponent[] commands)
        {
            _headerCommandsStack.Clear();
            foreach (var cmd in commands)
            {
                _headerCommandsStack.Add(cmd);
            }
            return this;
        }

        public Plan FooterMessage(string message)
        {
            _footerMessage.Text = message;
            return this;
        }

        public Plan FooterCommands(params IComponent[] commands)
        {
            _footerCommandsStack.Clear();
            foreach (var cmd in commands)
            {
                _footerCommandsStack.Add(cmd);
            }
            return this;
        }

        public Plan AddTask(string title, bool completed)
        {
            var task = new Task(title, completed);
            _tasks.Add(task);
            _tasksStack.Add(task);
            return this;
        }

        public Plan Progress(int position, int total)
        {
            _progressIndicator.Progress(position, total);
            return this;
        }

        public Plan Progress(float percent)
        {
            _progressIndicator.Progress(percent);
            return this;
        }

        public Plan Indeterminate()
        {
            _progressIndicator.Indeterminated();
            return this;
        }

        public Plan StartStopButton(Action<Button> onStartStop)
        {
            _startStopButton.OnClick((b, _) => onStartStop(b));
            return this;
        }

        public Plan HideStartStopButton()
        {
            _startStopButton.Collapse();
            return this;
        }

        public Plan ShowStartStopButton()
        {
            _startStopButton.Show();
            return this;
        }

        public Plan Start()
        {
            _startStopButton.SetIcon(UIcons.Play);
            return this;
        }

        public Plan Stop()
        {
            _startStopButton.SetIcon(UIcons.SquareSmall);
            return this;
        }

        public class Task : IComponent
        {
            private readonly Stack _taskStack;
            private readonly Icon _icon;
            private readonly TextBlock _text;

            public string Title
            {
                get => _text.Text;
                set => _text.Text = value;
            }

            public bool Completed
            {
                get => _icon.Render().dataset["icon"].As<string>().Contains(Icon(UIcons.CheckCircle).Render().dataset["icon"].As<string>());
                set
                {
                    if (value)
                    {
                        _icon.SetIcon(UIcons.CheckCircle);
                        _icon.Render().classList.add("tss-fontcolor-primary");
                    }
                    else
                    {
                        _icon.SetIcon(UIcons.Circle);
                        _icon.Render().classList.remove("tss-fontcolor-primary");
                    }
                }
            }

            public Task(string title, bool completed)
            {
                _icon = Icon(completed ? UIcons.CheckCircle : UIcons.Circle);
                if (completed)
                {
                    _icon.Render().classList.add("tss-fontcolor-primary");
                }

                _text = TextBlock(title).Regular();

                _taskStack = Stack().Horizontal().Gap(16.px()).AlignItems(ItemAlign.Center).Children(
                    _icon,
                    _text
                );
            }

            public HTMLElement Render() => _taskStack.Render();
        }
    }
}
