---
name: questionnaire
description: An inline question with choice buttons that locks once answered, showing the chosen answer highlighted. Use for a one-shot question in a transcript, form or wizard in a Tesserae (C#/Transpose) app.
---

# Questionnaire

`Questionnaire` shows a question and a row of choice buttons. Once the user picks one, it
switches to its answered state — the question with the chosen answer highlighted — and takes no
further input. Use it inline where a [`ChoiceGroup`](choice-group.md) would be too much and the
answer is not meant to be changed afterwards: a "was this helpful?" in a chat transcript, a
confirmation step in a wizard.

## Create

`UI.Questionnaire(string question, params string[] options)`
`UI.Questionnaire(string question, IEnumerable<string> options = null)`
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.OnAnswered(Action<Questionnaire>)` — raised when the user picks an answer.
- `Answer` / `IsAnswered` — what was picked, and whether anything was.
- `.SetAnswer(string)` — put the component into the answered state **without** raising
  `OnAnswered`. This is how an answer that is already known (fetched from the server, restored
  from a route) is reflected on load.
- `.ClearAnswer()` — re-enable the buttons.
- `.SetQuestion(string)`, `.AddOption(string)` / `.AddOptions(IEnumerable<string>)` — change the
  question or extend the choices after construction.

## Example

```csharp
using static Tesserae.UI;

// Asking
var feedback = Questionnaire("Was this answer helpful?", "Yes", "Partly", "No")
   .OnAnswered(q => SaveFeedbackAsync(q.Answer).FireAndForget());

// Reflecting an answer that is already known — no handler runs
var answered = Questionnaire("Was this answer helpful?", "Yes", "Partly", "No")
   .SetAnswer("Yes");
```

## Related

- ChoiceGroup — a re-selectable radio group — `choice-group.md`
- Rating — a star scale for the same kind of feedback — `rating.md`
- Chat / ToolCall — the transcript this usually sits in — `chat.md`, `tool-call.md`
- Full docs & API: `/tesserae/components/questionnaire`
