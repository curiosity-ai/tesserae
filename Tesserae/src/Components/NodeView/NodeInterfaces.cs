using System;
using H5;

namespace Tesserae
{
    public class NodeInput<T> : INodeInput<T>
    {
        public string Name { get; }
        public string Title { get; }
        public T DefaultValue { get; }
        public bool AllowMultipleConnections { get; }
        public bool SetPort { get; }

        public NodeInput(string name, string title, T defaultValue = default, bool allowMultipleConnections = false, bool setPort = true)
        {
            Name = name;
            Title = title;
            DefaultValue = defaultValue;
            AllowMultipleConnections = allowMultipleConnections;
            SetPort = setPort;
        }

        public virtual NodeView.NodeInterface Build()
        {
            var type = typeof(T);

            if (type == typeof(string))
            {
                return NodeView.Interfaces.TextInputInterface(Title, (string)(object)DefaultValue, AllowMultipleConnections, SetPort);
            }
            if (type == typeof(int))
            {
                return NodeView.Interfaces.IntegerInterface(Title, (int)(object)DefaultValue, AllowMultipleConnections, SetPort);
            }
            if (type == typeof(double))
            {
                return NodeView.Interfaces.NumberInterface(Title, (double)(object)DefaultValue, AllowMultipleConnections, SetPort);
            }
            if (type == typeof(bool))
            {
                return NodeView.Interfaces.CheckboxInterface(Title, (bool)(object)DefaultValue, AllowMultipleConnections, SetPort);
            }
            if (type == typeof(Action))
            {
                return NodeView.Interfaces.ButtonInterface(Title, (Action)(object)DefaultValue, AllowMultipleConnections, SetPort);
            }

            throw new ArgumentException($"Type {type.Name} is not supported for generic NodeInput without a specialized class.");
        }
    }

    public class NodeOutput<T> : INodeOutput<T>
    {
        public string Name { get; }
        public string Title { get; }
        public T DefaultValue { get; }
        public bool AllowMultipleConnections { get; }
        public bool SetPort { get; }

        public NodeOutput(string name, string title, T defaultValue = default, bool allowMultipleConnections = false, bool setPort = true)
        {
            Name = name;
            Title = title;
            DefaultValue = defaultValue;
            AllowMultipleConnections = allowMultipleConnections;
            SetPort = setPort;
        }

        public virtual NodeView.NodeInterface Build()
        {
            var type = typeof(T);

            if (type == typeof(string))
            {
                return NodeView.Interfaces.TextInterface(Title, (string)(object)DefaultValue, AllowMultipleConnections, SetPort);
            }
            if (type == typeof(int))
            {
                return NodeView.Interfaces.IntegerInterface(Title, (int)(object)DefaultValue, AllowMultipleConnections, SetPort);
            }
            if (type == typeof(double))
            {
                return NodeView.Interfaces.NumberInterface(Title, (double)(object)DefaultValue, AllowMultipleConnections, SetPort);
            }
            if (type == typeof(bool))
            {
                return NodeView.Interfaces.CheckboxInterface(Title, (bool)(object)DefaultValue, AllowMultipleConnections, SetPort);
            }
            if (type == typeof(Action))
            {
                return NodeView.Interfaces.ButtonInterface(Title, (Action)(object)DefaultValue, AllowMultipleConnections, SetPort);
            }

            throw new ArgumentException($"Type {type.Name} is not supported for generic NodeOutput without a specialized class.");
        }
    }

    public class NodeSelectInput : NodeInput<string>
    {
        public ReadOnlyArray<string> Values { get; }

        public NodeSelectInput(string name, string title, string defaultValue, ReadOnlyArray<string> values, bool allowMultipleConnections = false, bool setPort = true)
            : base(name, title, defaultValue, allowMultipleConnections, setPort)
        {
            Values = values;
        }

        public override NodeView.NodeInterface Build()
        {
            return NodeView.Interfaces.SelectInterface(Title, DefaultValue, Values, AllowMultipleConnections, SetPort);
        }
    }

    public class NodeSliderInput : NodeInput<double>
    {
        public double Min { get; }
        public double Max { get; }

        public NodeSliderInput(string name, string title, double defaultValue, double min, double max, bool allowMultipleConnections = false, bool setPort = true)
            : base(name, title, defaultValue, allowMultipleConnections, setPort)
        {
            Min = min;
            Max = max;
        }

        public override NodeView.NodeInterface Build()
        {
            return NodeView.Interfaces.SliderInterface(Title, DefaultValue, Min, Max, AllowMultipleConnections, SetPort);
        }
    }
}
