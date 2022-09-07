using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    public class SidebarCommands : ISidebarItem
    {
        private readonly SidebarCommand[] _commands;
        private bool _isEndAligned;

        public SidebarCommands(params SidebarCommand[] commands)
        {
            _commands = commands;
        }

        public bool IsSelected { get; set; }

        public IComponent CurrentRendered { get; private set; }

        private void WhenSizeIsStable(HTMLElement element, Action<int> action, int previousWidth = -999)
        {
            if (element.IsMounted())
            {
                var currentWidth = (int)(element.getBoundingClientRect().As<DOMRect>().width);
                if (currentWidth == previousWidth)
                {
                    action(currentWidth);
                }
                else
                {
                    window.setTimeout((_) => WhenSizeIsStable(element, action, currentWidth), 16);
                }
            }
            else
            {
                DomObserver.WhenMounted(element, () => WhenSizeIsStable(element, action));
            }
        }

        public IComponent RenderOpen()
        {
            var div = Div(_("tss-sidebar-commands-line"));
            var divWrapped = Raw(div);

            if (_isEndAligned)
            {
                div.classList.add("tss-sidebar-commands-end-aligned");
            }

            DomObserver.WhenMounted(div, () =>
            {
                ClearChildren(div); //Make sure we don't hit here twice

                _commands[0].RefreshTooltip();
                div.appendChild(_commands[0].Render());

                WhenSizeIsStable(div , (stableWidth) =>
                {
                    HTMLDivElement otherCommands = null;
                    int max = (int)Math.Floor(stableWidth / 48f);

                    if (_isEndAligned && _commands.Length > 1)
                    {
                        div.appendChild(Div(_("tss-sidebar-commands-spacer")));
                    }

                    for (int i = 1; i < _commands.Length; i++)
                    {
                        var command = _commands[i];
                        command.RefreshTooltip();
                        if (i < max)
                        {
                            div.appendChild(command.Render());
                        }
                        else
                        {
                            if (otherCommands is null) otherCommands = Div(_("tss-sidebar-commands-line-extra"));
                            otherCommands.appendChild(command.Render());
                        }
                    }

                    if (otherCommands is object)
                    {
                        divWrapped.Tooltip(Raw(otherCommands), true, placement: TooltipPlacement.Right, delayHide: 500, maxWidth:1000);

                        DomObserver.WhenMounted(otherCommands, () =>
                        {
                            DomObserver.WhenRemoved(otherCommands, () =>
                            {
                                if (divWrapped.IsMounted())
                                {
                                    for (int i = 0; i < _commands.Length; i++)
                                    {
                                        _commands[i].RefreshTooltip();
                                    }
                                }
                            });
                        });
                    }
                }, 400); //Need to be after the animation
            });
            CurrentRendered = divWrapped;
            return divWrapped;
        }

        internal IComponent RenderOpenFull()
        {
            var div = Div(_("tss-sidebar-commands-line"));
            var divWrapped = Raw(div);

            if (_isEndAligned)
            {
                div.classList.add("tss-sidebar-commands-end-aligned");
            }

            DomObserver.WhenMounted(div, () =>
            {
                ClearChildren(div); //Make sure we don't hit here twice

                for (int i = 0; i < _commands.Length; i++)
                {
                    var command = _commands[i];
                    command.RefreshTooltip();
                    div.appendChild(command.Render());

                    if (_isEndAligned && i == 0 && _commands.Length > 1)
                    {
                        div.appendChild(Div(_("tss-sidebar-commands-spacer")));
                    }
                }
            });

            CurrentRendered = divWrapped;
            return divWrapped;
        }

        public IComponent RenderClosed()
        {
            var div = Div(_("tss-sidebar-commands-line tss-sidebar-commands-line-closed"));
            var divWrapped = Raw(div);

            DomObserver.WhenMounted(div, () =>
            {
                ClearChildren(div); //Make sure we don't hit here twice

                HTMLDivElement otherCommands = null;
                int max = 1;

                for (int i = 0; i < _commands.Length; i++)
                {
                    var command = _commands[i];
                    command.RefreshTooltip();
                    if (i < max)
                    {
                        div.appendChild(command.Render());
                    }
                    else
                    {
                        if (otherCommands is null) otherCommands = Div(_("tss-sidebar-commands-line-extra"));
                        otherCommands.appendChild(command.Render());
                    }
                }

                if (otherCommands is object)
                {
                    divWrapped.Tooltip(Raw(otherCommands), true, placement: TooltipPlacement.Right, delayHide: 500, maxWidth: 1000);
                    DomObserver.WhenMounted(otherCommands, () =>
                    {
                        DomObserver.WhenRemoved(otherCommands, () =>
                        {
                            if (divWrapped.IsMounted())
                            {
                                for (int i = 0; i < _commands.Length; i++)
                                {
                                    _commands[i].RefreshTooltip();
                                }
                            }
                        });
                    });
                }
            });

            CurrentRendered = divWrapped;
            return divWrapped;
        }

        public SidebarCommands AlignEnd()
        {
            _isEndAligned = true;
            return this;
        }
    }
}