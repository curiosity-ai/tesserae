using System;
using System.Collections.Generic;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// Several <see cref="ActionCard{TData}"/> cards drawn as one: a single bordered card with a row per
    /// object, each with its identity on the left and its own actions on the right. For an answer that
    /// mentions more than one thing - a company, its contract, the dataset behind the numbers.
    /// <para>
    /// The cards keep working on their own (their actions, states and handlers are theirs); the group
    /// only draws them together and can take one <see cref="OnAction(Action{ActionCard{TData}, ActionCard{TData}.Item})"/>
    /// handler for all of them, including cards added later.
    /// </para>
    /// </summary>
    [Transpose.Name("tss.ActionCardGroupT")]
    public sealed class ActionCardGroup<TData> : ComponentBase<ActionCardGroup<TData>, HTMLElement>
    {
        private readonly List<ActionCard<TData>>                          _cards    = new List<ActionCard<TData>>();
        private readonly List<Action<ActionCard<TData>, ActionCard<TData>.Item>> _handlers = new List<Action<ActionCard<TData>, ActionCard<TData>.Item>>();

        /// <summary>
        /// Initializes a new instance of this class holding the given cards.
        /// </summary>
        public ActionCardGroup(params ActionCard<TData>[] cards)
        {
            InnerElement = Div(Att("tss-actioncard-group"));

            if (cards != null)
            {
                foreach (var card in cards) Add(card);
            }
        }

        /// <summary>
        /// Gets the cards in the group, in the order they are shown.
        /// </summary>
        public IReadOnlyList<ActionCard<TData>> Cards => _cards;

        /// <summary>
        /// Adds a card as the last row. Handlers registered on the group with
        /// <see cref="OnAction(Action{ActionCard{TData}, ActionCard{TData}.Item})"/> are attached to it.
        /// </summary>
        public ActionCardGroup<TData> Add(ActionCard<TData> card)
        {
            if (card == null || _cards.Contains(card)) return this;

            _cards.Add(card);
            InnerElement.appendChild(card.Render());

            foreach (var handler in _handlers) card.OnAction(handler);

            return this;
        }

        /// <summary>
        /// Removes a card from the group.
        /// </summary>
        public ActionCardGroup<TData> Remove(ActionCard<TData> card)
        {
            if (card == null || !_cards.Remove(card)) return this;

            InnerElement.removeChild(card.Render());
            return this;
        }

        /// <summary>
        /// Removes every card.
        /// </summary>
        public ActionCardGroup<TData> Clear()
        {
            _cards.Clear();
            ClearChildren(InnerElement);
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when an action on any card in the group is activated - the card it
        /// came from is the first argument.
        /// </summary>
        public ActionCardGroup<TData> OnAction(Action<ActionCard<TData>, ActionCard<TData>.Item> onAction)
        {
            if (onAction == null) return this;

            _handlers.Add(onAction);

            foreach (var card in _cards) card.OnAction(onAction);

            return this;
        }

        /// <summary>
        /// Registers a callback invoked with the text of an action activated on any card in the group.
        /// </summary>
        public ActionCardGroup<TData> OnAction(Action<string> onAction) => OnAction((_, a) => onAction?.Invoke(a.Text));

        /// <inheritdoc />
        public override HTMLElement Render() => InnerElement;
    }
}
