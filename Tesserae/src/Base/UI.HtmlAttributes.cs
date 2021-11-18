using System;
using static H5.Core.dom;

namespace Tesserae
{
    public static partial class UI
    {
        //Overloads for most used cases:
        public static Attributes _() => new Attributes();
        public static Attributes _(string className) => new Attributes() { ClassName = className };

        public static Attributes _(string className                         = null,
                                    string id                               = null,
                                    string src                              = null,
                                    string href                             = null,
                                    string rel                              = null,
                                    string target                           = null,
                                    string text                             = null,
                                    string type                             = null,
                                    bool?  disabled                         = null,
                                    string value                            = null,
                                    string placeholder                      = null,
                                    string defaultValue                     = null,
                                    string title                            = null,
                                    Action<HTMLElement> el                  = null,
                                    Action<CSSStyleDeclaration> styles      = null)
        {
            return new Attributes
            {
                ClassName = className,
                Id = id,
                OnElementCreate = el,
                Styles = styles,

                //TODO: remove all of this too:
                Title = title,
                Href = href,
                Src = src,
                Rel = rel,
                Target = target,

                Text = text,
                Type = type,
                Disabled = disabled,
                Value = value,
                DefaultValue = defaultValue,
                Placeholder = placeholder
            };
        }
    }
}
