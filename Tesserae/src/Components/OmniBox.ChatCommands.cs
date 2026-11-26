using System;
using System.Collections.Generic;
using System.Linq;
using Transpose;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    // Slash commands in the chat input.
    //
    // A registered command is offered while the message is a "/" at the very start of the box followed by the
    // characters typed so far: a popup lists the commands whose name matches, Arrow Up/Down move through it, Tab
    // completes the highlighted one into the box, Enter (or a click) completes a command that wants an argument and
    // runs one that does not. Sending a message whose first word is a registered command runs the command instead of
    // raising OnChat; a message that only looks like one ("/etc/hosts is missing") is sent as it always was.
    public partial class OmniBox
    {
        private const string COMMAND_SUGGESTION_CLASS = "tss-omnibox-command-suggestion";

        /// <summary>
        /// A command the chat input offers when the message starts with <c>/</c>. See <see cref="RegisterChatCommand"/>.
        /// </summary>
        public sealed class ChatCommand
        {
            /// <summary>
            /// The command as it is typed, without the leading slash: lower case letters, digits and dashes.
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// One line that says what the command does, shown beside it in the list.
            /// </summary>
            public string Description { get; }

            /// <summary>
            /// What the argument is, shown after the name in the list (for example <c>[focus]</c>). Null when the command takes none.
            /// </summary>
            public string ArgumentHint { get; }

            /// <summary>
            /// Whether the command cannot run without an argument. Picking it from the list then completes it into the box
            /// instead of running it, and sending it without one keeps the text in the box.
            /// </summary>
            public bool RequiresArgument { get; }

            /// <summary>The icon shown beside the command in the list.</summary>
            public UIcons? Icon { get; }

            /// <summary>
            /// Whether the command can be used right now; a command that answers false is left out of the list and sending it
            /// sends the text as an ordinary message. Null means always.
            /// </summary>
            public Func<bool> IsAvailable { get; }

            internal Action<ChatCommandInvocation> OnInvoked { get; }

            /// <param name="name">The command as it is typed, without the slash.</param>
            /// <param name="description">One line saying what it does.</param>
            /// <param name="onInvoked">Runs the command.</param>
            /// <param name="icon">The icon shown in the list.</param>
            /// <param name="argumentHint">What the argument is, or null for a command without one.</param>
            /// <param name="requiresArgument">Whether the command cannot run without its argument.</param>
            /// <param name="isAvailable">Whether the command can be used right now; null for always.</param>
            public ChatCommand(string name, string description, Action<ChatCommandInvocation> onInvoked, UIcons? icon = null, string argumentHint = null, bool requiresArgument = false, Func<bool> isAvailable = null)
            {
                name = (name ?? string.Empty).Trim().TrimStart('/');

                if (!IsValidName(name)) throw new ArgumentException($"'{name}' is not a valid command name: use lower case letters, digits and dashes.", nameof(name));
                if (onInvoked is null) throw new ArgumentNullException(nameof(onInvoked));

                Name             = name;
                Description      = description;
                OnInvoked        = onInvoked;
                Icon             = icon;
                ArgumentHint     = argumentHint;
                RequiresArgument = requiresArgument;
                IsAvailable      = isAvailable;
            }

            internal bool Available => IsAvailable is null || IsAvailable();

            private static bool IsValidName(string name)
            {
                if (string.IsNullOrEmpty(name)) return false;

                foreach (var ch in name)
                {
                    if (!((ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9') || ch == '-')) return false;
                }

                return true;
            }
        }

        /// <summary>
        /// One use of a <see cref="ChatCommand"/>: what was typed and what the command was given.
        /// </summary>
        public sealed class ChatCommandInvocation
        {
            internal ChatCommandInvocation(OmniBox sender, ChatCommand command, string arguments, string text)
            {
                Sender    = sender;
                Command   = command;
                Arguments = arguments;
                Text      = text;
            }

            /// <summary>The box the command was typed in.</summary>
            public OmniBox Sender { get; }

            /// <summary>The command that runs.</summary>
            public ChatCommand Command { get; }

            /// <summary>Everything after the command's name, trimmed; empty when nothing was typed.</summary>
            public string Arguments { get; }

            /// <summary>The whole message as it was typed, slash included.</summary>
            public string Text { get; }
        }

        private readonly List<ChatCommand> _chatCommands = new List<ChatCommand>();
        private Action                     _hideCommandSuggestions;
        private List<Button>               _currentCommandButtons     = new List<Button>();
        private ChatCommand[]              _currentCommandMatches     = Array.Empty<ChatCommand>();
        private int                        _highlightedCommandIndex   = -1;

        /// <summary>
        /// The commands the chat input offers, in the order they were registered.
        /// </summary>
        public IReadOnlyList<ChatCommand> ChatCommands => _chatCommands;

        /// <summary>
        /// Adds a slash command to the chat input (chat and search-and-chat modes only). Typing <c>/</c> as the first character
        /// of the message lists the registered commands and filters them as the name is typed; sending a message that starts
        /// with a command's name runs it instead of raising <see cref="OnChat"/>. A command registered under a name that is
        /// already taken replaces the earlier one.
        /// </summary>
        public OmniBox RegisterChatCommand(ChatCommand command)
        {
            if (command is null) throw new ArgumentNullException(nameof(command));

            if (_mode != Mode.Chat && _mode != Mode.SearchAndChat)
            {
                throw new InvalidOperationException("RegisterChatCommand can only be called when OmniBox is in Chat or SearchAndChat mode.");
            }

            _chatCommands.RemoveAll(c => c.Name == command.Name);
            _chatCommands.Add(command);

            return this;
        }

        /// <summary>Adds several slash commands at once - see <see cref="RegisterChatCommand"/>.</summary>
        public OmniBox RegisterChatCommands(params ChatCommand[] commands)
        {
            if (commands is null) return this;

            foreach (var command in commands) RegisterChatCommand(command);

            return this;
        }

        /// <summary>Removes the slash command registered under <paramref name="name"/> (with or without the slash).</summary>
        public OmniBox RemoveChatCommand(string name)
        {
            name = (name ?? string.Empty).Trim().TrimStart('/');
            _chatCommands.RemoveAll(c => c.Name == name);
            HideCommandSuggestions();

            return this;
        }

        /// <summary>Removes every slash command.</summary>
        public OmniBox ClearChatCommands()
        {
            _chatCommands.Clear();
            HideCommandSuggestions();

            return this;
        }

        private bool IsShowingCommandSuggestions => _hideCommandSuggestions is object && _currentCommandButtons.Count > 0;

        // The name being typed: the text between the leading slash and the caret, when the caret is still in the first word of the box.
        private bool TryGetTypedCommandName(HTMLTextAreaElement input, out string typed)
        {
            typed = null;

            if (_chatCommands.Count == 0 || input != _chatInput) return false;

            var val    = input.value ?? string.Empty;
            var cursor = (int)input.selectionStart;

            if (val.Length == 0 || val[0] != '/' || cursor < 1) return false;

            for (var i = 1; i < cursor; i++)
            {
                if (char.IsWhiteSpace(val[i])) return false;
            }

            typed = val.Substring(1, cursor - 1);

            return true;
        }

        private void TryUpdateChatCommands(HTMLTextAreaElement input)
        {
            if (!TryGetTypedCommandName(input, out var typed))
            {
                HideCommandSuggestions();
                return;
            }

            var matches = _chatCommands
               .Where(c => c.Available && c.Name.IndexOf(typed, StringComparison.OrdinalIgnoreCase) >= 0)
               .OrderBy(c => c.Name.StartsWith(typed, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
               .ToArray();

            if (matches.Length == 0)
            {
                HideCommandSuggestions();
                return;
            }

            ShowCommandSuggestions(matches);
        }

        private void ShowCommandSuggestions(ChatCommand[] matches)
        {
            var previous = _highlightedCommandIndex >= 0 && _highlightedCommandIndex < _currentCommandMatches.Length ? _currentCommandMatches[_highlightedCommandIndex] : null;

            _currentCommandMatches = matches;
            _currentCommandButtons = new List<Button>();

            var list = VStack().NoDefaultMargin().W(360).MaxHeight(360.px()).ScrollY().PT(4);

            foreach (var command in matches)
            {
                var captured = command;
                var btn      = Button().Class("tss-omnibox-snap-suggestion").Class(COMMAND_SUGGESTION_CLASS).WS().NoPadding().NoMinSize().H(40).MB(4).NoBorder().NoBackground().TextLeft();
                var row      = HStack().WS().AlignItems(ItemAlign.Center).PaddingLeft(12.px()).PaddingRight(12.px());

                if (captured.Icon.HasValue) row.Add(Icon(captured.Icon.Value).MR(8));

                var name = HStack().NoDefaultMargin().AlignItems(ItemAlign.Baseline).Children(TextBlock("/" + captured.Name).SemiBold());

                if (!string.IsNullOrEmpty(captured.ArgumentHint)) name.Add(TextBlock(captured.ArgumentHint).Small().Secondary().ML(6));

                var labels = VStack().NoDefaultMargin().W(10).Grow().Children(name);

                if (!string.IsNullOrEmpty(captured.Description)) labels.Add(TextBlock(captured.Description).Small().Secondary().Ellipsis());

                row.Add(labels);

                btn.ReplaceContent(row);
                btn.OnClick(() => PickChatCommand(captured, run: true));

                list.Add(btn);
                _currentCommandButtons.Add(btn);
            }

            if (_hideCommandSuggestions != null)
            {
                _hideCommandSuggestions();
                _hideCommandSuggestions = null;
            }

            // The chat box usually sits at the bottom of its page, so the list opens upwards (Tippy flips it when there is no room).
            Tippy.ShowFor(_chatInput, list.Render(), out _hideCommandSuggestions, placement: TooltipPlacement.TopStart, maxWidth: 400, onHiddenCallback: () => _hideCommandSuggestions = null);

            var keep = previous is object ? Array.IndexOf(matches, previous) : -1;
            _highlightedCommandIndex = -1;
            HighlightCommand(keep >= 0 ? keep : 0);
        }

        private void HideCommandSuggestions()
        {
            if (_hideCommandSuggestions != null)
            {
                _hideCommandSuggestions();
                _hideCommandSuggestions = null;
            }

            _currentCommandButtons.Clear();
            _currentCommandMatches   = Array.Empty<ChatCommand>();
            _highlightedCommandIndex = -1;
        }

        private void HighlightCommand(int index)
        {
            if (_highlightedCommandIndex >= 0 && _highlightedCommandIndex < _currentCommandButtons.Count)
            {
                _currentCommandButtons[_highlightedCommandIndex].RemoveClass("tss-omnibox-suggestion-highlight");
            }

            _highlightedCommandIndex = index;

            if (_highlightedCommandIndex >= 0 && _highlightedCommandIndex < _currentCommandButtons.Count)
            {
                _currentCommandButtons[_highlightedCommandIndex].Class("tss-omnibox-suggestion-highlight").ScrollIntoView();
            }
        }

        // Keys while the list is open. True when the key was taken.
        private bool TryHandleChatCommandKey(HTMLTextAreaElement input, KeyboardEvent ke)
        {
            if (input != _chatInput || !IsShowingCommandSuggestions) return false;
            if (ke.ctrlKey || ke.metaKey || ke.altKey) return false;

            switch (ke.key)
            {
                case "ArrowDown":
                    HighlightCommand((_highlightedCommandIndex + 1) % _currentCommandButtons.Count);
                    return true;

                case "ArrowUp":
                    HighlightCommand((_highlightedCommandIndex - 1 + _currentCommandButtons.Count) % _currentCommandButtons.Count);
                    return true;

                case "Tab":
                    if (ke.shiftKey) return false;
                    if (_highlightedCommandIndex < 0 || _highlightedCommandIndex >= _currentCommandMatches.Length) return false;
                    PickChatCommand(_currentCommandMatches[_highlightedCommandIndex], run: false);
                    return true;

                case "Enter":
                    if (ke.shiftKey) return false;
                    if (_highlightedCommandIndex < 0 || _highlightedCommandIndex >= _currentCommandMatches.Length) return false;
                    PickChatCommand(_currentCommandMatches[_highlightedCommandIndex], run: true);
                    return true;

                case "Escape":
                    HideCommandSuggestions();
                    return true;
            }

            return false;
        }

        // Replaces the name being typed with the picked command. A command that wants an argument (or Tab, which only completes)
        // stays in the box with the caret after it; any other one runs straight away.
        private void PickChatCommand(ChatCommand command, bool run)
        {
            HideCommandSuggestions();

            var val      = _chatInput.value ?? string.Empty;
            var nameEnd  = 1;

            while (nameEnd < val.Length && !char.IsWhiteSpace(val[nameEnd])) nameEnd++;

            var rest      = val.Substring(nameEnd).TrimStart();
            var completed = "/" + command.Name + " " + rest;

            if (run && !command.RequiresArgument)
            {
                RunChatCommand(command, rest.Trim(), completed.Trim());
                return;
            }

            _chatInput.value = completed;

            var caret = (uint)(command.Name.Length + 2);
            _chatInput.setSelectionRange(caret, caret);

            UpdateChatTriggerActiveState();
            UpdateChatCounter();
            ResizeChatInput();
            _chatInput.focus();
        }

        // Called on send. True when the message was a command that ran (or one that still needs its argument): the caller sends nothing.
        private bool TryRunChatCommandFromText(string text)
        {
            if (_chatCommands.Count == 0 || string.IsNullOrEmpty(text) || text[0] != '/') return false;

            var nameEnd = 1;

            while (nameEnd < text.Length && !char.IsWhiteSpace(text[nameEnd])) nameEnd++;

            var name    = text.Substring(1, nameEnd - 1);
            var command = _chatCommands.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));

            if (command is null || !command.Available) return false;

            var arguments = text.Substring(nameEnd).Trim();

            if (command.RequiresArgument && arguments.Length == 0)
            {
                // Nothing to run it with: leave it in the box for the argument, rather than send "/plan" to the assistant.
                _chatInput.focus();
                return true;
            }

            RunChatCommand(command, arguments, text.Trim());

            return true;
        }

        private void RunChatCommand(ChatCommand command, string arguments, string text)
        {
            var hadFocus = ChatHasFocus();

            HideCommandSuggestions();
            AddToChatHistory(text);
            SetChatSegmentsFromText("");

            if (hadFocus) _chatInput.focus();

            command.OnInvoked(new ChatCommandInvocation(this, command, arguments, text));
        }
    }
}
