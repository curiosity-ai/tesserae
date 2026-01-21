using System;
using static Tesserae.UI;
using static H5.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// Represents an item in a <see cref="TextBreadcrumbs"/> component.
    /// </summary>
    [H5.Name("tss.TextBreadcrumb")]
    public class TextBreadcrumb : ComponentBase<TextBreadcrumb, HTMLSpanElement>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextBreadcrumb"/> class.
        /// </summary>
        /// <param name="text">The breadcrumb text.</param>
        public TextBreadcrumb(string text)
        {
            InnerElement = Span(_("tss-textbreadcrumb", text: text));

            AttachClick();
        }
        /// <summary>
        /// Renders the component.
        /// </summary>
        /// <returns>The rendered HTML element.</returns>
        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }

    /// <summary>
    /// A breadcrumb component that displays a path as a sequence of text links.
    /// </summary>
    [H5.Name("tss.TextBreadcrumbs")]
    public class TextBreadcrumbs : IComponent, IContainer<TextBreadcrumbs, TextBreadcrumb>, ITextFormating, IHasForegroundColor
    {
        private readonly HTMLElement InnerElement;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextBreadcrumbs"/> class.
        /// </summary>
        public TextBreadcrumbs()
        {
            InnerElement = Div(_("tss-textbreadcrumb-container tss-fontsize-small tss-fontweight-regular"));
        }

        /// <summary>
        /// Clears all breadcrumbs.
        /// </summary>
        public void Clear()
        {
            ClearChildren(InnerElement);
        }

        /// <summary>
        /// Replaces a breadcrumb.
        /// </summary>
        /// <param name="newComponent">The new breadcrumb.</param>
        /// <param name="oldComponent">The old breadcrumb.</param>
        public void Replace(TextBreadcrumb newComponent, TextBreadcrumb oldComponent)
        {
            InnerElement.replaceChild(newComponent.Render(), oldComponent.Render());
        }

        /// <summary>
        /// Adds a breadcrumb.
        /// </summary>
        /// <param name="component">The breadcrumb to add.</param>
        public void Add(TextBreadcrumb component)
        {
            if (InnerElement.childElementCount == 0)
            {
                InnerElement.appendChild(
                    Div(_("tss-textbreadcrumb-wrap"),
                        component.Render()
                    ));
            }
            else
            {
                InnerElement.appendChild(
                    Div(_("tss-textbreadcrumb-wrap"),
                        Span(_("tss-textbreadcrumb-sep"),
                            I(_())
                        ),
                        component.Render())
                );
            }

        }

        /// <summary>
        /// Adds multiple breadcrumbs.
        /// </summary>
        /// <param name="children">The breadcrumbs to add.</param>
        /// <returns>The current instance.</returns>
        public TextBreadcrumbs Items(params TextBreadcrumb[] children)
        {
            children.ForEach(x => Add(x));
            return this;
        }

        /// <summary>
        /// Renders the component.
        /// </summary>
        /// <returns>The rendered HTML element.</returns>
        public HTMLElement Render()
        {
            return InnerElement;
        }

        /// <summary>Gets or sets the text size.</summary>
        public virtual TextSize Size
        {
            get => ITextFormatingExtensions.FromClassList(InnerElement, TextSize.Small);
            set
            {
                InnerElement.classList.remove(Size.ToString());
                InnerElement.classList.add(value.ToString());
            }
        }

        /// <summary>Gets or sets the text weight.</summary>
        public virtual TextWeight Weight
        {
            get => ITextFormatingExtensions.FromClassList(InnerElement, TextWeight.Regular);
            set
            {
                InnerElement.classList.remove(Weight.ToString());
                InnerElement.classList.add(value.ToString());
            }
        }

        /// <summary>Gets or sets the text alignment.</summary>
        public TextAlign TextAlign { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <summary>Gets or sets the foreground color.</summary>
        public string Foreground { get => InnerElement.style.color; set => InnerElement.style.color = value; }
    }
}