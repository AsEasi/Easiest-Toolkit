using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace Easiest.DialogueSystem
{
    public sealed class EDS_GraphWindow : EditorWindow
    {
        #region Params
        public static EDS_GraphWindow Current { get; private set; }
        public EDS_DialogueHolder CurrentDialogueHolder { get; set; }
        public EDS_Variables VarialbeHolder { get; private set; }
        public Dictionary<int, List<EDS_GraphElement>> LayerElements { get; private set; }
        public Dictionary<string, EDS_WindowTempData> TempData { get; private set; }
        public EDS_EventMonitor EventMonitor { get; private set; }
        public EDS_WindowSetting WindowSetting { get; private set; }
        public EDS_GraphView GraphView { get; private set; }
        public EDS_TopBar TopBar { get; private set; }
        public Rect ReliablePosition { get; private set; }

        #endregion

        #region Const
        const string Default_Setting_Path = "Editor/Default EDS_Window Setting";

        // Layers
        public int Graph_View_Layer { get => 0; }
        public int Menu_Layer { get => 10; }
        #endregion

        #region Main
        [MenuItem("ETK/Dialogue System Window")]
        private static void OpenWindow()
        {
            EDS_GraphWindow _window = GetWindow<EDS_GraphWindow>();

            _window.titleContent = new GUIContent("Easiest Dialogue System");

            _window.minSize = new Vector2(100, 100);
        }
        private void OnEnable()
        {
            Current = this;

            VarialbeHolder = new EDS_Variables();
            TempData = new();
            LayerElements = new();
            EventMonitor = new EDS_EventMonitor();
            WindowSetting = Resources.Load<EDS_WindowSetting>(Default_Setting_Path);

            AddTopBar();
            AddGraphView();

            RegisterEventCallback();
        }
        public void OnDisable()
        {
            Current = null;
        }
        public void OnGUI()
        {
            List<int> _layers = new List<int>(LayerElements.Keys);

            EDS_Caculator.QuickSort(ref _layers, 0, _layers.Count - 1);

            if (_layers.Count > 0)
                for (int i = 0; i < _layers.Count; i++)
                {
                    var _elementList = LayerElements[_layers[i]];

                    if (_elementList.Count > 0)
                        for (int e = 0; e < _elementList.Count; e++)
                        {
                            _elementList[e].OnGUI();

                            // If showonce, remove.
                            if (_elementList[e].AutoRemove)
                                _elementList.RemoveAt(e);
                        }
                }

            // TODO: clear temp data.

            EventMonitor.EventMonitor();
        }
        private void Update()
        {
            if (ReliablePosition != position)
            {
                ReliablePosition = position;
                OnWindowPisitionChanged();
            }
        }
        public EDS_WindowTempData GetTempData(string _key)
        {
            if (!TempData.ContainsKey(_key))
            {
                var _newValue = new EDS_WindowTempData();

                TempData.Add(_key, _newValue);
            }

            return TempData[_key];
        }
        public void DrawAllLayer()
        {
            List<int> _layers = new List<int>(LayerElements.Keys);

            EDS_Caculator.QuickSort(ref _layers, 0, _layers.Count - 1);

            if (_layers.Count > 0)
                for (int i = 0; i < _layers.Count; i++)
                {
                    var _elementList = LayerElements[_layers[i]];

                    List<EDS_GraphElement> _removeList = new();

                    if (_elementList.Count > 0)
                        for (int e = 0; e < _elementList.Count; e++)
                        {
                            _elementList[e].OnGUI();

                            // If showonce, remove.
                            if (_elementList[e].AutoRemove)
                                _removeList.Add(_elementList[e]);
                        }

                    // Begin remove

                    for (int r = 0; r < _removeList.Count; r++)
                    {
                        _elementList.Remove(_removeList[r]);
                    }
                }
        }
        public void DrawLayer(int _layer)
        {
            if (LayerElements == null) return;
            if (!LayerElements.ContainsKey(_layer))
            {
                return;
            }

            List<EDS_GraphElement> _removeList = new();

            var _elementList = LayerElements[_layer];

            if (_elementList.Count > 0)
                for (int e = 0; e < _elementList.Count; e++)
                {
                    _elementList[e].OnGUI();

                    // If showonce, remove.
                    if (_elementList[e].AutoRemove)
                        _removeList.Add(_elementList[e]);
                }

            // Begin remove

            for (int r = 0; r < _removeList.Count; r++)
            {
                _elementList.Remove(_removeList[r]);
            }
        }
        public bool DeleteLayer(int _layer)
        {
            if (LayerElements == null) return false;

            if (LayerElements.ContainsKey(_layer))
                LayerElements.Remove(_layer);

            return true;
        }
        public void DeleteAllLayers()
        {
            if (LayerElements == null) return;
            LayerElements.Clear();
        }
        public void AddElement(EDS_GraphElement _element, int _layer = 0, bool _autoRemove = true)
        {
            if (LayerElements == null) LayerElements = new();

            if (!LayerElements.ContainsKey(_layer))
                LayerElements.Add(_layer, new List<EDS_GraphElement>());

            // Repeat check, don't know if its nessessary. ?? Maybe think this in future.
            if (LayerElements[_layer].Contains(_element) || _element == null) return;

            _element.AutoRemove = _autoRemove;

            LayerElements[key: _layer].Add(_element);
        }
        public bool RemoveElement(EDS_GraphElement _element, int _layer = 0)
        {
            List<int> _keyList = new List<int>(LayerElements.Keys);

            for (int i = 0; i < _keyList.Count; i++)
            {
                var _elementList = LayerElements[_keyList[i]];

                if (_elementList.Contains(_element))
                {
                    _elementList.Remove(_element);

                    return true;
                }
            }

            return false;
        }
        #endregion

        #region Func
        private void AddTopBar()
        {
            TopBar = new EDS_TopBar(this);

            TopBar.AutoRemove = false;

            TopBar.ElementRect = new Rect(0, 0, position.width, 24);

            AddElement(TopBar, Menu_Layer, false);
        }
        private void AddGraphView()
        {
            GraphView = new EDS_GraphView(this);

            GraphView.ElementRect = new Rect(0, 24, position.width, position.height - 24);

            AddElement(GraphView, Graph_View_Layer, false);
        }

        private void OnWindowPisitionChanged()
        {
            LayerElements.Clear();

            // : Top bar

            AddTopBar();

            // : Graph view

            Vector2 _scrollPos = GraphView.scrollPos;
            Rect _scrollArea = GraphView.scrollArea;

            AddGraphView();

            GraphView.scrollPos = _scrollPos;
            GraphView.scrollArea = _scrollArea;

            // : End

            GUI.changed = true;
        }
        private void RegisterEventCallback()
        {
            EventMonitor.onMouseDown += (_event) =>
            {
                switch (_event.button)
                {
                    case ((int)Enum_MouseButton.Left):
                        {
                            break;
                        }

                    case ((int)Enum_MouseButton.Right): // Right button
                        {
                            CallRightClickMenu(_event.mousePosition);
                            break;
                        }

                    case ((int)Enum_MouseButton.Middle):
                        {
                            break;
                        }
                }
            };

            EventMonitor.onMouseUp += (_event) =>
            {
                switch (_event.button)
                {
                    case ((int)Enum_MouseButton.Left):
                        {
                            break;
                        }

                    case ((int)Enum_MouseButton.Right): // Right button
                        {
                            break;
                        }

                    case ((int)Enum_MouseButton.Middle):
                        {

                            break;
                        }
                }
            };

            EventMonitor.onMouseDrag += (_event) =>
            {
                switch (_event.button)
                {
                    case ((int)Enum_MouseButton.Left):
                        {
                            break;
                        }

                    case ((int)Enum_MouseButton.Right): // Right button
                        {
                            break;
                        }

                    case ((int)Enum_MouseButton.Middle):
                        {

                            GUI.changed = true;

                            break;
                        }
                }
            };
        }
        private void CallRightClickMenu(Vector2 _mousePosition)
        {
            GenericMenu _menu = new GenericMenu();

            Rect _graphViewLocalPosition = new Rect(_mousePosition.x + GraphView.scrollPos.x, _mousePosition.y + GraphView.scrollPos.y, 0, 0);

            _menu.AddItem(new GUIContent("New Node(Normal)"), false, () =>
            {
                CurrentDialogueHolder.dialogues.Add(new EDS_DialogueHolder.Dialogue(_graphViewLocalPosition, Enum_DialogueType.Normal));
            });
            _menu.AddItem(new GUIContent("New Node(Staight)"), false, () =>
            {
                CurrentDialogueHolder.dialogues.Add(new EDS_DialogueHolder.Dialogue(_graphViewLocalPosition, Enum_DialogueType.Straight));
            });
            _menu.AddItem(new GUIContent("New Node(Branch)"), false, () =>
            {
                CurrentDialogueHolder.dialogues.Add(new EDS_DialogueHolder.Dialogue(_graphViewLocalPosition, Enum_DialogueType.Branch));
            });
            _menu.AddItem(new GUIContent("New Node(Events)"), false, () =>
            {
                CurrentDialogueHolder.dialogues.Add(new EDS_DialogueHolder.Dialogue(_graphViewLocalPosition, Enum_DialogueType.Events));
            });

            _menu.ShowAsContext();
        }
        public void CreateNewDialogueHolder()
        {
            // Select path
            string _path = EditorUtility.SaveFilePanelInProject("Select save floder", "DialogueHolder", "asset", "Msg");

            // Create
            AssetDatabase.CreateAsset(CreateInstance<EDS_DialogueHolder>(), _path);
        }
        #endregion
    }

    public class EDS_GraphView : EDS_GraphElement
    {
        #region Params
        public Vector2 scrollPos;
        public Rect scrollArea;

        public const int Edge_Layer = 1;
        public const int Node_Layer = 2;
        public const int Connecting_Edge_Layer = 3;

        public List<EDS_Edge> Edges { get; }
        public List<EDS_Node> Nodes { get; }

        // Connection
        public bool isConnecting
        {
            get { return ParentWindow.GetTempData(ElementID).GraphView_isConnecting; }
            set { ParentWindow.GetTempData(ElementID).GraphView_isConnecting = value; }
        }
        public EDS_Port connectingPort
        {
            get { return ParentWindow.GetTempData(ElementID).GraphView_connectingPort; }
            set { ParentWindow.GetTempData(ElementID).GraphView_connectingPort = value; }
        }
        public EDS_Edge connectingEdge
        {
            get { return ParentWindow.GetTempData(ElementID).GraphView_connectingEdge; }
            protected set { ParentWindow.GetTempData(ElementID).GraphView_connectingEdge = value; }
        }

        public override string ElementID => "GraphView";

        #endregion

        public EDS_GraphView(EDS_GraphWindow _parentWindow)
        {
            ParentWindow = _parentWindow;
            Edges = new();
            Nodes = new();

            scrollPos = new Vector2(0, 0);
            scrollArea = new Rect(0, 0, 10000, 10000);
        }

        #region Implementation

        public override void OnGUI()
        {
            base.OnGUI();

            scrollPos = GUI.BeginScrollView(ElementRect, scrollPos, scrollArea);

            DrawGrid();

            DeleteLayer(Node_Layer);
            DeleteLayer(Edge_Layer);

            Edges.Clear();
            Nodes.Clear();


            // TODO !!!!! Optimize this behaviour

            AddNodes();
            DrawLayer(Node_Layer);
            AddEdges();
            DrawLayer(Edge_Layer);
            DrawLayer(Connecting_Edge_Layer);

            GUI.EndScrollView();
        }

        protected override void OnMouseUp(Event _event)
        {
            base.OnMouseUp(_event);

            if (isConnecting && connectingPort != null)
            {
                EndConnection();
            }
        }

        protected override void OnMouseDrag(Event _event)
        {
            // Graph viewport dragging
            if (ElementRect.Contains(_event.mousePosition))
                if (_event.button == ((int)Enum_MouseButton.Middle))
                {
                    scrollPos -= _event.delta;

                    _event.Use();
                }

            // Connection edge
            if (isConnecting && connectingEdge != null)
            {
                connectingEdge.EndPosition = _event.mousePosition - new Vector2(ElementRect.x, ElementRect.y) + scrollPos;
            }

            GUI.changed = true;
        }

        #endregion

        private void DrawGrid()
        {
            // Rect _area = new Rect(0, 0, scrollArea.width, scrollArea.height);
            Rect _area = scrollArea;

            EditorGUI.DrawRect(_area, EDS_GUIStyleProvider.Grid_Background_Color);
            EDS_GridDrawer.DrawGrid(_area, Vector2.zero, 20, new Color(1, 1, 1, 0.1f));
            EDS_GridDrawer.DrawGrid(_area, Vector2.zero, 200, new Color(1, 1, 1, 0.1f));
        }

        private void AddNodes()
        {
            if (ParentWindow.CurrentDialogueHolder != null &&
                ParentWindow.CurrentDialogueHolder.dialogues.Count > 0)
            {
                for (int i = 0; i < ParentWindow.CurrentDialogueHolder.dialogues.Count; i++)
                {
                    EDS_Node _node = new EDS_Node(ParentWindow.CurrentDialogueHolder.dialogues[i], ParentWindow);

                    _node.ElementRect = _node.Dialogue.nodeInfo.nodePos;

                    AddElement(_node, Node_Layer);
                    Nodes.Add(_node);
                }
            }
        }

        private void AddEdges()
        {
            var _nodes = Nodes;

            if (_nodes.Count > 0)
                for (int i = 0; i < _nodes.Count; i++)
                {
                    var _outputs = _nodes[i].outputPorts;

                    if (_outputs.Count > 0)
                        for (int o = 0; o < _outputs.Count; o++)
                        {
                            string _ID = _outputs[o].PortOutput.nextDialogueID;

                            // Begin finding match node
                            if (_ID == "" || _ID == null) continue;
                            for (int n = 0; n < _nodes.Count; n++)
                            {
                                if (_nodes[n].Dialogue.ID == _ID)
                                {
                                    // Found
                                    var _newEdge = CreateEdge(_outputs[o].ConnectionPosition, _nodes[n].inputPort.ConnectionPosition);

                                    AddElement(_newEdge, Edge_Layer);
                                    Edges.Add(_newEdge);

                                    break;
                                }
                            }
                        }
                }
        }

        private EDS_Edge CreateEdge(Vector2 _start, Vector2 _end)
        {
            return new EDS_Edge(_start, _end);
        }

        private void HightlightAvailablePort()
        {

        }
    
        public void StartConnection(EDS_Port _sourcePort)
        {
            connectingPort = _sourcePort;
            isConnecting = true;

            // Disconnect old
            if (_sourcePort.PortType == Enum_PortType.Output)
            {
                _sourcePort.PortOutput.nextDialogueID = "";
            }

            connectingEdge = new EDS_Edge(
                connectingPort.ConnectionPosition,
                Event.current.mousePosition - new Vector2(ElementRect.x, ElementRect.y) + scrollPos);

            AddElement(connectingEdge, Connecting_Edge_Layer, false);
        }
        private void EndConnection()
        {
            bool _found = false;
            Vector2 _localMousePosition = Event.current.mousePosition - new Vector2(ElementRect.x, ElementRect.y) + scrollPos;

            if (Nodes.Count > 0)
            for (int n = 0; n < Nodes.Count; n++)
            {
                if (connectingPort.PortType == Enum_PortType.Input)
                {
                    if (Nodes[n].outputPorts.Count > 0)
                    for (int pt = 0; pt < Nodes[n].outputPorts.Count; pt++)
                    {
                        if (Nodes[n].outputPorts[pt].ElementRect.Contains(_localMousePosition))
                        {
                            // If they are belong to same node, result is same as not found
                            if (connectingPort.ParentNode.ElementID == Nodes[n].outputPorts[pt].ParentNode.ElementID)
                            {
                                _found = false;
                                break;
                            }

                            // Found
                            Nodes[n].outputPorts[pt].PortOutput.nextDialogueID = connectingPort.PortInputDialogue.ID;
                            Debug.Log("Found");
                            _found = true;
                            break;
                        }
                    }
                }
                else if (connectingPort.PortType == Enum_PortType.Output)
                {
                    if (Nodes[n].inputPort.ElementRect.Contains(_localMousePosition))
                    {
                        // If they are belong to same node, result is same as not found
                        if (connectingPort.ParentNode.ElementID == Nodes[n].inputPort.ParentNode.ElementID)
                        {
                            _found = false;
                            break;
                        }

                        // Found
                        connectingPort.PortOutput.nextDialogueID = Nodes[n].inputPort.PortInputDialogue.ID;

                        _found = true;
                    }
                }

                if (_found) break;
            }

            // Mouse position not on a valid port
            if (!_found)
            {
                // Disconnect
                if (connectingPort.PortType == Enum_PortType.Output)
                {
                    connectingPort.PortOutput.nextDialogueID = "";
                }
            }

            // End connection
            connectingPort = null;
            RemoveElement(connectingEdge, Connecting_Edge_Layer);
            connectingEdge = null;
            isConnecting = false;
        }
    }
    public class EDS_Node : EDS_GraphElement
    {
        public EDS_DialogueHolder.Dialogue Dialogue { get; }

        public EDS_Port inputPort;
        public List<EDS_Port> outputPorts;

        const int Port_Layer = 1;

        private Rect mainContainerRect
        {
            get
            {
                switch (Dialogue.dialogueType)
                {
                    case Enum_DialogueType.Normal:
                        return new Rect(ElementRect.x, ElementRect.y, mainContinerWidth, titleContainerRect.height + buttonContainerHeight + contentContainerHeight + singleOutputHeight * Dialogue.outputs.Count);

                    case Enum_DialogueType.Straight:
                        return new Rect(ElementRect.x, ElementRect.y, mainContinerWidth, titleContainerRect.height + buttonContainerHeight + contentContainerHeight);

                    case Enum_DialogueType.Branch:
                        return new Rect(ElementRect.x, ElementRect.y, mainContinerWidth, titleContainerRect.height + buttonContainerHeight + singleOutputHeight * Dialogue.outputs.Count);

                    case Enum_DialogueType.Events:
                        return new Rect(ElementRect.x, ElementRect.y, mainContinerWidth, titleContainerRect.height + buttonContainerHeight + contentContainerHeight);

                    default:
                        return Rect.zero;
                }
            }
        }
        private Rect titleContainerRect
        { get { return new Rect(ElementRect.x, ElementRect.y, mainContinerWidth, titleContainerHeight); } }
        private Rect buttonContainerRect
        { get { return new Rect(ElementRect.x, ElementRect.y + titleContainerHeight, mainContinerWidth, buttonContainerHeight); } }
        private Rect contentContainerRect
        {
            get
            {
                switch (Dialogue.dialogueType)
                {
                    case Enum_DialogueType.Normal:
                        return new Rect(ElementRect.x, ElementRect.y + titleContainerHeight + buttonContainerHeight, mainContinerWidth, contentContainerHeight);

                    case Enum_DialogueType.Straight:
                        return new Rect(ElementRect.x, ElementRect.y + titleContainerHeight + buttonContainerHeight, mainContinerWidth, contentContainerHeight);

                    case Enum_DialogueType.Branch:
                        return Rect.zero;

                    case Enum_DialogueType.Events:
                        return new Rect(ElementRect.x, ElementRect.y + titleContainerHeight + buttonContainerHeight, mainContinerWidth, contentContainerHeight);

                    default:
                        return Rect.zero;
                }
            }
        }
        private Rect outputContainerRect
        {
            get
            {
                switch (Dialogue.dialogueType)
                {
                    case Enum_DialogueType.Normal:
                        return new Rect(ElementRect.x, ElementRect.y + titleContainerHeight + buttonContainerHeight + contentContainerHeight, mainContinerWidth, singleOutputHeight * Dialogue.outputs.Count);

                    case Enum_DialogueType.Straight:
                        return Rect.zero;

                    case Enum_DialogueType.Branch:
                        return new Rect(ElementRect.x, ElementRect.y + titleContainerHeight + buttonContainerHeight, mainContinerWidth, singleOutputHeight * Dialogue.outputs.Count);

                    case Enum_DialogueType.Events:
                        return Rect.zero;

                    default:
                        return Rect.zero;
                }
            }
        }
        private int mainContinerWidth { get => 260; }
        private int buttonContainerHeight { get => 21; }
        private int titleContainerHeight { get => 41; }
        private int contentContainerHeight
        {
            get
            {
                switch (Dialogue.dialogueType)
                {
                    case Enum_DialogueType.Normal:
                        return 121;

                    case Enum_DialogueType.Straight:
                        return 121;

                    case Enum_DialogueType.Branch:
                        return singleOutputHeight * Dialogue.outputs.Count;

                    case Enum_DialogueType.Events:
                        return 121;

                    default:
                        return 0;
                }
            }
        }
        private int singleOutputHeight
        {
            get
            {
                switch (Dialogue.dialogueType)
                {
                    case Enum_DialogueType.Normal:
                        return 41;

                    case Enum_DialogueType.Straight:
                        return 0;

                    case Enum_DialogueType.Branch:
                        return 41;

                    case Enum_DialogueType.Events:
                        return 0;

                    default:
                        return 0;
                }
            }
        }

        private Vector2 contentScrollPos
        {
            get { return ParentWindow.GetTempData(ElementID).Node_contentScrollPos; }
            set { ParentWindow.GetTempData(ElementID).Node_contentScrollPos = value; }
        }
        private bool isMouseDown
        {
            get { return ParentWindow.GetTempData(ElementID).Node_isMouseDown; }
            set { ParentWindow.GetTempData(ElementID).Node_isMouseDown = value; }
        }

        public EDS_Node(EDS_DialogueHolder.Dialogue _dialogue, EDS_GraphWindow _perentWindow)
        {
            Dialogue = _dialogue;
            ElementRect = Dialogue.nodeInfo.nodePos;
            ParentWindow = _perentWindow;
            outputPorts = new List<EDS_Port>();
        }

        #region Implementation

        public override string ElementID => Dialogue.ID;

        public override void OnGUI()
        {
            inputPort = null;
            outputPorts.Clear();

            Draw();
            base.OnGUI();
        }

        protected override void OnMouseDown(Event _event)
        {
            if (_event.button == ((int)Enum_MouseButton.Left))
            {
                // Node dragging
                if (titleContainerRect.Contains(_event.mousePosition))
                {
                    isMouseDown = true;

                    _event.Use();
                }
            }
        }

        protected override void OnMouseDrag(Event _event)
        {
            if (_event.button == ((int)Enum_MouseButton.Left))
            {
                // Node dragging
                if (isMouseDown)
                {
                    ElementRect = new Rect(
                        ElementRect.x + _event.delta.x,
                        ElementRect.y + _event.delta.y,
                        ElementRect.width, ElementRect.height);

                    UpdateSourceData();

                    _event.Use();

                    GUI.changed = true;
                }
            }
        }

        protected override void OnMouseUp(Event _event)
        {
            if (_event.button == ((int)Enum_MouseButton.Left))
            {
                // Node dragging
                if (isMouseDown)
                {
                    isMouseDown = false;

                    UpdateSourceData();

                    GUI.changed = true;
                }
            }
        }

        #endregion

        #region Func

        private void UpdateSourceData()
        {
            Dialogue.nodeInfo.nodePos = mainContainerRect;
        }

        private void Draw()
        {
            DeleteLayer(Port_Layer);

            // Container

            DrawContainer();

            // Title

            DrawTitle();

            // Buttons

            DrawButtons();

            // Contents

            switch (Dialogue.dialogueType)
            {
                case Enum_DialogueType.Normal:
                    {
                        DrawNode_Normal();
                        break;
                    }

                case Enum_DialogueType.Straight:
                    {
                        DrawNode_Staight();
                        break;
                    }

                case Enum_DialogueType.Branch:
                    {
                        DrawNode_Branch();
                        break;
                    }

                case Enum_DialogueType.Events:
                    {
                        DrawNode_Events();
                        break;
                    }
            }

            DrawOutline();

            DrawLayer(Port_Layer);

            UpdateSourceData();
        }

        private void DrawOutline()
        {
            Rect _mainRect = mainContainerRect;

            Rect _top = new Rect(_mainRect.x - 2, _mainRect.y - 2, _mainRect.width + 4, 2);
            Rect _bottom = new Rect(_mainRect.x - 2, _mainRect.y + _mainRect.height, _mainRect.width + 4, 2);
            Rect _left = new Rect(_mainRect.x - 2, _mainRect.y - 2, 2, _mainRect.height + 4);
            Rect _right = new Rect(_mainRect.x + _mainRect.width, _mainRect.y - 2, 2, _mainRect.height + 4);
            EditorGUI.DrawRect(_top, Color.black);
            EditorGUI.DrawRect(_bottom, Color.black);
            EditorGUI.DrawRect(_left, Color.black);
            EditorGUI.DrawRect(_right, Color.black);
        }

        private void DrawContainer()
        {
            EditorGUI.DrawRect(mainContainerRect, EDS_GUIStyleProvider.General_Background_Color);
        }

        private void DrawTitle()
        {
            Rect _rect = titleContainerRect;

            Color _titleColor;

            switch (Dialogue.dialogueType)
            {
                case Enum_DialogueType.Normal:
                    _titleColor = ParentWindow.WindowSetting.normalNodeColor;
                    break;

                case Enum_DialogueType.Straight:
                    _titleColor = ParentWindow.WindowSetting.staightNodeColor;
                    break;

                case Enum_DialogueType.Branch:
                    _titleColor = ParentWindow.WindowSetting.branchNodeColor;
                    break;

                case Enum_DialogueType.Events:
                    _titleColor = ParentWindow.WindowSetting.eventsNodeColor;
                    break;

                default:
                    _titleColor = EDS_GUIStyleProvider.Node_Element_Background_Color;
                    break;
            }

            EditorGUI.DrawRect(_rect, _titleColor);

            // : Input Port

            Rect _rectA = new Rect(titleContainerRect.x + 16, titleContainerRect.y + titleContainerHeight * 0.5f - EDS_Port.DefaultPortSize.y * 0.5f,
                EDS_Port.DefaultPortSize.x, EDS_Port.DefaultPortSize.y);

            EDS_Port _inputPort = new EDS_Port(Enum_PortType.Input, Dialogue, this);

            _inputPort.ElementRect = _rectA;

            inputPort = _inputPort;

            AddElement(_inputPort, Port_Layer);

            GUILayout.BeginArea(_rect);
            GUILayout.BeginHorizontal();
            {
                // : Input port space intend

                EditorGUILayout.GetControlRect(GUILayout.Width(32), GUILayout.Height(40));

                // : Title

                Rect _titleRect = EditorGUILayout.GetControlRect(GUILayout.Width(200), GUILayout.Height(40));

                EditorGUI.LabelField(_titleRect, Dialogue.title, EDS_GUIStyleProvider.NodeTitleLabel());

                // TODO : Draw foldout icon
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            if (Dialogue.dialogueType == Enum_DialogueType.Straight || Dialogue.dialogueType == Enum_DialogueType.Events)
            {
                // : Output port

                Rect _rectB = new Rect(
                    titleContainerRect.x + titleContainerRect.width - 24,
                    titleContainerRect.y + (titleContainerRect.height * 0.5f) - (EDS_Port.DefaultPortSize.x * .5f),
                    EDS_Port.DefaultPortSize.x, EDS_Port.DefaultPortSize.y);

                EDS_Port _outputPort = new EDS_Port(Enum_PortType.Output, Dialogue.outputs[0], this);

                _outputPort.ElementRect = _rectB;

                outputPorts.Add(_outputPort);

                AddElement(_outputPort, Port_Layer);
            }

            _rect.x = ElementRect.x;
            _rect.y += 40;
            _rect.width = 260;
            _rect.height = 1;

            EditorGUI.DrawRect(_rect, Color.black);
        }

        private void DrawButtons()
        {
            Rect _buttonContainerRect = buttonContainerRect;

            GUILayout.BeginArea(_buttonContainerRect);
            GUILayout.BeginHorizontal();

            EditorGUILayout.GetControlRect(GUILayout.Width(mainContinerWidth * 0.6f));

            if (GUILayout.Button("Edit"))
            {
                EDS_DialogueEditWindow.OpenWindow().Dialogue = Dialogue;
            }

            if (GUILayout.Button("Del"))
            {
                ParentWindow.CurrentDialogueHolder.dialogues.Remove(this.Dialogue);
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void DrawNode_Normal()
        {
            // : Content

            Rect _contentRect = contentContainerRect;

            EditorGUI.DrawRect(_contentRect, EDS_GUIStyleProvider.Node_Element_Background_Color);

            _contentRect.x += 8;
            _contentRect.y += 8;
            _contentRect.width -= 16;
            _contentRect.height -= 16;

            GUILayout.BeginArea(_contentRect);
            contentScrollPos = GUILayout.BeginScrollView(contentScrollPos, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            GUILayout.Label(Dialogue.content.content, EditorStyles.wordWrappedLabel);

            GUILayout.EndScrollView();
            GUILayout.EndArea();

            // : Outputs

            Rect _outputContainerRect = outputContainerRect;

            // :: Output background

            GUILayout.BeginArea(_outputContainerRect);
            EditorGUILayout.BeginVertical();

            for (int i = 0; i < Dialogue.outputs.Count; i++)
            {
                Rect _singleRect = new Rect(EditorGUILayout.GetControlRect(GUILayout.Width(mainContinerWidth), GUILayout.Height(singleOutputHeight - 1)));

                EditorGUI.DrawRect(_singleRect, EDS_GUIStyleProvider.Node_Output_Background_Color);
            }

            EditorGUILayout.EndVertical();
            GUILayout.EndArea();

            // :: Output content

            for (int i = 0; i < Dialogue.outputs.Count; i++)
            {
                Rect _rect = new Rect(
                    _outputContainerRect.x + 16,
                    _outputContainerRect.y + singleOutputHeight * i,
                    _outputContainerRect.width - 16,
                    singleOutputHeight);

                GUILayout.BeginArea(_rect);
                GUILayout.BeginHorizontal();

                // content

                Rect _rectA = EditorGUILayout.GetControlRect(GUILayout.Width(_rect.width - (32 + 8)), GUILayout.Height(singleOutputHeight - 1));

                GUI.Label(_rectA, Dialogue.outputs[i].content);

                GUILayout.EndHorizontal();
                GUILayout.EndArea();

                // port

                EDS_Port _port = new EDS_Port(Enum_PortType.Output, Dialogue.outputs[i], this);

                _port.ElementRect = new Rect(outputContainerRect.x + _rect.width - (32 + 8) + 16,
                    outputContainerRect.y + (singleOutputHeight * i) + (singleOutputHeight - 1) / 2 - 4,
                    EDS_Port.DefaultPortSize.x, EDS_Port.DefaultPortSize.y);

                outputPorts.Add(_port);

                AddElement(_port, Port_Layer);
            }
        }

        private void DrawNode_Staight()
        {
            // : Content

            Rect _contentRect = contentContainerRect;

            EditorGUI.DrawRect(_contentRect, EDS_GUIStyleProvider.Node_Element_Background_Color);

            _contentRect.x += 8;
            _contentRect.y += 8;
            _contentRect.width -= 16;
            _contentRect.height -= 16;

            GUILayout.BeginArea(_contentRect);
            contentScrollPos = GUILayout.BeginScrollView(contentScrollPos, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            GUILayout.Label(Dialogue.content.content, EditorStyles.wordWrappedLabel);

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawNode_Branch()
        {
            // : Outputs

            Rect _outputContainerRect = outputContainerRect;

            // :: Output background

            GUILayout.BeginArea(_outputContainerRect);
            EditorGUILayout.BeginVertical();

            for (int i = 0; i < Dialogue.outputs.Count; i++)
            {
                Rect _singleRect = new Rect(EditorGUILayout.GetControlRect(GUILayout.Width(mainContinerWidth), GUILayout.Height(singleOutputHeight - 1)));

                EditorGUI.DrawRect(_singleRect, EDS_GUIStyleProvider.Node_Output_Background_Color);
            }

            EditorGUILayout.EndVertical();
            GUILayout.EndArea();

            // :: Output content

            for (int i = 0; i < Dialogue.outputs.Count; i++)
            {
                Rect _rect = new Rect(
                    _outputContainerRect.x + 16,
                    _outputContainerRect.y + singleOutputHeight * i,
                    _outputContainerRect.width - 16,
                    singleOutputHeight);

                GUILayout.BeginArea(_rect);
                GUILayout.BeginHorizontal();

                // content

                Rect _rectA = EditorGUILayout.GetControlRect(GUILayout.Width(_rect.width - (32 + 8)), GUILayout.Height(singleOutputHeight - 1));

                GUI.Label(_rectA, Dialogue.outputs[i].content);

                GUILayout.EndHorizontal();
                GUILayout.EndArea();

                // port

                EDS_Port _port = new EDS_Port(Enum_PortType.Output, Dialogue.outputs[i], this);

                _port.ElementRect = new Rect(outputContainerRect.x + _rectA.width + 16,
                    outputContainerRect.y + (singleOutputHeight * i) + (singleOutputHeight - 1) / 2 - 4,
                    EDS_Port.DefaultPortSize.x, EDS_Port.DefaultPortSize.y);

                outputPorts.Add(_port);

                AddElement(_port, Port_Layer);
            }

        }

        private void DrawNode_Events()
        {
            // : Events

            Rect _contentRect = contentContainerRect;

            EditorGUI.DrawRect(_contentRect, EDS_GUIStyleProvider.Node_Element_Background_Color);

            _contentRect.x += 8;
            _contentRect.y += 8;
            _contentRect.width -= 16;

            GUILayout.BeginArea(_contentRect);
            contentScrollPos = GUILayout.BeginScrollView(contentScrollPos, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            for (int i = 0; i < Dialogue.outputs[0].events.Count; i++)
            {
                GUILayout.Label(Dialogue.outputs[0].events[i].itemName, EditorStyles.wordWrappedLabel);
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        #endregion
    }
    public class EDS_Port : EDS_GraphElement
    {
        public Enum_PortType PortType { get; }
        public Enum_UIElementStatus PortStatus
        {
            get { return ParentWindow.GetTempData(ElementID).Port_status; }
            protected set { ParentWindow.GetTempData(ElementID).Port_status = value; }
        }
        public bool isMouseDown
        {
            get { return ParentWindow.GetTempData(ElementID).Port_isMouseDown; }
            protected set { ParentWindow.GetTempData(ElementID).Port_isMouseDown = value; }
        }

        public Vector2 ConnectionPosition
        { get { return new Vector2(ElementRect.x + ElementRect.width * .5f, ElementRect.y + ElementRect.height * .5f); } }

        public static Vector2 DefaultPortSize { get { return new Vector2(12, 12); } }

        public EDS_DialogueHolder.Dialogue PortInputDialogue { get; set; }
        public EDS_DialogueHolder.DialogueOutput PortOutput { get; set; }
        public EDS_Node ParentNode { get; }

        public EDS_Port(Enum_PortType _portType, EDS_DialogueHolder.Dialogue _portInputDialogue, EDS_Node _parentNode)
        {
            PortType = _portType;
            PortInputDialogue = _portInputDialogue;
            ParentWindow = EDS_GraphWindow.Current;
            ParentNode = _parentNode;
        }

        public EDS_Port(Enum_PortType _portType, EDS_DialogueHolder.DialogueOutput _portOutput, EDS_Node _parentNode)
        {
            PortType = _portType;
            PortOutput = _portOutput;
            ParentWindow = EDS_GraphWindow.Current;
            ParentNode = _parentNode;
        }

        #region Implementation

        public override string ElementID => ElementRect.ToString();

        public override void OnGUI()
        {
            Draw();
            base.OnGUI();
        }

        protected override void OnMouseDown(Event _event)
        {
            if (ElementRect.Contains(_event.mousePosition))
            {
                isMouseDown = true;

                // Begin connection
                EDS_GraphWindow.Current.GraphView.StartConnection(this);

                // Change status
                PortStatus = Enum_UIElementStatus.Focus;

                Draw();

                _event.Use();
            }
        }

        protected override void OnMouseUp(Event _event)
        {
            isMouseDown = false;

            // : Status

            PortStatus = Enum_UIElementStatus.Normal;

            Draw();

            GUI.changed = true;
        }

        #endregion

        private void Draw()
        {
            switch (PortStatus)
            {
                case Enum_UIElementStatus.Normal:
                    EditorGUI.DrawRect(ElementRect, Color.gray);
                    break;
                case Enum_UIElementStatus.Hover:
                    EditorGUI.DrawRect(ElementRect, Color.white);
                    break;
                case Enum_UIElementStatus.Focus:
                    EditorGUI.DrawRect(ElementRect, Color.white);
                    break;
            }
        }
    }
    public class EDS_Edge : EDS_GraphElement
    {
        public Vector2 StartPosition { get; set; }
        public Vector2 EndPosition { get; set; }

        public override string ElementID => StartPosition.ToString() + EndPosition.ToString();

        public EDS_Edge(Vector2 _start, Vector2 _end)
        {
            StartPosition = _start;
            EndPosition = _end;
        }

        public override void OnGUI()
        {
            base.OnGUI();
            Draw();
        }

        private void Draw()
        {
            if (StartPosition.x <= EndPosition.x)
            {
                Handles.DrawBezier(     //绘制通过给定切线的起点和终点的纹理化贝塞尔曲线
                                StartPosition,    //startPosition	贝塞尔曲线的起点。
                                EndPosition,   //endPosition	贝塞尔曲线的终点。
                                StartPosition - Vector2.left * 50f,   //startTangent	贝塞尔曲线的起始切线。
                                EndPosition + Vector2.left * 50f,  //endTangent	贝塞尔曲线的终点切线。
                                Color.white,        //color	    要用于贝塞尔曲线的颜色。
                                null,               //texture	要用于绘制贝塞尔曲线的纹理。
                                2f                  //width	    贝塞尔曲线的宽度。
                                );
            }
            else
            {
                Handles.DrawBezier(     //绘制通过给定切线的起点和终点的纹理化贝塞尔曲线
                                EndPosition,    //startPosition	贝塞尔曲线的起点。
                                StartPosition,   //endPosition	贝塞尔曲线的终点。
                                EndPosition - Vector2.left * 50f,   //startTangent	贝塞尔曲线的起始切线。
                                StartPosition + Vector2.left * 50f,  //endTangent	贝塞尔曲线的终点切线。
                                Color.white,        //color	    要用于贝塞尔曲线的颜色。
                                null,               //texture	要用于绘制贝塞尔曲线的纹理。
                                2f                  //width	    贝塞尔曲线的宽度。
                                );
            }
        }
    }
    public class EDS_Sidebar : EDS_GraphElement
    {
        public Enum_ListMenuType ListMenuType => Enum_ListMenuType.Vertical;

        public override string ElementID => "Sidebar";

        const int Menu_Item_Layer = 1;

        public EDS_Sidebar(EDS_GraphWindow _parentWindow)
        {
            ParentWindow = _parentWindow;
        }

        #region Implementation

        public override void OnGUI()
        {
            base.OnGUI();
        }

        #endregion
    }
    public class EDS_TopBar : EDS_GraphElement
    {
        public Enum_ListMenuType ListMenuType => Enum_ListMenuType.Horizontal;

        public override string ElementID => "TopBar";

        const int Menu_Item_Layer = 1;

        public EDS_TopBar(EDS_GraphWindow _parentWindow)
        {
            ParentWindow = _parentWindow;

            LayerElements = new();

            AddDialogueHolderSetting();
        }

        #region Implem overrideentation

        public override void OnGUI()
        {
            Rect _topBarRect = ElementRect;

            EditorGUI.DrawRect(_topBarRect, EDS_GUIStyleProvider.General_Background_Color);

            GUILayout.BeginHorizontal();

            if (LayerElements.ContainsKey(Menu_Item_Layer))
                DrawLayer(Menu_Item_Layer);

            GUILayout.EndHorizontal();

            base.OnGUI();
        }

        #endregion

        private void AddDialogueHolderSetting()
        {
            EDS_MenuDialogueHolder _dialogueHolderSetting = new EDS_MenuDialogueHolder(ParentWindow);

            AddElement(_dialogueHolderSetting, Menu_Item_Layer, false);
        }
    }
    public class EDS_MenuDialogueHolder : EDS_GraphElement
    {
        public EDS_MenuDialogueHolder(EDS_GraphWindow _parentWindow)
        {
            ParentWindow = _parentWindow;
        }

        public override string ElementID => "MenuDialogueHolder";

        #region Implementation

        public override void OnGUI()
        {
            base.OnGUI();

            ParentWindow.CurrentDialogueHolder = (EDS_DialogueHolder)EditorGUILayout.ObjectField("Dialogue Holder", ParentWindow.CurrentDialogueHolder, typeof(EDS_DialogueHolder), false);

            if (GUILayout.Button("New")) { ParentWindow.CreateNewDialogueHolder(); }
        }

        #endregion
    }

    public class EDS_WindowTempData
    {
        public bool GraphView_isConnecting;
        public EDS_Port GraphView_connectingPort;
        public EDS_Edge GraphView_connectingEdge;

        public bool Node_isMouseDown;
        public Vector2 Node_contentScrollPos;

        public Enum_UIElementStatus Port_status;
        public bool Port_isMouseDown;
    }
}