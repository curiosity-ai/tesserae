using System;
using System.Collections.Generic;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// Several <see cref="RelatedQuestions"/> cards drawn as one: a single bordered card with a row per
    /// object, each with its identity on the left and its own questions on the right. For an answer that
    /// mentions more than one thing - a company, its contract, the dataset behind the numbers.
    /// <para>
    /// The cards keep working on their own (their questions, states and handlers are theirs); the group
    /// only draws them together and can take one <see cref="OnAsk(Action{RelatedQuestions, RelatedQuestions.Question})"/>
    /// handler for all of them, including cards added later.
    /// </para>
    /// </summary>
    [Transpose.Name("tss.RelatedQuestionsGroup")]
    public sealed class RelatedQuestionsGroup : ComponentBase<RelatedQuestionsGroup, HTMLElement>
    {
        private readonly List<RelatedQuestions>                            _cards    = new List<RelatedQuestions>();
        private readonly List<Action<RelatedQuestions, RelatedQuestions.Question>> _handlers = new List<Action<RelatedQuestions, RelatedQuestions.Question>>();

        /// <summary>
        /// Initializes a new instance of this class holding the given cards.
        /// </summary>
        public RelatedQuestionsGroup(params RelatedQuestions[] cards)
        {
            InnerElement = Div(Att("tss-relatedquestions-group"));

            if (cards != null)
            {
                foreach (var card in cards) Add(card);
            }
        }

        /// <summary>
        /// Gets the cards in the group, in the order they are shown.
        /// </summary>
        public IReadOnlyList<RelatedQuestions> Cards => _cards;

        /// <summary>
        /// Adds a card as the last row. Handlers registered on the group with
        /// <see cref="OnAsk(Action{RelatedQuestions, RelatedQuestions.Question})"/> are attached to it.
        /// </summary>
        public RelatedQuestionsGroup Add(RelatedQuestions card)
        {
            if (card == null || _cards.Contains(card)) return this;

            _cards.Add(card);
            InnerElement.appendChild(card.Render());

            foreach (var handler in _handlers) card.OnAsk(handler);

            return this;
        }

        /// <summary>
        /// Removes a card from the group.
        /// </summary>
        public RelatedQuestionsGroup Remove(RelatedQuestions card)
        {
            if (card == null || !_cards.Remove(card)) return this;

            InnerElement.removeChild(card.Render());
            return this;
        }

        /// <summary>
        /// Removes every card.
        /// </summary>
        public RelatedQuestionsGroup Clear()
        {
            _cards.Clear();
            ClearChildren(InnerElement);
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when a question on any card in the group is asked - the card it
        /// came from is the first argument.
        /// </summary>
        public RelatedQuestionsGroup OnAsk(Action<RelatedQuestions, RelatedQuestions.Question> onAsk)
        {
            if (onAsk == null) return this;

            _handlers.Add(onAsk);

            foreach (var card in _cards) card.OnAsk(onAsk);

            return this;
        }

        /// <summary>
        /// Registers a callback invoked with the text of a question asked on any card in the group.
        /// </summary>
        public RelatedQuestionsGroup OnAsk(Action<string> onAsk) => OnAsk((_, q) => onAsk?.Invoke(q.Text));

        /// <inheritdoc />
        public override HTMLElement Render() => InnerElement;
    }
}
