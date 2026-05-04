using System;
using H5;

namespace Tesserae
{
    public class NodeTextInput : INodeInput, INodeOutput
    {
        public string Name { get; }
        public string Title { get; }
        public string DefaultValue { get; }
        public bool AllowMultipleConnections { get; }
        public bool SetPort { get; }

        public NodeTextInput(string name, string title, string defaultValue = "", bool allowMultipleConnections = false, bool setPort = true)
        {
            Name = name;
            Title = title;
            DefaultValue = defaultValue;
            AllowMultipleConnections = allowMultipleConnections;
            SetPort = setPort;
        }

        public NodeView.NodeInterface Build() => NodeView.Interfaces.TextInputInterface(Title, DefaultValue, AllowMultipleConnections, SetPort);
    }

    public class NodeText : INodeInput, INodeOutput
    {
        public string Name { get; }
        public string Title { get; }
        public string DefaultValue { get; }
        public bool AllowMultipleConnections { get; }
        public bool SetPort { get; }

        public NodeText(string name, string title, string defaultValue = "", bool allowMultipleConnections = false, bool setPort = true)
        {
            Name = name;
            Title = title;
            DefaultValue = defaultValue;
            AllowMultipleConnections = allowMultipleConnections;
            SetPort = setPort;
        }

        public NodeView.NodeInterface Build() => NodeView.Interfaces.TextInterface(Title, DefaultValue, AllowMultipleConnections, SetPort);
    }

    public class NodeInteger : INodeInput, INodeOutput
    {
        public string Name { get; }
        public string Title { get; }
        public int DefaultValue { get; }
        public bool AllowMultipleConnections { get; }
        public bool SetPort { get; }

        public NodeInteger(string name, string title, int defaultValue = 0, bool allowMultipleConnections = false, bool setPort = true)
        {
            Name = name;
            Title = title;
            DefaultValue = defaultValue;
            AllowMultipleConnections = allowMultipleConnections;
            SetPort = setPort;
        }

        public NodeView.NodeInterface Build() => NodeView.Interfaces.IntegerInterface(Title, DefaultValue, AllowMultipleConnections, SetPort);
    }

    public class NodeNumber : INodeInput, INodeOutput
    {
        public string Name { get; }
        public string Title { get; }
        public double DefaultValue { get; }
        public bool AllowMultipleConnections { get; }
        public bool SetPort { get; }

        public NodeNumber(string name, string title, double defaultValue = 0, bool allowMultipleConnections = false, bool setPort = true)
        {
            Name = name;
            Title = title;
            DefaultValue = defaultValue;
            AllowMultipleConnections = allowMultipleConnections;
            SetPort = setPort;
        }

        public NodeView.NodeInterface Build() => NodeView.Interfaces.NumberInterface(Title, DefaultValue, AllowMultipleConnections, SetPort);
    }

    public class NodeButton : INodeInput, INodeOutput
    {
        public string Name { get; }
        public string Title { get; }
        public Action OnClick { get; }
        public bool AllowMultipleConnections { get; }
        public bool SetPort { get; }

        public NodeButton(string name, string title, Action onClick, bool allowMultipleConnections = false, bool setPort = true)
        {
            Name = name;
            Title = title;
            OnClick = onClick;
            AllowMultipleConnections = allowMultipleConnections;
            SetPort = setPort;
        }

        public NodeView.NodeInterface Build() => NodeView.Interfaces.ButtonInterface(Title, OnClick, AllowMultipleConnections, SetPort);
    }

    public class NodeCheckbox : INodeInput, INodeOutput
    {
        public string Name { get; }
        public string Title { get; }
        public bool DefaultValue { get; }
        public bool AllowMultipleConnections { get; }
        public bool SetPort { get; }

        public NodeCheckbox(string name, string title, bool defaultValue = false, bool allowMultipleConnections = false, bool setPort = true)
        {
            Name = name;
            Title = title;
            DefaultValue = defaultValue;
            AllowMultipleConnections = allowMultipleConnections;
            SetPort = setPort;
        }

        public NodeView.NodeInterface Build() => NodeView.Interfaces.CheckboxInterface(Title, DefaultValue, AllowMultipleConnections, SetPort);
    }

    public class NodeSelect : INodeInput, INodeOutput
    {
        public string Name { get; }
        public string Title { get; }
        public string DefaultValue { get; }
        public ReadOnlyArray<string> Values { get; }
        public bool AllowMultipleConnections { get; }
        public bool SetPort { get; }

        public NodeSelect(string name, string title, string defaultValue, ReadOnlyArray<string> values, bool allowMultipleConnections = false, bool setPort = true)
        {
            Name = name;
            Title = title;
            DefaultValue = defaultValue;
            Values = values;
            AllowMultipleConnections = allowMultipleConnections;
            SetPort = setPort;
        }

        public NodeView.NodeInterface Build() => NodeView.Interfaces.SelectInterface(Title, DefaultValue, Values, AllowMultipleConnections, SetPort);
    }

    public class NodeSlider : INodeInput, INodeOutput
    {
        public string Name { get; }
        public string Title { get; }
        public double DefaultValue { get; }
        public double Min { get; }
        public double Max { get; }
        public bool AllowMultipleConnections { get; }
        public bool SetPort { get; }

        public NodeSlider(string name, string title, double defaultValue, double min, double max, bool allowMultipleConnections = false, bool setPort = true)
        {
            Name = name;
            Title = title;
            DefaultValue = defaultValue;
            Min = min;
            Max = max;
            AllowMultipleConnections = allowMultipleConnections;
            SetPort = setPort;
        }

        public NodeView.NodeInterface Build() => NodeView.Interfaces.SliderInterface(Title, DefaultValue, Min, Max, AllowMultipleConnections, SetPort);
    }
}
