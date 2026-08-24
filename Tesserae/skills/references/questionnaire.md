---
name: questionnaire
description: An inline question with a row of answer buttons that locks itself once one is picked, showing the question with the chosen answer highlighted. Use when an assistant or a flow has to ask one thing mid-conversation in a Tesserae (C#/Transpose) app.
---

# Questionnaire

`Questionnaire` asks one question inline and offers its answers as buttons. The moment an
answer is picked — by the user, or by you — it switches to its **answered** mode: the
question stays, the chosen answer is highlighted and the buttons stop taking input, so a
transcript shows what was asked and what was decided.

That one-shot shape is what makes it a chat component rather than a form control. For a
question that stays editable use `ChoiceGroup` (`choice-group.md`).

## Create

`UI.Questionnaire(string question, params string[] options)` or
`UI.Questionnaire(string question, IEnumerable<string> options = null)`.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.OnAnswered(Action<Questionnaire>)` — the user picked an option; read `.Answer` in the
  handler. **Not** raised by `SetAnswer`, so restoring a known answer never looks like the
  user answering again.
- `.SetAnswer(string)` — put it in answered mode with that option chosen, for an answer
  that came back from the server or a reloaded transcript.
- `.ClearAnswer()` — drop the answer and re-enable the buttons.
- `.AddOption(string)` / `.AddOptions(IEnumerable<string>)` — add options after
  construction.
- `.SetQuestion(string)` — change the question.
- `.Question` / `.Answer` (null until answered) / `.IsAnswered` — read state.

## Example

```csharp
using static Tesserae.UI;

var confirm = Questionnaire("Do you want to delete this file?", "Yes", "No", "Show me the diff first")
    .OnAnswered(q => console.log("Answered: " + q.Answer));

// A turn being replayed from history arrives already answered.
var replayed = Questionnaire("Which framework should we use?", "React", "Vue", "Svelte", "Solid")
    .SetAnswer("Svelte");

chat.Add(ChatMessage(confirm).LeftAligned().MaxWidth());
```

## Related

- ChoiceGroup — the form control, when the answer stays changeable — `choice-group.md`
- Chat — the transcript this usually sits in — `chat.md`
- Dialog — when the question should block the page instead — `dialog.md`
- Rating — a score rather than a choice — `rating.md`
- Full docs & API: `/tesserae/components/questionnaire`
