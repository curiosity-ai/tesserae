using System;
using System.Collections.Generic;
using System.Linq;
using Transpose;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    // Code blocks in the chat input.
    //
    // The chat input is a run of segments that alternate prose / code / prose / ..., always starting and ending with
    // prose: the first prose segment is _chatInput itself, so a message without code is exactly the one textarea it
    // always was. Typing ``` at the start of a line opens a code box after the text before it (and moves whatever was
    // after the caret below the box); ``` typed on the last line of a box closes it again; Backspace in an empty box
    // removes it. Text that arrives with fences in it (a paste, ChatText, the history walk, dictation) is parsed into
    // the same segments.
    //
    // ChatText is the segments composed back into Markdown: each box is fenced with one backtick more than the longest
    // run of backticks in its content (and never fewer than three), so a box holding ``` itself still round-trips.
    public partial class OmniBox
    {
        private const string CODE_BLOCK_CLASS       = "tss-omnibox-code-block";
        private const string CODE_INPUT_CLASS       = "tss-omnibox-code-input";
        private const string CODE_LANG_CLASS        = "tss-omnibox-code-lang";
        private const string PROSE_CONTINUED_CLASS  = "tss-omnibox-chat-input-continued";
        private const string HAS_CODE_CLASS         = "tss-omnibox-has-code";

        private sealed class ChatSegment
        {
            public bool                IsCode;
            public string              Language;
            public HTMLTextAreaElement Input;
            public HTMLElement         Root;
        }

        /// <summary>One run of a chat message: prose, or the content of a fenced code block.</summary>
        public sealed class ChatPart
        {
            /// <summary>True for the content of a fenced code block, false for the text around it.</summary>
            public bool   IsCode   { get; set; }
            /// <summary>The info string after the opening fence (e.g. "csharp"), or null.</summary>
            public string Language { get; set; }
            /// <summary>The text, without the fence lines.</summary>
            public string Text     { get; set; }
            /// <summary>False for a code block whose closing fence never came (it ran to the end of the text).</summary>
            public bool   Closed   { get; set; } = true;
        }

        private readonly List<ChatSegment> _chatSegments = new List<ChatSegment>();
        private bool                       _codeBlocksEnabled;
        private HTMLTextAreaElement        _lastChatInput;
        private string                     _chatPlaceholder = string.Empty;
        private int?                       _chatTabIndex;

        private bool HasCodeBlocks => _chatSegments.Count > 1;

        private IEnumerable<HTMLTextAreaElement> ChatTextAreas => _chatSegments.Count > 0 ? _chatSegments.Select(s => s.Input) : (_chatInput is object ? new[] { _chatInput } : Array.Empty<HTMLTextAreaElement>());

        /// <summary>
        /// Splits <paramref name="text"/> into prose and fenced code blocks, as the chat input does with text that
        /// arrives with fences in it. The result alternates prose and code, starting and ending with prose (which can
        /// be empty). A fence is three or more backticks at the start of a line (up to three spaces in), optionally
        /// followed by a language; it is closed by a line of at least as many backticks, or by the end of the text.
        /// </summary>
        public static List<ChatPart> ParseChatParts(string text)
        {
            var parts = new List<ChatPart>();
            var lines = (text ?? string.Empty).Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
            var prose = new List<string>();
            var i     = 0;

            while (i < lines.Length)
            {
                if (TryReadOpeningFence(lines[i], out var fenceLength, out var language))
                {
                    parts.Add(new ChatPart { Text = string.Join("\n", prose) });
                    prose.Clear();

                    var code   = new List<string>();
                    var closed = false;
                    i++;

                    while (i < lines.Length)
                    {
                        if (IsClosingFence(lines[i], fenceLength))
                        {
                            closed = true;
                            i++;
                            break;
                        }

                        code.Add(lines[i]);
                        i++;
                    }

                    parts.Add(new ChatPart { IsCode = true, Language = language, Text = string.Join("\n", code), Closed = closed });
                    continue;
                }

                prose.Add(lines[i]);
                i++;
            }

            parts.Add(new ChatPart { Text = string.Join("\n", prose) });
            return parts;
        }

        /// <summary>
        /// Writes <paramref name="parts"/> back as Markdown, the way the chat input composes <see cref="ChatText"/>:
        /// empty prose and blank code blocks are left out, and every code block is fenced with <see cref="FenceFor"/>.
        /// </summary>
        public static string ComposeChatParts(IEnumerable<ChatPart> parts)
        {
            var pieces = new List<string>();

            foreach (var part in parts)
            {
                if (part.IsCode)
                {
                    if (string.IsNullOrWhiteSpace(part.Text)) continue;
                    var fence = FenceFor(part.Text);
                    pieces.Add(fence + (part.Language ?? string.Empty) + "\n" + part.Text + "\n" + fence);
                }
                else if (!string.IsNullOrEmpty(part.Text))
                {
                    pieces.Add(part.Text);
                }
            }

            return string.Join("\n", pieces);
        }

        /// <summary>
        /// The fence for a code block holding <paramref name="code"/>: one backtick more than the longest run of
        /// backticks anywhere in it, and never fewer than three, so nothing inside can close the block early.
        /// </summary>
        public static string FenceFor(string code)
        {
            int longest = 0, run = 0;

            foreach (var c in code ?? string.Empty)
            {
                if (c == '`')
                {
                    run++;
                    if (run > longest) longest = run;
                }
                else
                {
                    run = 0;
                }
            }

            return new string('`', Math.Max(3, longest + 1));
        }

        private static int LeadingSpaces(string line)
        {
            int n = 0;
            while (n < line.Length && line[n] == ' ') n++;
            return n;
        }

        private static bool TryReadOpeningFence(string line, out int fenceLength, out string language)
        {
            fenceLength = 0;
            language    = null;

            var indent = LeadingSpaces(line);
            if (indent > 3) return false;

            var n = indent;
            while (n < line.Length && line[n] == '`') n++;

            fenceLength = n - indent;
            if (fenceLength < 3) return false;

            var info = line.Substring(n);
            if (info.IndexOf('`') >= 0) return false; //Not a fence: an inline code span like ```a``` on its own line

            info     = info.Trim();
            language = info.Length > 0 ? info : null;
            return true;
        }

        private static bool IsClosingFence(string line, int fenceLength)
        {
            var indent = LeadingSpaces(line);
            if (indent > 3) return false;

            var n = indent;
            while (n < line.Length && line[n] == '`') n++;

            return n - indent >= fenceLength && line.Substring(n).Trim().Length == 0;
        }

        // ---------- Composing and rebuilding ----------

        private string ComposeChatText()
        {
            if (_chatSegments.Count <= 1) return _chatInput.value;

            return ComposeChatParts(_chatSegments.Select(s => new ChatPart { IsCode = s.IsCode, Language = s.Language, Text = s.Input.value ?? string.Empty }));
        }

        // Replaces everything in the chat input with text, fences parsed into boxes.
        private void SetChatSegmentsFromText(string text)
        {
            var hadFocus = ChatHasFocus();

            for (int i = _chatSegments.Count - 1; i >= 1; i--)
            {
                _chatSegments[i].Root.remove();
                _chatSegments.RemoveAt(i);
            }

            _lastChatInput = _chatInput;

            if (!_codeBlocksEnabled)
            {
                _chatInput.value = text;
                OnChatStructureChanged();
                return;
            }

            var parts = ParseChatParts(text);
            _chatInput.value = parts[0].Text;

            if (parts.Count > 1)
            {
                InsertSegments(1, parts.Skip(1).Select(CreateSegment).ToList());
            }

            OnChatStructureChanged();

            if (hadFocus) FocusSegment(_chatSegments[_chatSegments.Count - 1], atEnd: true);
        }

        private bool ChatHasFocus()
        {
            var active = document.activeElement;
            return active is object && _chatContainer is object && _chatContainer.contains(active);
        }

        private ChatSegment CreateSegment(ChatPart part) => part.IsCode ? CreateCodeSegment(part.Language, part.Text) : CreateProseSegment(part.Text);

        private ChatSegment CreateProseSegment(string text)
        {
            var input = UI.TextArea(Att("tss-omnibox-chat-input " + PROSE_CONTINUED_CLASS, type: "text"));
            input.setAttribute("rows", "1");
            input.spellcheck = true;
            input.value      = text ?? string.Empty;

            var segment = new ChatSegment { Input = input, Root = input };
            WireChatProse(input);
            ApplyChatInputState(input);
            return segment;
        }

        private ChatSegment CreateCodeSegment(string language, string text)
        {
            var input = UI.TextArea(Att(CODE_INPUT_CLASS, type: "text", ariaLabel: string.IsNullOrEmpty(language) ? "Code block" : "Code block, " + language));
            input.setAttribute("rows", "1");
            input.setAttribute("autocapitalize", "off");
            input.setAttribute("autocomplete", "off");
            input.setAttribute("autocorrect", "off");
            input.spellcheck = false;
            input.value      = text ?? string.Empty;

            var root = Div(Att(CODE_BLOCK_CLASS));

            if (!string.IsNullOrWhiteSpace(language))
            {
                root.appendChild(Span(Att(CODE_LANG_CLASS, text: language)));
            }

            root.appendChild(input);

            var segment = new ChatSegment { IsCode = true, Language = language, Input = input, Root = root };
            WireChatCode(segment);
            ApplyChatInputState(input);
            return segment;
        }

        private void InsertSegments(int index, IList<ChatSegment> segments)
        {
            var anchor = _chatSegments[index - 1].Root;

            for (int i = 0; i < segments.Count; i++)
            {
                anchor.insertAdjacentElement(InsertPosition.afterend, segments[i].Root);
                anchor = segments[i].Root;
                _chatSegments.Insert(index + i, segments[i]);
            }
        }

        private void ApplyChatInputState(HTMLTextAreaElement input)
        {
            if (_chatTabIndex.HasValue) input.tabIndex = _chatTabIndex.Value;
            if (_container is object) input.disabled = !IsEnabled;
        }

        // After anything that adds or removes a box
        private void OnChatStructureChanged()
        {
            if (_chatContainer is object)
            {
                if (HasCodeBlocks) _chatContainer.classList.add(HAS_CODE_CLASS);
                else               _chatContainer.classList.remove(HAS_CODE_CLASS);
            }

            if (_lastChatInput is null || !_chatSegments.Any(s => s.Input == _lastChatInput)) _lastChatInput = _chatInput;

            UpdateChatPlaceholder();
            UpdateChatTriggerActiveState();
            UpdateChatCounter();
            ResizeChatInput();
        }

        // The placeholder is the empty box's prompt: with a code block in it the box is not empty, and the prompt
        // above the block would read as a second, blank message.
        private void UpdateChatPlaceholder()
        {
            if (_chatInput is null) return;
            _chatInput.placeholder = HasCodeBlocks ? string.Empty : (_chatPlaceholder ?? string.Empty);
        }

        // The character budget is the whole message, fences included: every textarea may grow by what is left of it.
        private void ApplyChatBudget()
        {
            if (_chatInput is null) return;

            if (_maxChatCharacters <= 0)
            {
                foreach (var t in ChatTextAreas) t.removeAttribute("maxlength");
                return;
            }

            var remaining = Math.Max(0, _maxChatCharacters - (ComposeChatText() ?? string.Empty).Length);

            foreach (var t in ChatTextAreas)
            {
                t.maxLength = (t.value ?? string.Empty).Length + remaining;
            }
        }

        private static void FocusTextArea(HTMLTextAreaElement input, int caret)
        {
            input.focus();
            var at = (uint)Math.Max(0, Math.Min(caret, (input.value ?? string.Empty).Length));
            input.setSelectionRange(at, at);
        }

        private void FocusSegment(ChatSegment segment, bool atEnd)
        {
            FocusTextArea(segment.Input, atEnd ? (segment.Input.value ?? string.Empty).Length : 0);
        }

        // Takes the box at index out, joining the prose on either side of it; the caret lands where the box was.
        private void RemoveCodeSegment(int index)
        {
            var code   = _chatSegments[index];
            var before = _chatSegments[index - 1];
            var after  = _chatSegments[index + 1];

            var head = before.Input.value ?? string.Empty;
            var tail = after.Input.value ?? string.Empty;

            before.Input.value = tail.Length == 0 ? head : head.Length == 0 ? tail : head + "\n" + tail;

            code.Root.remove();
            after.Root.remove();
            _chatSegments.RemoveAt(index + 1);
            _chatSegments.RemoveAt(index);

            OnChatStructureChanged();
            FocusTextArea(before.Input, head.Length);
            Input?.Invoke(this, null);
        }

        // ---------- Typing a fence ----------

        //True when the caret sits just after ``` that starts its line (up to three spaces in), with nothing selected.
        private static bool TypedFenceAtCaret(HTMLTextAreaElement input, out int lineStart, out int caret)
        {
            var value = input.value ?? string.Empty;
            caret     = (int)input.selectionStart;
            lineStart = 0;

            if (caret != (int)input.selectionEnd || caret < 3 || caret > value.Length) return false;
            if (value.Substring(caret - 3, 3) != "```") return false;

            lineStart = caret - 3 == 0 ? 0 : value.LastIndexOf('\n', caret - 4) + 1;
            var indent = value.Substring(lineStart, caret - 3 - lineStart);

            return indent.Length <= 3 && indent.Trim().Length == 0;
        }

        private static bool IsTypedText(Event e) => Script.Write<string>("({0} && {0}.inputType) || ''", e) == "insertText";

        // A prose segment changed: ``` just typed opens a box at the caret, and text that came in with fences in it
        // (a paste, an autocorrect, a drop) is split into boxes. True when the segments changed.
        private bool TrySplitProse(HTMLTextAreaElement input, Event e)
        {
            if (!_codeBlocksEnabled) return false;

            var index = _chatSegments.FindIndex(s => s.Input == input);
            if (index < 0) return false;

            var value = input.value ?? string.Empty;

            if (IsTypedText(e) && TypedFenceAtCaret(input, out var lineStart, out var caret))
            {
                var before = value.Substring(0, lineStart);
                if (before.EndsWith("\n")) before = before.Substring(0, before.Length - 1);

                var after = value.Substring(caret);
                if (after.StartsWith("\n")) after = after.Substring(1);

                input.value = before;

                var code = CreateCodeSegment(null, string.Empty);
                InsertSegments(index + 1, new[] { code, CreateProseSegment(after) });

                OnChatStructureChanged();
                FocusSegment(code, atEnd: false);
                return true;
            }

            if (value.IndexOf("```") < 0) return false;

            var parts = ParseChatParts(value);
            if (parts.Count == 1) return false;

            input.value = parts[0].Text;

            var created = parts.Skip(1).Select(CreateSegment).ToList();
            InsertSegments(index + 1, created);

            OnChatStructureChanged();

            // A box that ran to the end of what was pasted is still being written; otherwise carry on after the last one
            var lastCode = parts[parts.Count - 2];
            if (!lastCode.Closed) FocusSegment(created[created.Count - 2], atEnd: true);
            else                  FocusSegment(created[created.Count - 1], atEnd: false);

            return true;
        }

        // ``` typed on the last line of a box closes it: the fence line goes, and the caret moves to the text below.
        // Anywhere else in the box the backticks are just code (and the composed fence grows to fit them).
        private bool TryCloseCodeFromTyping(ChatSegment segment, Event e)
        {
            if (!IsTypedText(e) || !TypedFenceAtCaret(segment.Input, out var lineStart, out var caret)) return false;

            var value = segment.Input.value ?? string.Empty;
            if (value.Substring(caret).Trim().Length > 0) return false;

            var content = value.Substring(0, lineStart);
            if (content.EndsWith("\n")) content = content.Substring(0, content.Length - 1);
            segment.Input.value = content;

            var index = _chatSegments.IndexOf(segment);
            OnChatStructureChanged();
            FocusSegment(_chatSegments[index + 1], atEnd: false);
            return true;
        }

        // ---------- Keys shared by every segment ----------

        private static bool HasModifier(KeyboardEvent ke) => ke.shiftKey || ke.altKey || ke.ctrlKey || ke.metaKey;

        private static bool CaretAtStart(HTMLTextAreaElement input) => input.selectionStart == 0 && input.selectionEnd == 0;

        private static bool CaretAtEnd(HTMLTextAreaElement input)
        {
            var length = (uint)(input.value ?? string.Empty).Length;
            return input.selectionStart == length && input.selectionEnd == length;
        }

        // Moves between segments when the caret runs off the start or end of one. True when the key was taken.
        private bool TryCrossSegment(HTMLTextAreaElement input, KeyboardEvent ke)
        {
            if (_chatSegments.Count <= 1 || HasModifier(ke) || ke.defaultPrevented) return false;

            var index = _chatSegments.FindIndex(s => s.Input == input);
            if (index < 0) return false;

            var previous = index > 0 ? _chatSegments[index - 1] : null;
            var next     = index + 1 < _chatSegments.Count ? _chatSegments[index + 1] : null;

            switch (ke.key)
            {
                case "ArrowUp":
                case "ArrowLeft":
                    if (previous is null || !CaretAtStart(input)) return false;
                    FocusSegment(previous, atEnd: true);
                    return true;

                case "ArrowDown":
                case "ArrowRight":
                    if (next is null || !CaretAtEnd(input)) return false;
                    FocusSegment(next, atEnd: false);
                    return true;

                case "Backspace":
                    // At the start of the text after a box: an empty box goes, a full one takes the caret
                    if (previous is null || !previous.IsCode || !CaretAtStart(input)) return false;
                    if (string.IsNullOrEmpty(previous.Input.value)) RemoveCodeSegment(index - 1);
                    else FocusSegment(previous, atEnd: true);
                    return true;

                case "Delete":
                    if (next is null || !next.IsCode || !CaretAtEnd(input) || !string.IsNullOrEmpty(next.Input.value)) return false;
                    RemoveCodeSegment(index + 1);
                    return true;
            }

            return false;
        }

        private void WireChatCommon(HTMLTextAreaElement input)
        {
            input.addEventListener("keyup", (e) => KeyUp?.Invoke(this, e.As<KeyboardEvent>()));
            input.addEventListener("keypress", (e) => KeyPress?.Invoke(this, e.As<KeyboardEvent>()));
            input.addEventListener("focus", (e) =>
            {
                _lastChatInput = input;
                ReceivedFocus?.Invoke(this, e);
            });
            input.addEventListener("blur", (e) => LostFocus?.Invoke(this, e));
            input.addEventListener("paste", (e) => OnChatPaste(e));
        }

        // The first prose segment is _chatInput; every one after a box is wired the same way.
        private void WireChatProse(HTMLTextAreaElement input)
        {
            input.addEventListener("keydown", (e) =>
            {
                var ke = e.As<KeyboardEvent>();
                KeyDown?.Invoke(this, ke);

                if (_chatMention != null && _mentionStart >= 0 && _mentionInput == input && (_chatMention.IsOpen?.Invoke() ?? false))
                {
                    if (ke.key == "ArrowDown")
                    {
                        StopEvent(e);
                        _chatMention.OnMove?.Invoke(1);
                        return;
                    }
                    if (ke.key == "ArrowUp")
                    {
                        StopEvent(e);
                        _chatMention.OnMove?.Invoke(-1);
                        return;
                    }
                    if (ke.key == "Enter" || ke.key == "Tab")
                    {
                        StopEvent(e);
                        if (_chatMention.OnCommit?.Invoke() ?? false)
                        {
                            CommitChatMention();
                        }
                        return;
                    }
                    if (ke.key == "Escape")
                    {
                        StopEvent(e);
                        HideChatMention();
                        return;
                    }
                }

                if ((ke.key == "ArrowUp" || ke.key == "ArrowDown") && TryWalkChatHistory(ke, older: ke.key == "ArrowUp"))
                {
                    StopEvent(e);
                    return;
                }

                if (TryCrossSegment(input, ke))
                {
                    StopEvent(e);
                    return;
                }

                if (ke.key == "Enter" && !ke.shiftKey)
                {
                    TriggerChat();
                    StopEvent(e);
                }
            });

            input.addEventListener("input", (e) =>
            {
                TrySplitProse(input, e);
                UpdateChatTriggerActiveState();
                UpdateChatCounter();
                ResizeChatInput();
                Input?.Invoke(this, e);
                if (document.activeElement == input) TryUpdateChatMention(input);
                else if (_mentionStart >= 0) HideChatMention();
            });

            WireChatCommon(input);
        }

        private void OnChatPaste(Event e)
        {
            //Nothing subscribed means the browser's own paste, unchanged - reading the
            //clipboard to hand nobody the result would only cost the paste a frame.
            if (ChatPasted is null) return;

            var clipboard = e.As<ClipboardEvent>();

            var paste = new ChatPaste
            {
                Text  = clipboard?.clipboardData?.getData("text/plain") ?? string.Empty,
                Files = FilesOnClipboard(clipboard)
            };

            ChatPasted.Invoke(this, paste);

            //Only a handler that says it took the paste keeps it out of the input.
            if (paste.Handled) StopEvent(e);
        }

        private void WireChatCode(ChatSegment segment)
        {
            var input = segment.Input;

            input.addEventListener("keydown", (e) =>
            {
                var ke = e.As<KeyboardEvent>();
                KeyDown?.Invoke(this, ke);

                // Enter is a new line of code; Ctrl/Cmd+Enter sends
                if (ke.key == "Enter" && (ke.ctrlKey || ke.metaKey))
                {
                    StopEvent(e);
                    TriggerChat();
                    return;
                }

                if (ke.key == "Backspace" && !HasModifier(ke) && string.IsNullOrEmpty(input.value))
                {
                    StopEvent(e);
                    RemoveCodeSegment(_chatSegments.IndexOf(segment));
                    return;
                }

                if (ke.key == "Escape" && !HasModifier(ke))
                {
                    StopEvent(e);
                    FocusSegment(_chatSegments[_chatSegments.IndexOf(segment) + 1], atEnd: false);
                    return;
                }

                if (ke.key != "Backspace" && ke.key != "Delete" && TryCrossSegment(input, ke))
                {
                    StopEvent(e);
                }
            });

            input.addEventListener("input", (e) =>
            {
                TryCloseCodeFromTyping(segment, e);
                UpdateChatTriggerActiveState();
                UpdateChatCounter();
                ResizeChatInput();
                Input?.Invoke(this, e);
            });

            WireChatCommon(input);
        }
    }
}
