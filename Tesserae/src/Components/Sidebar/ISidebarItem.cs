using System;
using System.Collections.Generic;

namespace Tesserae
{
    public interface ISidebarItem
    {
        IComponent RenderClosed();
        IComponent RenderOpen();
        bool       IsSelected      { get; set; }
        IComponent CurrentRendered { get; }
        void       Show();
        void       Collapse();
        string     Identifier { get; }
        string     OwnIdentifier { get; }
        void       AddGroupIdentifier(string groupIdentifier);
    }
}