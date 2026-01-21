using System;
using System.Collections.Generic;
using System.Text;
using H5;
using H5.Core;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A NodeView component that provides a visual node-based editor (based on BaklavaJS).
    /// </summary>
    [H5.Name("tss.NodeView")]
    public class NodeView : IComponent
    {
        private dom.HTMLDivElement _owner;
        private Raw _parent;
        private ViewModel _viewModel;
        private MutationObserver _observer;
        private List<Action> _whenMounted = new List<Action>();
        private double _onChangeTimeout;
        private event Action<NodeView> _onChange;
        private string _previousState;

        /// <summary>
        /// Initializes a new instance of the NodeView class.
        /// </summary>
        public NodeView()
        {
            //Docs: https://baklava.tech/getting-started.html
            
            _owner = Div(_("tss-nodeview"));
            _parent = Raw(_owner);
            _parent.WhenMounted(() =>
            {
                _viewModel = Script.Write<ViewModel>("BaklavaJS.createBaklava({0})", _owner);

                foreach (var v in _whenMounted)
                {
                    v();
                }

                HookChangeMonitoring();

                InitializeSettings();

                window.setTimeout((_) => ReplaceToolbarIcons(), 15); //On a timer to ensure the entire UI is built and rendered
            });
        }

        /// <summary>
        /// Adds a change event handler to the node view.
        /// </summary>
        /// <param name="onChange">The event handler.</param>
        /// <returns>The current instance of the type.</returns>
        public NodeView OnChange(Action<NodeView> onChange)
        {
            _onChange += onChange;
            return this;
        }

        /// <summary>
        /// Defines a new static node type.
        /// </summary>
        /// <param name="nodeTypeName">The name of the node type.</param>
        /// <param name="buildNode">An action to build the node interface.</param>
        /// <returns>The current instance of the type.</returns>
        public NodeView DefineNode(string nodeTypeName, Action<InterfaceBuilder> buildNode)
        {
            Action action = () =>
            {
                var ib = InterfaceBuilder.New(nodeTypeName);
                buildNode(ib);
                if(!string.IsNullOrEmpty(ib._category))
                {
                    _viewModel.editor.registerNodeType(DefineNode(ib.Build()), new IRegisterNodeTypeOptions() { category = ib._category});
                }
                else
                {
                    _viewModel.editor.registerNodeType(DefineNode(ib.Build()));
                }
            };

            if(_viewModel is object)
            {
                action();
            }
            else
            {
                _whenMounted.Add(action);
            }
            
            return this;
        }

        /// <summary>
        /// Defines a new dynamic node type.
        /// </summary>
        /// <param name="nodeTypeName">The name of the node type.</param>
        /// <param name="buildBaseNode">An action to build the base node interface.</param>
        /// <param name="onUpdate">An action to perform when the node needs updating.</param>
        /// <returns>The current instance of the type.</returns>
        public NodeView DefineDynamicNode(string nodeTypeName, Action<InterfaceBuilder> buildBaseNode, Action<InputsState, OutputsState, InterfaceBuilder> onUpdate)
        {
            Action action = () =>
            {
                var ib = InterfaceBuilder.New(nodeTypeName).OnUpdate(onUpdate);
                buildBaseNode(ib);
                if (!string.IsNullOrEmpty(ib._category))
                {
                    _viewModel.editor.registerNodeType(DefineDynamicNode(ib.Build()), new IRegisterNodeTypeOptions() { category = ib._category });
                }
                else
                {
                    _viewModel.editor.registerNodeType(DefineDynamicNode(ib.Build()));
                }
            };

            if (_viewModel is object)
            {
                action();
            }
            else
            {
                _whenMounted.Add(action);
            }

            return this;
        }

        /// <summary>
        /// Renders the node view.
        /// </summary>
        /// <returns>The rendered HTMLElement.</returns>
        public dom.HTMLElement Render() => _parent.Render();

        /// <summary>
        /// Gets the current state of the node graph.
        /// </summary>
        /// <returns>The graph state.</returns>
        public NodeViewGraphState GetState() => _viewModel.displayedGraph.save();

        /// <summary>
        /// Gets the current state of the node graph as a JSON string.
        /// </summary>
        /// <param name="formated">Whether to format the JSON string.</param>
        /// <returns>The JSON string.</returns>
        public string GetJsonState(bool formated = false)
        {
            if (formated)
            {
                return es5.JSON.stringify(GetState(), (double[])null, 4);
            }
            else
            {
                return es5.JSON.stringify(GetState());
            }
        }
        /// <summary>
        /// Sets the state of the node graph from a JSON string.
        /// </summary>
        /// <param name="stateJson">The JSON string representing the graph state.</param>
        public void SetState(string stateJson)
        {
            SetState(es5.JSON.parse(stateJson).As<NodeViewGraphState>());
        }

        /// <summary>
        /// Sets the state of the node graph.
        /// </summary>
        /// <param name="state">The graph state.</param>
        public void SetState(NodeViewGraphState state)
        {
            if (_viewModel is object)
            {
                _viewModel.displayedGraph.load(state);
            }
            else
            {
                _whenMounted.Add(() => SetState(state));
            }
        }

        private void InitializeSettings()
        {
            _viewModel.settings.useStraightConnections = false;

            _viewModel.settings.enableMinimap = false; //Disabled in the source code as it has a performance impact

            _viewModel.settings.displayValueOnHover = false;

            _viewModel.settings.nodes = new NodeSettings()
            {
                defaultWidth = 200,
                maxWidth = 320,
                minWidth = 150,
                resizable = false,
                reverseY = false
            };

            _viewModel.settings.contextMenu = new ContextMenuSettings() { enabled = true, additionalItems = new ReadOnlyArray<ContextMenuItem>(new ContextMenuItem[0]) };

            _viewModel.settings.zoomToFit = new ZoomToFitSettings() { paddingLeft = 300, paddingRight = 50, paddingTop = 110, paddingBottom = 50 };

            _viewModel.settings.background = new BackgroundSetttings()
            {
                gridDivision = 5,
                gridSize = 100,
                subGridVisibleThreshold = 0.6,
            };

            //_viewModel.settings.palette = new PalleteSettings() { enabled = false };
            _viewModel.settings.sidebar = new SidebarSettings() { enabled = false, width = 200, resizable = true };

            _viewModel.settings.toolbar = new ToolbarSettings()
            {
                commands = new ReadOnlyArray<ToolbarCommand>(new[]
                    {
                        new ToolbarCommand()
                        {
                            command = ToolbarCommands.UNDO_COMMAND,
                            title = "Undo"
                        },
                        new ToolbarCommand()
                        {
                            command = ToolbarCommands.REDO_COMMAND,
                            title = "Redo"
                        },
                        new ToolbarCommand()
                        {
                            command = ToolbarCommands.COPY_COMMAND,
                            title = "Copy"
                        },
                        new ToolbarCommand()
                        {
                            command = ToolbarCommands.PASTE_COMMAND,
                            title = "Paste"
                        },
                        new ToolbarCommand()
                        {
                            command = ToolbarCommands.ZOOM_TO_FIT_GRAPH_COMMAND,
                            title = "Zoom To Fit"
                        },
                        new ToolbarCommand()
                        {
                            command = ToolbarCommands.START_SELECTION_BOX_COMMAND,
                            title = "Box Select"
                        }

                    }),
                subgraphCommands = new ReadOnlyArray<ToolbarCommand>(new ToolbarCommand[0]),
                enabled = true
            };
        }

        private void HookChangeMonitoring()
        {
            //Hack to capture all changes within the node view
            _observer = new MutationObserver((list, observer) =>
            {
                window.clearTimeout(_onChangeTimeout);
                _onChangeTimeout = window.setTimeout(_ => RaiseChange(), 100);
            });

            window.setTimeout(_ =>
            {
                _observer.observe(_owner.querySelector(".node-container").As<HTMLElement>(), new MutationObserverInit() { attributes = true, subtree = true });
            }, 50);
        }

        private void ReplaceToolbarIcons()
        {
            foreach (HTMLElement toolbarEl in _owner.querySelectorAll(".baklava-toolbar-button"))
            {
                switch (toolbarEl.title)
                {
                    case "Undo":
                    {
                        ReplaceIcon(toolbarEl, UIcons.Undo);
                        break;
                    }
                    case "Redo":
                    {
                        ReplaceIcon(toolbarEl, UIcons.Redo);
                        break;
                    }
                    case "Copy":
                    {
                        ReplaceIcon(toolbarEl, UIcons.Copy);
                        break;
                    }
                    case "Paste":
                    {
                        ReplaceIcon(toolbarEl, UIcons.Paste);
                        break;
                    }
                    case "Zoom To Fit":
                    {
                        ReplaceIcon(toolbarEl, UIcons.ExpandArrowsAlt);
                        break;
                    }
                    case "Box Select":
                    {
                        ReplaceIcon(toolbarEl, UIcons.SquareDashed);
                        break;
                    }
                }
            }

            void ReplaceIcon(HTMLElement el, UIcons icon)
            {
                ClearChildren(el);
                el.appendChild(Button().SetIcon(icon).Tooltip(el.title).NoMinSize().H(32).W(32).Render());
            }
        }

        private void RaiseChange()
        {
            var state = GetJsonState();
            if (state != _previousState)
            {
                _onChange?.Invoke(this);
                _previousState = state;
            }
        }


        private static object Wrap<T>(Func<T> value)
        {
            return value.As<object>();
        }

        /// <summary>
        /// A builder class for defining node interfaces.
        /// </summary>
        public class InterfaceBuilder
        {
            private string _type;
            private Dictionary<string, Func<NodeInterface>> _inputs = new Dictionary<string, Func<NodeInterface>>();
            private Dictionary<string, Func<NodeInterface>> _outputs = new Dictionary<string, Func<NodeInterface>>();
            private bool _twoColumn = false;
            private int _width = 200;
            private Action<InputsState, OutputsState, InterfaceBuilder> _onUpdate;
            private string[] _forceInputUpdates;
            private string[] _forceOutputUpdates;

            internal string _category { get; private set; }

            private InterfaceBuilder(string type)
            {
                _type = type;
            }

            internal static InterfaceBuilder New(string type) => new InterfaceBuilder(type);

            /// <summary>
            /// Sets whether the node should use a two-column layout.
            /// </summary>
            /// <param name="twoColumn">Whether to use two columns.</param>
            /// <returns>The current instance of the type.</returns>
            public InterfaceBuilder TwoColumn(bool twoColumn = true)
            {
                _twoColumn = twoColumn;
                return this;
            }

            /// <summary>
            /// Sets the category for the node type.
            /// </summary>
            /// <param name="category">The category name.</param>
            /// <returns>The current instance of the type.</returns>
            public InterfaceBuilder Category(string category)
            {
                _category = category;
                return this;
            }

            /// <summary>
            /// Sets the width of the node.
            /// </summary>
            /// <param name="width">The width in pixels.</param>
            /// <returns>The current instance of the type.</returns>
            public InterfaceBuilder Width(int width)
            {
                _width = width;
                return this;
            }

            /// <summary>
            /// Adds an input interface to the node.
            /// </summary>
            /// <param name="inputName">The name of the input.</param>
            /// <param name="inputGenerator">A function that returns the node interface.</param>
            /// <returns>The current instance of the type.</returns>
            public InterfaceBuilder AddInput(string inputName, Func<NodeInterface> inputGenerator)
            {
                _inputs[inputName] = inputGenerator;
                return this;
            }

            /// <summary>
            /// Adds an output interface to the node.
            /// </summary>
            /// <param name="outputName">The name of the output.</param>
            /// <param name="outputGenerator">A function that returns the node interface.</param>
            /// <returns>The current instance of the type.</returns>
            public InterfaceBuilder AddOutput(string outputName, Func<NodeInterface> outputGenerator)
            {
                _outputs[outputName] = outputGenerator;
                return this;
            }

            internal InterfaceBuilder OnUpdate(Action<InputsState, OutputsState, InterfaceBuilder> onUpdate)
            {
                _onUpdate = onUpdate;
                return this;
            }
            internal DynamicNodeDefinition Build()
            {
                GetInputsOutputs(out var inputObj, out var outputObj);

                return new DynamicNodeDefinition()
                {
                    type = _type,
                    inputs = inputObj,
                    outputs = outputObj,
                    twoColumn = _twoColumn,
                    width = _width,
                    onUpdate = _onUpdate is object ? WrapOnUpdate(_onUpdate) : null
                };
            }

            private void GetInputsOutputs(out object inputObj, out object outputObj)
            {
                inputObj = new object();
                outputObj = new object();
                foreach (var kv in _inputs)
                {
                    inputObj[kv.Key] = Wrap(kv.Value);
                }

                foreach (var kv in _outputs)
                {
                    outputObj[kv.Key] = Wrap(kv.Value);
                }
            }

            /// <summary>
            /// Specifies inputs that must be forced to update.
            /// </summary>
            /// <param name="inputs">The input names.</param>
            /// <returns>The current instance of the type.</returns>
            public InterfaceBuilder ForceInputUpdates(params string[] inputs)
            {
                _forceInputUpdates = inputs;
                return this;
            }

            /// <summary>
            /// Specifies outputs that must be forced to update.
            /// </summary>
            /// <param name="outputs">The output names.</param>
            /// <returns>The current instance of the type.</returns>
            public InterfaceBuilder ForceOutputUpdates(params string[] outputs)
            {
                _forceOutputUpdates = outputs;
                return this;
            }

            private Func<InputsState, OutputsState, DynamicNodeUpdateResult> WrapOnUpdate(Action<InputsState, OutputsState, InterfaceBuilder> onUpdate)
            {
                return (i,o) =>
                {
                    var ib = InterfaceBuilder.New(_type);
                    onUpdate(i, o, ib);
                    ib.GetInputsOutputs(out var inputObj, out var outputObj);
                    return new DynamicNodeUpdateResult()
                    {
                        inputs = inputObj,
                        outputs = outputObj,
                        forceUpdateInputs = _forceInputUpdates,
                        forceUpdateOutputs = _forceOutputUpdates
                    };
                };
            }
        }

        /// <summary>
        /// Helper methods for creating standard BaklavaJS interfaces.
        /// </summary>
        public static class Interfaces
        {
            /// <summary>
            /// Creates a button interface.
            /// </summary>
            public static NodeInterface ButtonInterface(string name, Action onClick, bool allowMultipleConnections = false, bool setPort = true)
            {
                var ni = Script.Write<NodeInterface>("new BaklavaJS.RendererVue.ButtonInterface({0}, {1})", name, onClick);
                ni["allowMultipleConnections"] = allowMultipleConnections;
                ni.setPort(setPort);
                return ni;
            }
            /// <summary>
            /// Creates a checkbox interface.
            /// </summary>
            public static NodeInterface CheckboxInterface(string name, bool value, bool allowMultipleConnections = false, bool setPort = true)
            {
                var ni = Script.Write<NodeInterface>("new BaklavaJS.RendererVue.CheckboxInterface({0}, {1})", name, value);
                ni["allowMultipleConnections"] = allowMultipleConnections;
                ni.setPort(setPort);
                return ni;
            }

            /// <summary>
            /// Creates an integer input interface.
            /// </summary>
            public static NodeInterface IntegerInterface(string name, int value, bool allowMultipleConnections = false, bool setPort = true)
            {
                var ni = Script.Write<NodeInterface>("new BaklavaJS.RendererVue.IntegerInterface({0}, {1})", name, value);
                ni["allowMultipleConnections"] = allowMultipleConnections;
                ni.setPort(setPort);
                return ni;
            }
            /// <summary>
            /// Creates a number input interface.
            /// </summary>
            public static NodeInterface NumberInterface(string name, double value, bool allowMultipleConnections = false, bool setPort = true)
            {
                var ni = Script.Write<NodeInterface>("new BaklavaJS.RendererVue.NumberInterface({0}, {1})", name, value);
                ni["allowMultipleConnections"] = allowMultipleConnections;
                ni.setPort(setPort);
                return ni;
            }
            /// <summary>
            /// Creates a select (dropdown) interface.
            /// </summary>
            public static NodeInterface SelectInterface(string name, string defaultValue, ReadOnlyArray<string> values, bool allowMultipleConnections = false, bool setPort = true)
            {
                var ni = Script.Write<NodeInterface>("new BaklavaJS.RendererVue.SelectInterface({0}, {1}, {2})", name, defaultValue, values);
                ni["allowMultipleConnections"] = allowMultipleConnections;
                ni.setPort(setPort);
                return ni;
            }
            /// <summary>
            /// Creates a slider interface.
            /// </summary>
            public static NodeInterface SliderInterface(string name, double value, double min, double max, bool allowMultipleConnections = false, bool setPort = true)
            {
                var ni = Script.Write<NodeInterface>("new BaklavaJS.RendererVue.SliderInterface({0}, {1})", name, value, min, max);
                ni["allowMultipleConnections"] = allowMultipleConnections;
                ni.setPort(setPort);
                return ni;
            }
            /// <summary>
            /// Creates a static text interface.
            /// </summary>
            public static NodeInterface TextInterface(string name, string value, bool allowMultipleConnections = false, bool setPort = true)
            {
                var ni = Script.Write<NodeInterface>("new BaklavaJS.RendererVue.TextInterface({0}, {1})", name, value);
                ni["allowMultipleConnections"] = allowMultipleConnections;
                ni.setPort(setPort);
                return ni;
            }

            /// <summary>
            /// Creates a text input interface.
            /// </summary>
            public static NodeInterface TextInputInterface(string name, string value, bool allowMultipleConnections = false, bool setPort = true)
            {
                var ni = Script.Write<NodeInterface>("new BaklavaJS.RendererVue.TextInputInterface({0}, {1})", name, value);
                ni["allowMultipleConnections"] = allowMultipleConnections;
                ni.setPort(setPort);
                return ni;
            }
        }

        private static Node CreateGraphNodeType(GraphTemplate template) => Script.Write<Node>("BaklavaJS.Core.createGraphNodeType({0})", template);
        private static AbstractNodeConstructor DefineDynamicNode(DynamicNodeDefinition nodeDefinition) => Script.Write<AbstractNodeConstructor>("BaklavaJS.Core.defineDynamicNode({0})", nodeDefinition);
        private static AbstractNodeConstructor DefineNode(DynamicNodeDefinition definition) => Script.Write<AbstractNodeConstructor>("BaklavaJS.Core.defineNode({0})", definition);
        private static string GetGraphNodeTypeString(GraphTemplate template) => Script.Write<string>("BaklavaJS.Core.getGraphNodeTypeString({0})", template);


        [ObjectLiteral]
        public class Node
        {
            public GraphTemplate template;
            public Graph subgraph;

            public CalculateFunction calculate;
            public EventEmitter events;
            public BeforeLoadAfterSaveHooks hooks;
            public string id;
            public ReadOnlyMap<string, NodeInterface> inputs;
            public ReadOnlyMap<string, NodeInterface> outputs;
            public string type;
            public Graph graph() { return null; }
            public string title() { return null; }
            public void title(string title) { }
            public void load(NodeState state) { }
            public void onDestroy() { }
            public void onPlaced() { }
            public void registerGraph(Graph graph) { }
            public NodeState save() { return null; }
        }

        [ObjectLiteral(ObjectCreateMode.Constructor)]
        public class Connection
        {
            public Connection(NodeInterface from, NodeInterface to) { }
            public bool destructed;
            public EventEmitter events;
            public NodeInterface from;
            public NodeInterface to;
            public string id;
            public void destruct() { }
        }

        [ObjectLiteral]
        public class Editor
        {
            public object connectionEvents;
            public EventEmitter events;
            public GraphEventEmitter graphEvents;
            public GraphHooks graphHooks;
            public GraphTemplateEventEmitter graphTemplateEvents;
            public BeforeLoadAfterSaveHooks graphTemplateHooks;
            public LoadSaveHooks hooks;
            public NodeEventsEmitter nodeEvents;
            public BeforeLoadAfterSaveHooks nodeHooks;

            public Graph graph() {  return null; }
            public ReadOnlyArray<Graph> graphs() { return null; }
            public ReadOnlyArray<GraphTemplate> graphTemplates() { return null; }
            public bool loading() { return false; }
            public ReadOnlyMap<string, INodeTypeInformation> nodeTypes() { return null; }

            public void addGraphTemplate(GraphTemplate graphTemplate) { }
            public void load(IEditorState state) { }
            public void registerGraph(Graph graph) { }
            [Template("{this}.registerNodeType({0})")] public extern void registerNodeType(AbstractNodeConstructor type);
            [Template("{this}.registerNodeType({0}, {1})")] public extern void registerNodeType(AbstractNodeConstructor type, IRegisterNodeTypeOptions options);
            public void removeGraphTemplate(GraphTemplate graphTemplate) { }
            public IEditorState save() { return null; }
            public void unregisterGraph(Graph graph) { }
            public void unregisterNodeType(string type) { }
            public void unregisterNodeType(AbstractNodeConstructor type) { }
        }

        [ObjectLiteral]
        public class Graph
        {
            public int activeTransactions;
            public DestructEventEmitter connectionEvents;
            public Editor editor;
            public GraphEventEmitter events;
            public GraphHooks hooks;
            public string id;
            public NodeEventsEmitter nodeEvents;
            public BeforeLoadAfterSaveHooks nodeHooks;
            public GraphTemplate  template;

            [Template("{this}.connections()")] public ReadOnlyArray<Connection> connections() { return null; }
            [Template("{this}.destroying()")] public bool destroying() { return false; }
            [Template("{this}.inputs()")] public ReadOnlyArray<IGraphInterface> inputs() { return null; }
            [Template("{this}.loading()")] public bool loading() { return false; }
            [Template("{this}.nodes()")] public ReadOnlyArray<Node> nodes() { return null; }
            [Template("{this}.outputs()")] public ReadOnlyArray<IGraphInterface> outputs() { return null; }

            [Template("{this}.addConnection({0}, {1})")] public void addConnection(NodeInterface from, NodeInterface to) { }
            [Template("{this}.addNode({0})")] public void addNode(NodeInterface node) { }
            [Template("{this}.checkConnection({0}, {1})")] public CheckConnectionResult checkConnection(NodeInterface from, NodeInterface to) { return null; }
            [Template("{this}.destroy()")] public void destroy() { }
            [Template("{this}.findNodeById({0})")] public Node findNodeById(string id) { return null; }
            [Template("{this}.findNodeInterface({0})")] public NodeInterface findNodeInterface(string id) { return null; }
            [Template("{this}.load({0})")] public void load(NodeViewGraphState state) { }
            [Template("{this}.removeConnection({0})")] public void removeConnection(Connection connection) { }
            [Template("{this}.removeNode({0})")] public void removeNode(Node node) { }
            [Template("{this}.save()")] public NodeViewGraphState save() { return null; }
        }

        [ObjectLiteral]
        public class ViewModel
        {
            public Graph displayedGraph;
            public Editor editor;
            public IHistory history;
            public readonly bool isSubgraph;
            public IViewSettings settings;
            [Template("{this}.switchGraph({0})")] public void switchGraph(Graph newGraph) { }
            [Template("{this}.switchGraph({0})")] public void switchGraph(GraphTemplate newGraph) { }

            //    clipboard: IClipboard;
            //    commandHandler: ICommandHandler;
            //    hooks: {
            //        renderInterface: SequentialHook<
            //            { el: HTMLElement; intf: NodeInterface<any>
            //        },
            //            null,
            //        >;
            //        renderNode: SequentialHook<{ el: HTMLElement; node: AbstractNode
            //    }, null>;
            //    };
            //isSubgraph: Readonly<boolean>;
        }

        [ObjectLiteral]
        public class DynamicNodeDefinition
        {
            public object inputs;
            public object outputs;
            public object calculate;
            public Action onCreate;
            public Action onDestroy;
            public Action onPlaced;
            public Func<InputsState, OutputsState, DynamicNodeUpdateResult> onUpdate;
            public string title;
            public string type;
            public bool twoColumn;
            public int width;
        }

        [ObjectLiteral]
        public class GraphTemplate
        {
            public ReadOnlyArray<ConnectionState> connections;
            public Editor editor;
            public GraphTemplateEventEmitter events;
            public BeforeLoadAfterSaveHooks hooks;
            public string id;
            public ReadOnlyArray<NodeState> nodes;
            [Template("{this}.inputs()")] public ReadOnlyArray<IGraphInterface> inputs() { return null; }
            [Template("{this}.name()")] public string name() { return null; }
            [Template("{this}.outputs()")] public ReadOnlyArray<IGraphInterface> outputs() { return null; }
            [Template("{this}.createGraph()")] public Graph createGraph() { return null; }
            [Template("{this}.createGraph({0})")] public Graph createGraph(Graph graph) { return null; }
            [Template("{this}.save()")] public IGraphTemplateState save() { return null; }
            [Template("{this}.update({0})")] public void update(NodeViewGraphState state) { }
            [Template("{this}.fromGraph({0}, {1})")] public static GraphTemplate fromGraph(Graph graph, Editor editor) { return null; }
        }

        [ObjectLiteral]
        public class CalculateFunction
        {

        }

        [ObjectLiteral]
        public class EventEmitter
        {

        }

        [ObjectLiteral]
        public class GraphEventEmitter
        {
            //addConnection: BaklavaEvent<IConnection, Graph>;
            //addNode: BaklavaEvent<AbstractNode, Graph>;
            //beforeAddConnection: PreventableBaklavaEvent<
            //    IAddConnectionEventData,
            //    Graph,
            //>;
            //beforeAddNode: PreventableBaklavaEvent<AbstractNode, Graph>;
            //beforeRemoveConnection: PreventableBaklavaEvent<IConnection, Graph>;
            //beforeRemoveNode: PreventableBaklavaEvent<AbstractNode, Graph>;
            //checkConnection: PreventableBaklavaEvent<IAddConnectionEventData, Graph>;
            //removeConnection: BaklavaEvent<IConnection, Graph>;
            //removeNode: BaklavaEvent<AbstractNode, Graph>;
        }

        [ObjectLiteral]
        public class GraphHooks
        {
            public object checkConnection;
            public object load;
            public object save;
        }

        [ObjectLiteral]
        public class GraphTemplateEventEmitter
        {
            public object nameChanged;
            public object updated;
        }
        [ObjectLiteral]
        public class DestructEventEmitter
        {
            public object destruct;
        }
        [ObjectLiteral]
        public class BeforeLoadAfterSaveHooks
        {
            public object beforeLoad;
            public object afterSave;
        }
        
        [ObjectLiteral]
        public class LoadSaveHooks
        {
            public object save;
            public object load;
        }

        [ObjectLiteral]
        public class NodeInterfaceEvents
        {
            //beforeSetValue: PreventableBaklavaEvent<T, NodeInterface<T>>;
            //setConnectionCount: BaklavaEvent<number, NodeInterface<T>>;
            //setValue: BaklavaEvent<T, NodeInterface<T>>;
            //updated: BaklavaEvent<void, NodeInterface<T>>;
        }

        [ObjectLiteral]
        public class NodeInterface
        {
            public object component;
            public NodeInterfaceEvents events;
            public bool hidden;
            public LoadSaveHooks hooks;
            public string id;
            public bool isInput;
            public string name;
            public string nodeId;
            public bool port;
            public string templateId;
            public int connectionCount() { return 0; }
            public void connectionCount(int value) { }
            [Template("{this}.value()")] public object value() { return null; }
            [Template("{this}.value({0})")] public void value(object value) { }
            [Template("{this}.load({0})")] public void load(INodeInterfaceState state) { }
            [Template("{this}.save()")] public INodeInterfaceState save() {  return null; }
            [Template("{this}.setComponent({0})")] public void setComponent(object component) { }
            [Template("{this}.setHidden({0})")] public void setHidden(bool hidden) { }
            [Template("{this}.setPort({0})")] public void setPort(bool value) { }
        }

        [ObjectLiteral]
        public class INodeInterfaceState
        {

        }

        [ObjectLiteral]
        public class NodeEventsEmitter
        {
            //addInput: BaklavaEvent<NodeInterface<any>, AbstractNode>;
            //addOutput: BaklavaEvent<NodeInterface<any>, AbstractNode>;
            //beforeAddInput: PreventableBaklavaEvent<NodeInterface<any>, AbstractNode>;
            //beforeAddOutput: PreventableBaklavaEvent<NodeInterface<any>, AbstractNode>;
            //beforeRemoveInput: PreventableBaklavaEvent<
            //    NodeInterface<any>,
            //    AbstractNode,
            //>;
            //beforeRemoveOutput: PreventableBaklavaEvent<
            //    NodeInterface<any>,
            //    AbstractNode,
            //>;
            //beforeTitleChanged: PreventableBaklavaEvent<string, AbstractNode>;
            //loaded: BaklavaEvent<AbstractNode, AbstractNode>;
            //removeInput: BaklavaEvent<NodeInterface<any>, AbstractNode>;
            //removeOutput: BaklavaEvent<NodeInterface<any>, AbstractNode>;
            //titleChanged: BaklavaEvent<string, AbstractNode>;
            //update: BaklavaEvent<null | INodeUpdateEventData, AbstractNode>;
        }



        [ObjectLiteral]
        public class INodeTypeInformation
        {
        }

        [ObjectLiteral]
        public class IEditorState
        {

        }

        [ObjectLiteral]
        public class NodeViewGraphState
        {
            public string id;
            public ReadOnlyArray<NodeState> nodes;
            public ReadOnlyArray<ConnectionState> connections;
            public ReadOnlyArray<GraphInterfaceState> inputs;
            public ReadOnlyArray<GraphInterfaceState> outputs;
            public PanningState panning;
            public double scaling;
        }

        [ObjectLiteral]
        public class PanningState
        {
            public double x;
            public double y;
        }
        [ObjectLiteral]
        public class PositionState
        {
            public double x;
            public double y;
        }

        [ObjectLiteral]
        public class NodeState
        {
            public string type;
            public string id;
            public string title;
            public ReadOnlyMap<string, ValueState> inputs;
            public ReadOnlyMap<string, ValueState> outputs;
            public PositionState position;
            public double width;
            public bool twoColumn;
        }

        [ObjectLiteral]
        public class GraphInterfaceState
        {
            public string id;
            public string name;
            public string nodeId;
            public string nodeInterfaceId;
        }

        [ObjectLiteral]
        public class ValueState
        {
            public string id;
            public dynamic value;
        }

        [ObjectLiteral]
        public class IRegisterNodeTypeOptions
        {
            public string category;
        }

        [ObjectLiteral]
        public class AbstractNodeConstructor
        {

        }

        [ObjectLiteral]
        public class IGraphInterface
        {

        }
        
        [ObjectLiteral]
        public class IGraphTemplateState
        {

        }

        [ObjectLiteral]
        public class CheckConnectionResult
        {

        }
        
        [ObjectLiteral]
        public class ConnectionState
        {

        }

        [ObjectLiteral]
        public class IHistory
        {

        }

        [ObjectLiteral]
        public class IViewSettings
        {
            public BackgroundSetttings background;
            public ContextMenuSettings contextMenu;
            public bool displayValueOnHover;
            public bool enableMinimap;
            public NodeSettings nodes;
            public PalleteSettings palette;
            public SidebarSettings sidebar;
            public ToolbarSettings toolbar;
            public bool useStraightConnections;
            public ZoomToFitSettings zoomToFit;
        }

        [ObjectLiteral]
        public class BackgroundSetttings
        {
            public double gridDivision;
            public double gridSize;
            public double subGridVisibleThreshold;
        }

        [ObjectLiteral]
        public class ContextMenuSettings
        {
            public bool enabled;
            public ReadOnlyArray<ContextMenuItem> additionalItems;
        }

        [ObjectLiteral]
        public class ContextMenuItem
        {

        }

        [ObjectLiteral]
        public class NodeSettings
        {
            public int defaultWidth;
            public int maxWidth;
            public int minWidth;
            public bool resizable;
            public bool reverseY;
        }

        [ObjectLiteral]
        public class PalleteSettings
        {
            public bool enabled;
        }

        [ObjectLiteral]
        public class SidebarSettings
        {
            public bool enabled;
            public bool resizable;
            public int width;
        }

        [ObjectLiteral]
        public class ToolbarSettings
        {
            public bool enabled;
            public ReadOnlyArray<ToolbarCommand> commands;
            public ReadOnlyArray<ToolbarCommand> subgraphCommands;
        }

        [Enum(Emit.StringName)]
        public enum ToolbarCommands
        {
            [Name("ZOOM_TO_FIT_RECT")] ZOOM_TO_FIT_RECT_COMMAND,
            [Name("ZOOM_TO_FIT_NODES")] ZOOM_TO_FIT_NODES_COMMAND,
            [Name("ZOOM_TO_FIT_GRAPH")] ZOOM_TO_FIT_GRAPH_COMMAND,
            [Name("COPY")] COPY_COMMAND,
            [Name("PASTE")] PASTE_COMMAND,
            [Name("CLEAR_CLIPBOARD")] CLEAR_CLIPBOARD_COMMAND,
            [Name("UNDO")] UNDO_COMMAND,
            [Name("REDO")] REDO_COMMAND,
            [Name("CREATE_SUBGRAPH")] CREATE_SUBGRAPH_COMMAND,
            [Name("SAVE_SUBGRAPH")] SAVE_SUBGRAPH_COMMAND,
            [Name("START_SELECTION_BOX")] START_SELECTION_BOX_COMMAND,
            [Name("DELETE_NODES")] DELETE_NODES_COMMAND,
            [Name("SWITCH_TO_MAIN_GRAPH")] SWITCH_TO_MAIN_GRAPH_COMMAND,
        }

        [ObjectLiteral]
        public class ToolbarCommand
        {
            public ToolbarCommands command;
            public string title;
            public object icon;
        }

        [ObjectLiteral]
        public class ZoomToFitSettings
        {
            public int paddingBottom;
            public int paddingLeft;
            public int paddingRight;
            public int paddingTop;
        }
        
        [ObjectLiteral]
        public class DynamicNodeUpdateResult
        {
            public object inputs;
            public object outputs;
            public ReadOnlyArray<string> forceUpdateInputs;
            public ReadOnlyArray<string> forceUpdateOutputs;
        }

        [ObjectLiteral]
        public class InputsState
        {

        }
        
        [ObjectLiteral]
        public class OutputsState
        {

        }
    }
}
