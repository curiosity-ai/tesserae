using System;
using System.Collections.Generic;
using H5.Core;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    public class SidebarText : ISidebarItem
    {
        private readonly TextBlock  _closed;
        private readonly TextBlock  _open;
        public           IComponent CurrentRendered => _closed.IsMounted() ? _closed : _open;

        public bool IsSelected { get; set; }

        public SidebarText(string identifier, string text, string closedText = null, TextSize textSize = TextSize.Small, TextWeight textWeight = TextWeight.Regular)
        {
            Identifier = identifier;
            _closed    = TextBlock(closedText ?? "", textSize: textSize, textWeight: textWeight);
            _open      = TextBlock(text, textSize: textSize, textWeight: textWeight);
        }

        public void Show()
        {
            _closed.Show();
            _open.Show();
        }

        public void Collapse()
        {
            _closed.Collapse();
            _open.Collapse();
        }
        public string Identifier { get; private set; }
        public void AddGroupIdentifier(string groupIdentifier)
        {
            Identifier = groupIdentifier + "_|_" + Identifier;
        }


        public SidebarText SetText(string text)
        {
            _open.Text = text;
            return this;
        }

        public SidebarText Foreground(string color)
        {
            _open.Foreground   = color;
            _closed.Foreground = color;
            return this;
        }

        public SidebarText PT(int pixels)
        {
            _open.PT(pixels);
            _closed.PT(pixels);
            return this;
        }

        public SidebarText PB(int pixels)
        {
            _open.PB(pixels);
            _closed.PB(pixels);
            return this;
        }

        public SidebarText PL(int pixels)
        {
            _open.PL(pixels);
            return this;
        }


        public IComponent RenderClosed()
        {
            return _closed;
        }



        public IComponent RenderOpen()
        {
            return _open;
        }
    }
}