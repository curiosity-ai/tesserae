using System;
using System.Collections.Generic;
using System.Text;
using H5;
using H5.Core;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.NodeView")]
    public class NodeView : IComponent
    {
        private dom.HTMLDivElement _owner;
        private Raw _parent;
        private ViewModel _viewModel;
        private MutationObserver _observer;

        private string _previousState;

        public NodeView()
        {
            _owner = Div(_("tss-nodeview"));
            _parent = Raw(_owner);
            _parent.WhenMounted(() =>
            {
                _viewModel = Script.Write<ViewModel>("BaklavaJS.createBaklava({0})", _owner);

                _viewModel.editor.registerNodeType(DefineNode(InterfaceBuilder.New("Hello World")
                                                          .AddInput("inp", () => Interfaces.TextInputInterface("Input", "Hi Input"))
                                                          .AddOutput("out", () => Interfaces.TextInputInterface("Output", "Hi Output"))
                                                          .Build()));

                _viewModel.editor.registerNodeType(DefineNode(InterfaceBuilder.New("Complex")
                                          .AddInput("text", () => Interfaces.TextInterface("Input", "Hi Input"))
                                          .AddInput("int", () => Interfaces.IntegerInterface("Input", 123))
                                          .AddInput("num", () => Interfaces.NumberInterface("Input", 3.14))
                                          .AddInput("btn", () => Interfaces.ButtonInterface("Input", () => Toast().Information("Hi!")))
                                          .AddInput("chk", () => Interfaces.CheckboxInterface("Input", false))
                                          .AddInput("sel", () => Interfaces.SelectInterface("Input", "A", new ReadOnlyArray<string>(new[] { "A", "B", "C" })))
                                          .AddInput("sld", () => Interfaces.SliderInterface("Input", 0.5, 0, 1))
                                          .AddOutput("out", () => Interfaces.TextInterface("Output", "Hi Output"))
                                          .Build()));

                HookChangeMonitoring();

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
                        }
                    }),
                    subgraphCommands = new ReadOnlyArray<ToolbarCommand>(new ToolbarCommand[0]),
                    enabled = true
                };

                window.setTimeout((_) => ReplaceToolbarIcons(), 15);
            });
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
            foreach(HTMLElement toolbarEl in _owner.querySelectorAll(".baklava-toolbar-button"))
            {
                switch(toolbarEl.title)
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
                }
            }
            
            void ReplaceIcon(HTMLElement el, UIcons icon)
            {
                ClearChildren(el);
                el.appendChild(Button().SetIcon(icon).Tooltip(el.title).NoMinSize().H(32).W(32).Render());
            }
        }

        private double _onChangeTimeout;

        private event Action<NodeView> _onChange; 
        private void RaiseChange()
        {
            var state = GetState();
            if (state != _previousState)
            {
                _onChange?.Invoke(this);
                _previousState = state;
            }
        }

        public NodeView OnChange(Action<NodeView> onChange)
        {
            _onChange += onChange;
            return this;
        }

        private static object Wrap<T>(Func<T> value)
        {
            return value.As<object>();
        }

        public class InterfaceBuilder
        {
            private string _type;
            private Dictionary<string, Func<NodeInterface>> _inputs = new Dictionary<string, Func<NodeInterface>>();
            private Dictionary<string, Func<NodeInterface>> _outputs = new Dictionary<string, Func<NodeInterface>>();
            private InterfaceBuilder(string type)
            {
                _type = type;
            }

            public static InterfaceBuilder New(string type) => new InterfaceBuilder(type);
            public InterfaceBuilder AddInput(string inputName, Func<NodeInterface> inputGenerator)
            {
                _inputs[inputName] = inputGenerator;
                return this;
            }

            public InterfaceBuilder AddOutput(string outputName, Func<NodeInterface> outputGenerator)
            {
                _outputs[outputName] = outputGenerator;
                return this;
            }

            public DynamicNodeDefinition Build()
            {
                var inputObj  = new object();
                var outputObj = new object();
                foreach (var kv in _inputs)
                {
                    inputObj[kv.Key] = Wrap(kv.Value);
                }
                foreach (var kv in _outputs)
                {
                    outputObj[kv.Key] = Wrap(kv.Value);
                }

                return new DynamicNodeDefinition()
                {
                    type = _type,
                    inputs = inputObj,
                    outputs = outputObj
                };
            }
        }

        public static class Interfaces
        {
            public static NodeInterface ButtonInterface(string name, Action onClick, bool allowMultipleConnections = false)
            {
                var ni = Script.Write<NodeInterface>("new BaklavaJS.RendererVue.ButtonInterface({0}, {1})", name, onClick);
                ni["allowMultipleConnections"] = allowMultipleConnections;
                return ni;
            }
            public static NodeInterface CheckboxInterface(string name, bool value, bool allowMultipleConnections = false)
            {
                var ni = Script.Write<NodeInterface>("new BaklavaJS.RendererVue.CheckboxInterface({0}, {1})", name, value);
                ni["allowMultipleConnections"] = allowMultipleConnections;
                return ni;
            }

            public static NodeInterface IntegerInterface(string name, int value, bool allowMultipleConnections = false)
            {
                var ni = Script.Write<NodeInterface>("new BaklavaJS.RendererVue.IntegerInterface({0}, {1})", name, value);
                ni["allowMultipleConnections"] = allowMultipleConnections;
                return ni;
            }
            public static NodeInterface NumberInterface(string name, double value, bool allowMultipleConnections = false)
            {
                var ni = Script.Write<NodeInterface>("new BaklavaJS.RendererVue.NumberInterface({0}, {1})", name, value);
                ni["allowMultipleConnections"] = allowMultipleConnections;
                return ni;
            }
            public static NodeInterface SelectInterface(string name, string defaultValue, ReadOnlyArray<string> values, bool allowMultipleConnections = false)
            {
                var ni = Script.Write<NodeInterface>("new BaklavaJS.RendererVue.SelectInterface({0}, {1}, {2})", name, defaultValue, values);
                ni["allowMultipleConnections"] = allowMultipleConnections;
                return ni;
            }
            public static NodeInterface SliderInterface(string name, double value, double min, double max, bool allowMultipleConnections = false)
            {
                var ni = Script.Write<NodeInterface>("new BaklavaJS.RendererVue.SliderInterface({0}, {1})", name, value, min, max);
                ni["allowMultipleConnections"] = allowMultipleConnections;
                return ni;
            }
            public static NodeInterface TextInterface(string name, string value, bool allowMultipleConnections = false)
            {
                var ni = Script.Write<NodeInterface>("new BaklavaJS.RendererVue.TextInterface({0}, {1})", name, value);
                ni["allowMultipleConnections"] = allowMultipleConnections;
                return ni;
            }

            public static NodeInterface TextInputInterface(string name, string value, bool allowMultipleConnections = false)
            {
                var ni = Script.Write<NodeInterface>("new BaklavaJS.RendererVue.TextInputInterface({0}, {1})", name, value);
                ni["allowMultipleConnections"] = allowMultipleConnections;
                return ni;
            }
        }


        public static Node CreateGraphNodeType(GraphTemplate template) => Script.Write<Node>("BaklavaJS.Core.createGraphNodeType({0})", template);
        public static Node DefineDynamicNode(DynamicNodeDefinition nodeDefinition) => Script.Write<Node>("BaklavaJS.Core.defineDynamicNode({0})", nodeDefinition);
        public static AbstractNodeConstructor DefineNode(DynamicNodeDefinition definition) => Script.Write<AbstractNodeConstructor>("BaklavaJS.Core.defineNode({0})", definition);
        public static string GetGraphNodeTypeString(GraphTemplate template) => Script.Write<string>("BaklavaJS.Core.getGraphNodeTypeString({0})", template);


        public dom.HTMLElement Render() => _parent.Render();

        public string GetState(bool formated = false)
        {
            if (formated)
            {
                return es5.JSON.stringify(_viewModel.displayedGraph.save(), (double[])null, 4);
            }
            else
            {
                return es5.JSON.stringify(_viewModel.displayedGraph.save());
            }
        }

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

            public ReadOnlyArray<Connection> connections() { return null; }
            public bool destroying() { return false; }
            public ReadOnlyArray<IGraphInterface> inputs() { return null; }
            public bool loading() { return false; }
            public ReadOnlyArray<Node> nodes() { return null; }
            public ReadOnlyArray<IGraphInterface> outputs() { return null; }

            public void addConnection(NodeInterface from, NodeInterface to) { }
            public void addNode(NodeInterface node) { }
            public CheckConnectionResult checkConnection(NodeInterface from, NodeInterface to) { return null; }
            public void destroy() { }
            public Node findNodeById(string id) { return null; }
            public NodeInterface findNodeInterface(string id) { return null; }
            public void load(IGraphState state) { }
            public void removeConnection(Connection connection) { }
            public void removeNode(Node node) { }
            [Template("{this}.save()")] public IGraphState save() { return null; }
        }

        [ObjectLiteral]
        public class ViewModel
        {
            public Graph displayedGraph;
            public Editor editor;
            public IHistory history;
            public readonly bool isSubgraph;
            public IViewSettings settings;
            public void switchGraph(Graph newGraph) { }
            public void switchGraph(GraphTemplate newGraph) { }

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
            public string title;
            public string type;
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
            public ReadOnlyArray<IGraphInterface> inputs() { return null; }
            public string name() { return null; }
            public ReadOnlyArray<IGraphInterface> outputs() { return null; }
            public Graph createGraph() { return null; }
            public Graph createGraph(Graph graph) { return null; }
            public IGraphTemplateState save() { return null; }
            public void update(IGraphState state) { }
            public static GraphTemplate fromGraph(Graph graph, Editor editor) { return null; }
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
        public class NodeState
        {

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
            public object value() { return null; }
            public void value(object value) { }
            public void load(INodeInterfaceState state) { }
            public INodeInterfaceState save() {  return null; }
            public void setComponent(object component) { }
            public void setHidden(bool hidden) { }
            public void setPort(bool value) { }
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
        public class IGraphState
        {

        }

        [ObjectLiteral]
        public class IRegisterNodeTypeOptions
        {

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
    }
}
