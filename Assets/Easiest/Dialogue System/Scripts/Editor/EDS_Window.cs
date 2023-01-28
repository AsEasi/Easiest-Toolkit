// #define CLASSICAL
#define GRAPHVIEW

#if CLASSICAL
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using static Easiest.EEditorUtility;
using System.Text;
using System;
using UnityEditor.IMGUI.Controls;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
#endif
#if GRAPHVIEW
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Easiest;
using System;
#endif

namespace Easiest.DialogueSystem
{
#if CLASSICAL
    public class EDS_Window : EditorWindow
    {
    #region class
        public class DialogueNode
        {

            #region class
            public struct NodeConnection
            {
                public Rect rect;
                public Vector2 connectionPos;
                public Vector2 toPos;

                public NodeConnection(Rect _rect, Vector2 _connectionPos)
                {
                    rect = _rect;
                    connectionPos = _connectionPos;
                    toPos = Vector2.zero;
                }
                public NodeConnection(Rect _rect, Vector2 _connectionPos, Vector2 _toPos)
                {
                    rect = _rect;
                    connectionPos = _connectionPos;
                    toPos = _toPos;
                }
            }
            #endregion

            public EDS_Dialogue.DialogueHolder holder;

            private NodeConnection inConnection;
            private List<NodeConnection> outConnection = new();

            private bool isConnecting;
            private Vector2 fromPos;
            private Vector2 toPos;

            private Rect NodeRect
            {
                get
                {
                    return new Rect(holder.nodeInfo.nodePos.x, holder.nodeInfo.nodePos.y,
                        200, 140);
                }
            }
            private bool isDragging;

            public DialogueNode(EDS_Dialogue.DialogueHolder _holder, EDS_Window _edsWindow = null)
            {
                holder = _holder;

                if (_holder.title == "" || _holder.title == null)
                    _holder.title = "EMPTY";
            }

            public void Draw()
            {
                DrawMain();
                DrawInConnection();
                DrawOutConnection();
                DrawConnecttionLines(new Color(1, 1, 1, 1));

                if (isConnecting)
                {
                    toPos = Event.current.mousePosition;
                    GUI.changed = true;
                }
            }

            private void DrawMain()
            {
                if (holder.nodeInfo.nodePos == null)
                    holder.nodeInfo.nodePos = Vector2.zero;

                // Background
                EditorGUI.LabelField(NodeRect, "", GUI.skin.window);

                // Layout area
                Rect _areaRect = new Rect(
                    NodeRect.x + 4, NodeRect.y + 4,
                    NodeRect.width - 8, NodeRect.height - 8);
                GUILayout.BeginArea(_areaRect);

                EditorGUILayout.BeginHorizontal();
                // Title
                EditorGUILayout.LabelField(holder.title, EditorStyles.boldLabel, GUILayout.Width(_areaRect.width * .8f));
                if (GUILayout.Button("X", GUILayout.Width(_areaRect.width * .2f)))
                {
                    EDS_Window.current.RemoveNode(holder);
                }
                EditorGUILayout.EndHorizontal();
                // Draw actor name
                EditorGUILayout.LabelField(holder.actor);
                GUIHorizontalLine(new Color(.5f, .5f, .5f, .5f), 1, 4);
                // Draw text field
                EditorGUILayout.SelectableLabel(holder.content.content, EditorStyles.wordWrappedLabel, GUILayout.ExpandHeight(true));
                // Edit button
                if (GUILayout.Button("Edit"))
                {
                    var _infoWindow = DialogueInfoWindow.GetWindow();
                    _infoWindow.Init(holder);
                }

                GUILayout.EndArea();
            }
            private void DrawInConnection()
            {
                Rect _rect = new Rect(
                    NodeRect.x - 28,
                    NodeRect.y,
                    20, 24);

                GUILayout.BeginArea(_rect);

                if (GUILayout.Button("锟斤拷", GUILayout.Width(20), GUILayout.Height(24)))
                {
                    isConnecting = false;
                }
                inConnection = new(_rect, new Vector2(_rect.x - 4, _rect.y + 12));

                GUILayout.EndArea();
            }
            private void DrawOutConnection()
            {
                Rect _rect = new Rect(
                    NodeRect.x + NodeRect.width + 8,
                    NodeRect.y,
                    20, 24);

                outConnection.Clear();

                for (int i = 0; holder.choices.Count > 0 && i < holder.choices.Count; i++)
                {
                    if (GUI.Button(_rect, (i + 1).ToString()))
                    {
                        isConnecting = true;
                        fromPos = Event.current.mousePosition;
                    }

                    EDS_Dialogue.DialogueHolder _holder = EDS_Window.current.currentDialogue.dialogueHolders.Find(
                        (EDS_Dialogue.DialogueHolder _listHolder) => 
                        { return _listHolder.ID == holder.choices[i].nextDialogueID; });
                    DialogueNode _node = EDS_Window.current.GetNode(_holder);

                    if (_holder != null && _node != null)
                    {
                        Vector2 _toPos = _node.inConnection.connectionPos;
                        outConnection.Add(new NodeConnection(_rect,
                            new Vector2(_rect.x + 24, _rect.y + 12), _toPos));
                    }

                    _rect.y += 24;
                }
            }
            private void DrawConnecttionLines(Color _color)
            {
                Handles.color = _color;

                Handles.BeginGUI();
                GUILayout.BeginArea(new Rect(0, 0, EDS_Window.current.position.width, EDS_Window.current.position.height));

                // Out lines
                foreach (var _connection in outConnection)
                {
                    Handles.DrawLine(_connection.connectionPos, _connection.toPos);
                }

                if (isConnecting)
                {
                    Handles.DrawLine(fromPos, toPos);
                }

                GUILayout.EndArea();
                Handles.EndGUI();
            }

            public void EventCheck()
            {
                Event _event = Event.current;
                switch (_event.type)
                {
                    case EventType.MouseDown:
                        {
                            if (_event.button == 0) // Left down
                            {
                                if (NodeRect.Contains(_event.mousePosition))
                                {
                                    isDragging = true;
                                }
                            }
                            if (_event.button == 2) //Middle down
                            {
                                isDragging = true;
                            }
                        }
                        break;

                    case EventType.MouseUp:
                        {
                            if (_event.button == 0) // Left up
                            {
                                isDragging = false;
                            }
                            else if (_event.button == 2) // Middle up
                            {
                                isDragging = false;
                            }
                        }
                        break;

                    case EventType.MouseDrag:
                        {
                            if (isDragging)
                            {
                                BeenDrag(_event.delta);
                            }
                        }
                        break;
                }
            }

            private void BeenDrag(Vector2 _delta)
            {
                holder.nodeInfo.nodePos += _delta;
                GUI.changed = true;
            }
        }
        public class DialogueInfoWindow : EditorWindow
        {
            private EDS_Dialogue.DialogueHolder holder;
            private SerializedObject dialogueSObj;

            private ReorderableList contentConditionsROList;
            private ReorderableList contentEventsROList;
            private ReorderableList contentChoicesROList;
            private ReorderableList choiceConditionsROList;
            private ReorderableList choiceEventsROList;

            private EDS_Dialogue.DialogueChoice focusedChoice;

            private EDS_DialogueAdvancedDropDown dialogueDropDown;

            public static DialogueInfoWindow GetWindow()
            {
                DialogueInfoWindow _window = GetWindow<DialogueInfoWindow>();

                _window.titleContent = new GUIContent("Dialogue Info");
                _window.Show();

                return _window;
            }

            private void OnGUI()
            {
                Undo.RecordObject(EDS_Window.current.currentDialogue, "Dialogue Info changed");

                if (EDS_Window.current.currentDialogue == null)
                {
                    EditorGUILayout.HelpBox("Select a dialogue to edit", MessageType.Info);
                    return;
                }

                GUI.enabled = false;
                // ID
                EditorGUILayout.TextField(holder.ID.ToString());
                GUI.enabled = true;

                // Title
                LayoutHorizontalTitle("Title");
                holder.title = EditorGUILayout.TextField(holder.title);
                // Actor
                LayoutHorizontalTitle("Actor");
                holder.actor = EditorGUILayout.TextField(holder.actor);
                GUIHorizontalLine(new Color(.5f, .5f, .5f, .5f));

                // Content
                LayoutHorizontalTitle("Content");
                holder.content.enable = EditorGUILayout.ToggleLeft("Enable", holder.content.enable);
                GUI.enabled = holder.content.enable;
                // Show text
                holder.content.content = EditorGUILayout.TextArea(holder.content.content, WordWrapTextArea());
                // Ex content
                if (holder.content.showExContent)
                {
                    if (GUILayout.Button("Hide"))
                    {
                        holder.content.showExContent = false;
                    }
                }
                else
                {
                    if (GUILayout.Button("Show Extra Content"))
                    {
                        holder.content.showExContent = true;
                    }
                }
                if (holder.content.showExContent)
                {
                    EditorGUILayout.BeginHorizontal();

                    // Show conditions
                    contentConditionsROList.DoLayoutList();
                    // Show events
                    contentEventsROList.DoLayoutList();

                    EditorGUILayout.EndHorizontal();
                }
                GUI.enabled = true;
                GUIHorizontalLine(new Color(.5f, .5f, .5f, .5f));

                // Choices
                LayoutHorizontalTitle("Choices");
                contentChoicesROList.DoLayoutList();
                // Choice extra content
                if (choiceConditionsROList != null ||
                    choiceEventsROList != null)
                {
                    GUILayout.BeginHorizontal();
                    choiceConditionsROList.DoLayoutList();
                    choiceEventsROList.DoLayoutList();
                    GUILayout.EndHorizontal();
                }

                dialogueSObj.Update();
            }

            public void Init(EDS_Dialogue.DialogueHolder _holder)
            {
                holder = _holder;

                dialogueSObj = new SerializedObject(EDS_Window.current.currentDialogue);
                dialogueDropDown = new EDS_DialogueAdvancedDropDown(new AdvancedDropdownState());

                contentConditionsROList = new ReorderableList(holder.content.conditions,
                    typeof(EDS_Dialogue.DialogueCondition),
                    true, true, true, true)
                {
                    drawHeaderCallback = (Rect rect) =>
                    {
                        EditorGUI.LabelField(rect, "Conditions");
                    },
                    onAddCallback = (ReorderableList list) =>
                    {
                        holder.content.conditions.Add(new EDS_Dialogue.DialogueCondition());
                    },
                    elementHeightCallback = (int index) =>
                    {
                        if (holder.content.conditions.Count > 0 &&
                            holder.content.conditions[index].open &&
                            holder.content.conditions[index].parameters.Count > 0)
                        {
                            return 20 * holder.content.conditions[index].parameters.Count + 52;
                        }
                        else
                            return 24;
                    },
                    drawElementCallback = DrawElementCallback_Condition
                };

                contentEventsROList = new ReorderableList(holder.content.events,
                    typeof(EDS_Dialogue.DialogueEvent),
                    true, true, true, true)
                {
                    drawHeaderCallback = (Rect rect) =>
                    {
                        EditorGUI.LabelField(rect, "Events");
                    },
                    onAddCallback = (ReorderableList list) =>
                    {
                        holder.content.events.Add(new EDS_Dialogue.DialogueEvent());
                    },
                    elementHeightCallback = (int index) =>
                    {
                        if (holder.content.events.Count > 0 &&
                            holder.content.events[index].open)
                        {
                            if (holder.content.events[index].parameters.Count > 0)
                                return 20 * holder.content.events[index].parameters.Count + 72;
                            else
                                // Show execute time
                                return 48;
                        }
                        else
                            return 24;
                    },
                    drawElementCallback = DrawElementCallback_Event
                };

                contentChoicesROList = new ReorderableList(holder.choices,
                    typeof(EDS_Dialogue.DialogueChoice),
                    true, false, true, true)
                {
                    drawHeaderCallback = (Rect rect) =>
                    {
                        EditorGUI.LabelField(rect, "Choices");
                    },
                    onAddCallback = (ReorderableList list) =>
                    {
                        holder.choices.Add(new EDS_Dialogue.DialogueChoice());
                    },
                    onRemoveCallback = (ReorderableList list) =>
                    {
                        for (int i = 0; i < holder.choices.Count; i++)
                        {
                            if (list.IsSelected(i))
                            {
                                holder.choices.RemoveAt(i);
                                break;
                            }
                        }
                        ResetChoiceExStatus();
                    },
                    elementHeightCallback = (int index) =>
                    {
                        return 24;
                    },
                    drawElementCallback = DrawElementCallback_Choice
                };
            }

            private void RegenerateChoiceExContentROList(EDS_Dialogue.DialogueChoice _choice, int _index)
            {
                focusedChoice = _choice;

                choiceConditionsROList = new ReorderableList(_choice.conditions,
                    typeof(EDS_Dialogue.DialogueChoice),
                    true, true, true, true)
                {
                    drawHeaderCallback = (Rect rect) =>
                    {
                        StringBuilder _header = new StringBuilder("No.");
                        _header.Append(_index.ToString());
                        _header.Append("  Conditions");
                        EditorGUI.LabelField(rect, _header.ToString());
                    },
                    onAddCallback = (ReorderableList list) =>
                    {
                        _choice.conditions.Add(new EDS_Dialogue.DialogueCondition());
                    },
                    elementHeightCallback = (int index) =>
                    {
                        if (_choice.conditions.Count > 0 &&
                            _choice.conditions[index].open &&
                            _choice.conditions[index].parameters.Count > 0)
                        {
                            return 20 * _choice.conditions[index].parameters.Count + 52;
                        }
                        else
                            return 24;
                    },
                    drawElementCallback = DrawElementCallback_ChoiceCondition
                };

                choiceEventsROList = new ReorderableList(_choice.events,
                    typeof(EDS_Dialogue.DialogueEvent),
                    true, true, true, true)
                {
                    drawHeaderCallback = (Rect rect) =>
                    {
                        StringBuilder _header = new StringBuilder("No.");
                        _header.Append(_index.ToString());
                        _header.Append("  Events");
                        EditorGUI.LabelField(rect, _header.ToString());
                    },
                    onAddCallback = (ReorderableList list) =>
                    {
                        _choice.events.Add(new EDS_Dialogue.DialogueEvent());
                    },
                    elementHeightCallback = (int index) =>
                    {
                        if (_choice.events.Count > 0 &&
                            _choice.events[index].open &&
                            _choice.events[index].parameters.Count > 0)
                        {
                            return 20 * _choice.events[index].parameters.Count + 52;
                        }
                        else
                            return 24;
                    },
                    drawElementCallback = DrawElementCallback_ChoiceEvent
                };
            }

            private void DrawElementCallback_Condition(Rect rect, int index, bool isActive, bool isFocused)
            {
                var _condition = holder.content.conditions[index];

                Rect _rectA = new(rect.x, rect.y + 2, rect.width, 20);

                StringBuilder _buttonName = new StringBuilder(
                    _condition.itemName == "" ||
                    _condition.itemName == null ?
                    "*Condition*" : _condition.itemName);

                if (_condition.itemType != null)
                {
                    _buttonName.Append("(");
                    _buttonName.Append(_condition.itemType.ToString());
                    _buttonName.Append(")");
                }

                Rect _foldoutRect = new(_rectA.x, _rectA.y, 16, _rectA.height);
                _condition.open = EditorGUI.Foldout(_foldoutRect, _condition.open, "");

                Rect _buttonRect = new(_rectA.x + 16, _rectA.y, _rectA.width - 16, _rectA.height);
                if (GUI.Button(_buttonRect, _buttonName.ToString()))
                {
                    dialogueDropDown.OnItemSelectOnce += (AdvancedDropdownItem _item) =>
                    {
                        _condition.itemName = _item.name;
                    };
                    dialogueDropDown.type = EDS_DialogueAdvancedDropDown.DropDownType.DialogueCondition;
                    dialogueDropDown.Show(_buttonRect);
                }

                // If been fold
                if (!_condition.open) return;

                if (_condition.itemName != null)
                    // Contain check
                    if (EDS_DialogueCondition.conditionInfos.ContainsKey(_condition.itemName))
                    {
                        // Get value check
                        if (EDS_DialogueCondition.conditionInfos.TryGetValue(_condition.itemName, out _condition.itemType))
                        {
                            // Null value check
                            if (_condition.itemType != null)
                            {
                                Rect _rectB = new Rect(_rectA.x, _rectA.y + 8, _rectA.width, 20);

                                // Get fields
                                FieldInfo[] _fieldInfos = _condition.itemType.GetFields();

                                // Clear dictionary
                                foreach (var _param in _condition.parameters)
                                {
                                    bool _find = false;

                                    foreach (var _info in _fieldInfos)
                                    {
                                        if (_param.Key == _info)
                                        {
                                            _find = true;
                                            break;
                                        }
                                    }

                                    if (!_find)
                                    {
                                        _condition.parameters.Remove(_param.Key);
                                    }
                                }


                                // Set rect
                                _rectB.y += 20;

                                // Show parameters
                                for (int i = 0; i < _fieldInfos.Length; i++)
                                {
                                    // Public value
                                    if (_fieldInfos[i].IsPublic)
                                    {

                                        // Set value
                                        if (_fieldInfos[i].FieldType == typeof(int))
                                        {
                                            if (!_condition.parameters.ContainsKey(_fieldInfos[i]))
                                                _condition.parameters.Add(_fieldInfos[i], 0);

                                            // Set
                                            _condition.parameters[_fieldInfos[i]] = EditorGUI.IntField(_rectB,
                                                _fieldInfos[i].Name,
                                                (int)_condition.parameters[_fieldInfos[i]]);
                                        }
                                        else if (_fieldInfos[i].FieldType == typeof(float))
                                        {
                                            if (!_condition.parameters.ContainsKey(_fieldInfos[i]))
                                                _condition.parameters.Add(_fieldInfos[i], 0f);

                                            // Set
                                            _condition.parameters[_fieldInfos[i]] = EditorGUI.FloatField(_rectB,
                                                _fieldInfos[i].Name,
                                                (float)_condition.parameters[_fieldInfos[i]]);
                                        }
                                        else if (_fieldInfos[i].FieldType == typeof(string))
                                        {
                                            if (!_condition.parameters.ContainsKey(_fieldInfos[i]))
                                                _condition.parameters.Add(_fieldInfos[i], "");

                                            // Set
                                            _condition.parameters[_fieldInfos[i]] = EditorGUI.TextField(_rectB,
                                                _fieldInfos[i].Name,
                                                (string)_condition.parameters[_fieldInfos[i]]);
                                        }
                                        else if (_fieldInfos[i].FieldType == typeof(UnityEngine.Object))
                                        {
                                            if (!_condition.parameters.ContainsKey(_fieldInfos[i]))
                                                _condition.parameters.Add(_fieldInfos[i], Vector4.zero);

                                            // Set
                                            _condition.parameters[_fieldInfos[i]] = EditorGUI.ObjectField(_rectB,
                                                label: _fieldInfos[i].Name,
                                                obj: _condition.parameters[_fieldInfos[i]] as UnityEngine.Object,
                                                typeof(UnityEngine.Object), false);
                                        }

                                        // Set rect
                                        _rectB.y += 20;
                                    }
                                    else continue;
                                }

                                // Show reset button
                                if (GUI.Button(_rectB, "Reset"))
                                {
                                    _condition.parameters.Clear();
                                }
                            }
                            else
                            {

                            }
                        }
                        else
                        {
                            Debug.LogError("Failed to find value.");
                        }
                    }
                    else
                    {
                        //EditorGUI.HelpBox()
                    }
            }
            private void DrawElementCallback_Event(Rect rect, int index, bool isActive, bool isFocused)
            {
                var _event = holder.content.events[index];

                Rect _rectA = new(rect.x, rect.y + 2, rect.width, 20);

                StringBuilder _buttonName = new StringBuilder(
                    _event.itemName == "" ||
                    _event.itemName == null ?
                    "*Event*" : _event.itemName);

                if (_event.itemType != null)
                {
                    _buttonName.Append("(");
                    _buttonName.Append(_event.itemType.ToString());
                    _buttonName.Append(")");
                }

                Rect _foldoutRect = new(_rectA.x, _rectA.y, 16, 20);
                _event.open = EditorGUI.Foldout(_foldoutRect, _event.open, "");

                Rect _buttonRect = new(_rectA.x + 16, _rectA.y, _rectA.width - 16, 20);
                if (GUI.Button(_buttonRect, _buttonName.ToString()))
                {
                    dialogueDropDown.OnItemSelectOnce += (AdvancedDropdownItem _item) =>
                    {
                        _event.itemName = _item.name;
                    };
                    dialogueDropDown.type = EDS_DialogueAdvancedDropDown.DropDownType.DialogueEvent;
                    dialogueDropDown.Show(_buttonRect);
                }

                // If been fold
                if (!_event.open) return;

                // Show execute time
                Rect _enumRect = new(_rectA.x, _rectA.y + 28, _rectA.width, 20);
                _event.executeTime = (EDS_Dialogue.DialogueEvent.EventExecuteTime)EditorGUI.EnumPopup(_enumRect, "ExecuteTime", _event.executeTime);

                _rectA.y += 28;

                if (_event.itemName != null)
                    // Contain check
                    if (EDS_DialogueCondition.conditionInfos.ContainsKey(_event.itemName))
                    {
                        // Get value check
                        if (EDS_DialogueCondition.conditionInfos.TryGetValue(_event.itemName, out _event.itemType))
                        {
                            // Null value check
                            if (_event.itemType != null)
                            {
                                Rect _rectB = new Rect(_rectA.x, _rectA.y, _rectA.width, 20);

                                // Get fields
                                FieldInfo[] _fieldInfos = _event.itemType.GetFields();

                                // Clear dictionary
                                foreach (var _param in _event.parameters)
                                {
                                    bool _find = false;

                                    foreach (var _info in _fieldInfos)
                                    {
                                        if (_param.Key == _info)
                                        {
                                            _find = true;
                                            break;
                                        }
                                    }

                                    if (!_find)
                                    {
                                        _event.parameters.Remove(_param.Key);
                                    }
                                }


                                // Set rect
                                _rectB.y += 20;

                                // Show parameters
                                for (int i = 0; i < _fieldInfos.Length; i++)
                                {
                                    // Public value
                                    if (_fieldInfos[i].IsPublic)
                                    {

                                        // Set value
                                        if (_fieldInfos[i].FieldType == typeof(int))
                                        {
                                            if (!_event.parameters.ContainsKey(_fieldInfos[i]))
                                                _event.parameters.Add(_fieldInfos[i], 0);

                                            // Set
                                            _event.parameters[_fieldInfos[i]] = EditorGUI.IntField(_rectB,
                                                _fieldInfos[i].Name,
                                                (int)_event.parameters[_fieldInfos[i]]);
                                        }
                                        else if (_fieldInfos[i].FieldType == typeof(float))
                                        {
                                            if (!_event.parameters.ContainsKey(_fieldInfos[i]))
                                                _event.parameters.Add(_fieldInfos[i], 0f);

                                            // Set
                                            _event.parameters[_fieldInfos[i]] = EditorGUI.FloatField(_rectB,
                                                _fieldInfos[i].Name,
                                                (float)_event.parameters[_fieldInfos[i]]);
                                        }
                                        else if (_fieldInfos[i].FieldType == typeof(string))
                                        {
                                            if (!_event.parameters.ContainsKey(_fieldInfos[i]))
                                                _event.parameters.Add(_fieldInfos[i], "");

                                            // Set
                                            _event.parameters[_fieldInfos[i]] = EditorGUI.TextField(_rectB,
                                                _fieldInfos[i].Name,
                                                (string)_event.parameters[_fieldInfos[i]]);
                                        }
                                        else if (_fieldInfos[i].FieldType == typeof(UnityEngine.Object))
                                        {
                                            if (!_event.parameters.ContainsKey(_fieldInfos[i]))
                                                _event.parameters.Add(_fieldInfos[i], Vector4.zero);

                                            // Set
                                            _event.parameters[_fieldInfos[i]] = EditorGUI.ObjectField(_rectB,
                                                label: _fieldInfos[i].Name,
                                                obj: _event.parameters[_fieldInfos[i]] as UnityEngine.Object,
                                                typeof(UnityEngine.Object), false);
                                        }

                                        // Set rect
                                        _rectB.y += 20;
                                    }
                                    else continue;
                                }

                                // Show reset button
                                if (GUI.Button(_rectB, "Reset"))
                                {
                                    _event.parameters.Clear();
                                }
                            }
                            else
                            {

                            }
                        }
                        else
                        {
                            Debug.LogError("Failed to find value.");
                        }
                    }
                    else
                    {
                        //EditorGUI.HelpBox()
                    }
            }
            private void DrawElementCallback_Choice(Rect rect, int index, bool isActive, bool isFocused)
            {
                var _choice = holder.choices[index];

                Rect _rect = new(rect.x, rect.y + 2, rect.width * .05f, 20);
                EditorGUI.LabelField(_rect, (index + 1).ToString());

                _rect.x += _rect.width;
                _rect.width = rect.width * 0.75f;
                _choice.content = EditorGUI.TextField(_rect, _choice.content);

                _rect.x += _rect.width;
                _rect.width = rect.width * 0.1f;
                if (GUI.Button(_rect, "Text"))
                {
                    var _window = TextAreaWindow.GetWindow();
                    _window.SetContent(_choice.content);
                    _window.OnSaveOnce += (string _value) =>
                    {
                        _choice.content = _value;
                    };
                }
                _rect.x += _rect.width;
                if (GUI.Button(_rect, "Ex"))
                {
                    if (focusedChoice == _choice)
                        ResetChoiceExStatus();
                    else
                        RegenerateChoiceExContentROList(_choice, index);
                }
            }
            private void DrawElementCallback_ChoiceCondition(Rect rect, int index, bool isActive, bool isFocused)
            {
                if (focusedChoice == null) return;
                var _condition = focusedChoice.conditions[index];

                Rect _rectA = new(rect.x, rect.y + 2, rect.width, 20);

                StringBuilder _buttonName = new StringBuilder(
                    _condition.itemName == "" ||
                    _condition.itemName == null ?
                    "*Condition*" : _condition.itemName);

                if (_condition.itemType != null)
                {
                    _buttonName.Append("(");
                    _buttonName.Append(_condition.itemType.ToString());
                    _buttonName.Append(")");
                }

                Rect _foldoutRect = new(_rectA.x, _rectA.y, 16, _rectA.height);
                _condition.open = EditorGUI.Foldout(_foldoutRect, _condition.open, "");

                Rect _buttonRect = new(_rectA.x + 16, _rectA.y, _rectA.width - 16, _rectA.height);
                if (GUI.Button(_buttonRect, _buttonName.ToString()))
                {
                    dialogueDropDown.OnItemSelectOnce += (AdvancedDropdownItem _item) =>
                    {
                        _condition.itemName = _item.name;
                    };
                    dialogueDropDown.type = EDS_DialogueAdvancedDropDown.DropDownType.DialogueCondition;
                    dialogueDropDown.Show(_buttonRect);
                }

                // If been fold
                if (!_condition.open) return;

                if (_condition.itemName != null)
                    // Contain check
                    if (EDS_DialogueCondition.conditionInfos.ContainsKey(_condition.itemName))
                    {
                        // Get value check
                        if (EDS_DialogueCondition.conditionInfos.TryGetValue(_condition.itemName, out _condition.itemType))
                        {
                            // Null value check
                            if (_condition.itemType != null)
                            {
                                Rect _rectB = new Rect(_rectA.x, _rectA.y + 8, _rectA.width, 20);

                                // Get fields
                                FieldInfo[] _fieldInfos = _condition.itemType.GetFields();

                                // Clear dictionary
                                foreach (var _param in _condition.parameters)
                                {
                                    bool _find = false;

                                    foreach (var _info in _fieldInfos)
                                    {
                                        if (_param.Key == _info)
                                        {
                                            _find = true;
                                            break;
                                        }
                                    }

                                    if (!_find)
                                    {
                                        _condition.parameters.Remove(_param.Key);
                                    }
                                }


                                // Set rect
                                _rectB.y += 20;

                                // Show parameters
                                for (int i = 0; i < _fieldInfos.Length; i++)
                                {
                                    // Public value
                                    if (_fieldInfos[i].IsPublic)
                                    {

                                        // Set value
                                        if (_fieldInfos[i].FieldType == typeof(int))
                                        {
                                            if (!_condition.parameters.ContainsKey(_fieldInfos[i]))
                                                _condition.parameters.Add(_fieldInfos[i], 0);

                                            // Set
                                            _condition.parameters[_fieldInfos[i]] = EditorGUI.IntField(_rectB,
                                                _fieldInfos[i].Name,
                                                (int)_condition.parameters[_fieldInfos[i]]);
                                        }
                                        else if (_fieldInfos[i].FieldType == typeof(float))
                                        {
                                            if (!_condition.parameters.ContainsKey(_fieldInfos[i]))
                                                _condition.parameters.Add(_fieldInfos[i], 0f);

                                            // Set
                                            _condition.parameters[_fieldInfos[i]] = EditorGUI.FloatField(_rectB,
                                                _fieldInfos[i].Name,
                                                (float)_condition.parameters[_fieldInfos[i]]);
                                        }
                                        else if (_fieldInfos[i].FieldType == typeof(string))
                                        {
                                            if (!_condition.parameters.ContainsKey(_fieldInfos[i]))
                                                _condition.parameters.Add(_fieldInfos[i], "");

                                            // Set
                                            _condition.parameters[_fieldInfos[i]] = EditorGUI.TextField(_rectB,
                                                _fieldInfos[i].Name,
                                                (string)_condition.parameters[_fieldInfos[i]]);
                                        }
                                        else if (_fieldInfos[i].FieldType == typeof(UnityEngine.Object))
                                        {
                                            if (!_condition.parameters.ContainsKey(_fieldInfos[i]))
                                                _condition.parameters.Add(_fieldInfos[i], Vector4.zero);

                                            // Set
                                            _condition.parameters[_fieldInfos[i]] = EditorGUI.ObjectField(_rectB,
                                                label: _fieldInfos[i].Name,
                                                obj: _condition.parameters[_fieldInfos[i]] as UnityEngine.Object,
                                                typeof(UnityEngine.Object), false);
                                        }

                                        // Set rect
                                        _rectB.y += 20;
                                    }
                                    else continue;
                                }

                                // Show reset button
                                if (GUI.Button(_rectB, "Reset"))
                                {
                                    _condition.parameters.Clear();
                                }
                            }
                            else
                            {

                            }
                        }
                        else
                        {
                            Debug.LogError("Failed to find value.");
                        }
                    }
                    else
                    {
                        //EditorGUI.HelpBox()
                    }
            }
            private void DrawElementCallback_ChoiceEvent(Rect rect, int index, bool isActive, bool isFocused)
            {
                if (focusedChoice == null) return;
                var _event = focusedChoice.events[index];

                Rect _rectA = new(rect.x, rect.y + 2, rect.width, 20);

                StringBuilder _buttonName = new StringBuilder(
                    _event.itemName == "" ||
                    _event.itemName == null ?
                    "*Event*" : _event.itemName);

                if (_event.itemType != null)
                {
                    _buttonName.Append("(");
                    _buttonName.Append(_event.itemType.ToString());
                    _buttonName.Append(")");
                }

                Rect _foldoutRect = new(_rectA.x, _rectA.y, 16, 20);
                _event.open = EditorGUI.Foldout(_foldoutRect, _event.open, "");

                Rect _buttonRect = new(_rectA.x + 16, _rectA.y, _rectA.width - 16, 20);
                if (GUI.Button(_buttonRect, _buttonName.ToString()))
                {
                    dialogueDropDown.OnItemSelectOnce += (AdvancedDropdownItem _item) =>
                    {
                        _event.itemName = _item.name;
                    };
                    dialogueDropDown.type = EDS_DialogueAdvancedDropDown.DropDownType.DialogueEvent;
                    dialogueDropDown.Show(_buttonRect);
                }

                // If been fold
                if (!_event.open) return;

                if (_event.itemName != null)
                    // Contain check
                    if (EDS_DialogueCondition.conditionInfos.ContainsKey(_event.itemName))
                    {
                        // Get value check
                        if (EDS_DialogueCondition.conditionInfos.TryGetValue(_event.itemName, out _event.itemType))
                        {
                            // Null value check
                            if (_event.itemType != null)
                            {
                                Rect _rectB = new Rect(_rectA.x, _rectA.y, _rectA.width, 20);

                                // Get fields
                                FieldInfo[] _fieldInfos = _event.itemType.GetFields();

                                // Clear dictionary
                                foreach (var _param in _event.parameters)
                                {
                                    bool _find = false;

                                    foreach (var _info in _fieldInfos)
                                    {
                                        if (_param.Key == _info)
                                        {
                                            _find = true;
                                            break;
                                        }
                                    }

                                    if (!_find)
                                    {
                                        _event.parameters.Remove(_param.Key);
                                    }
                                }


                                // Set rect
                                _rectB.y += 20;

                                // Show parameters
                                for (int i = 0; i < _fieldInfos.Length; i++)
                                {
                                    // Public value
                                    if (_fieldInfos[i].IsPublic)
                                    {

                                        // Set value
                                        if (_fieldInfos[i].FieldType == typeof(int))
                                        {
                                            if (!_event.parameters.ContainsKey(_fieldInfos[i]))
                                                _event.parameters.Add(_fieldInfos[i], 0);

                                            // Set
                                            _event.parameters[_fieldInfos[i]] = EditorGUI.IntField(_rectB,
                                                _fieldInfos[i].Name,
                                                (int)_event.parameters[_fieldInfos[i]]);
                                        }
                                        else if (_fieldInfos[i].FieldType == typeof(float))
                                        {
                                            if (!_event.parameters.ContainsKey(_fieldInfos[i]))
                                                _event.parameters.Add(_fieldInfos[i], 0f);

                                            // Set
                                            _event.parameters[_fieldInfos[i]] = EditorGUI.FloatField(_rectB,
                                                _fieldInfos[i].Name,
                                                (float)_event.parameters[_fieldInfos[i]]);
                                        }
                                        else if (_fieldInfos[i].FieldType == typeof(string))
                                        {
                                            if (!_event.parameters.ContainsKey(_fieldInfos[i]))
                                                _event.parameters.Add(_fieldInfos[i], "");

                                            // Set
                                            _event.parameters[_fieldInfos[i]] = EditorGUI.TextField(_rectB,
                                                _fieldInfos[i].Name,
                                                (string)_event.parameters[_fieldInfos[i]]);
                                        }
                                        else if (_fieldInfos[i].FieldType == typeof(UnityEngine.Object))
                                        {
                                            if (!_event.parameters.ContainsKey(_fieldInfos[i]))
                                                _event.parameters.Add(_fieldInfos[i], Vector4.zero);

                                            // Set
                                            _event.parameters[_fieldInfos[i]] = EditorGUI.ObjectField(_rectB,
                                                label: _fieldInfos[i].Name,
                                                obj: _event.parameters[_fieldInfos[i]] as UnityEngine.Object,
                                                typeof(UnityEngine.Object), false);
                                        }

                                        // Set rect
                                        _rectB.y += 20;
                                    }
                                    else continue;
                                }

                                // Show reset button
                                if (GUI.Button(_rectB, "Reset"))
                                {
                                    _event.parameters.Clear();
                                }
                            }
                            else
                            {

                            }
                        }
                        else
                        {
                            Debug.LogError("Failed to find value.");
                        }
                    }
                    else
                    {
                        //EditorGUI.HelpBox()
                    }
            }
            private void ResetChoiceExStatus()
            {
                focusedChoice = null;
                choiceConditionsROList = null;
                choiceEventsROList = null;
            }
        }
        public class TextAreaWindow : EditorWindow
        {
            public string content;
            public Action<string> OnSaveOnce;

            public static TextAreaWindow GetWindow()
            {
                TextAreaWindow _window = GetWindow<TextAreaWindow>();

                _window.titleContent = new GUIContent("Text Area");
                _window.minSize = new Vector2(64, 64);
                _window.Show();

                return _window;
            }

            private void OnGUI()
            {
                content = EditorGUILayout.TextArea(content, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Save"))
                {
                    OnSaveOnce?.Invoke(content);

                    var _delegates = OnSaveOnce.GetInvocationList();
                    foreach (var _delegate in _delegates)
                    {
                        OnSaveOnce -= _delegate as Action<string>;
                    }

                    Close();
                }
                if (GUILayout.Button("Cancel"))
                {
                    var _delegates = OnSaveOnce.GetInvocationList();
                    foreach (var _delegate in _delegates)
                    {
                        OnSaveOnce -= _delegate as Action<string>;
                    }

                    content = "";
                    Close();
                }
                GUILayout.EndHorizontal();
            }

            public void SetContent(string _content)
            {
                content = _content;
            }
        }
    #endregion

    #region params

        public static EDS_Window current;
        public EDS_Dialogue currentDialogue;

        public SerializedObject currentDialogueSObj;
        private List<DialogueNode> nodes = new();

        private Vector2 gridOffset;
        private bool isDragging;

    #endregion

    #region const
        string New_Dialogue_Name = "NewDialogue";
        static readonly Color Grid_Color_Dim = new Color(.3f, .3f, .3f, .5f);
        static readonly Color Grid_Color_Gleam = new Color(.4f, .4f, .4f, .5f);
    #endregion

    #region main

        [MenuItem("ETK/DialogueSystem")]
        static void OpenWindow()
        {
            EDS_Window _window = GetWindow<EDS_Window>();

            _window.titleContent = new GUIContent("Easiest Dialogue System");

            _window.Show();

            EditorGUIUtility.labelWidth = 0;
            EditorGUIUtility.fieldWidth = 0;

            current = _window;
        }

        private void Awake()
        {
            Undo.undoRedoPerformed += () =>
            {
                currentDialogueSObj?.Update();

                if (currentDialogue != null)
                {
                    RegenerateSerializedData();
                    RecreateNodeList();
                }
            };

            RegenerateSerializedData();
            RecreateNodeList();

        }

        private void OnGUI()
        {
            currentDialogueSObj?.Update();

            DrawGrid(position, 20, Grid_Color_Dim);
            DrawGrid(position, 100, Grid_Color_Gleam);
            ShowDialogueSetting();
            ShowHelp();
            ShowNodes();

            EventCheck();
        }
    #endregion

    #region func
        private void RegenerateSerializedData()
        {
            if (currentDialogue != null)
                currentDialogueSObj = new SerializedObject(currentDialogue);
        }
        private void RecreateNodeList()
        {
            // Clear
            nodes.Clear();

            // Check
            if (currentDialogue == null ||
                currentDialogue.dialogueHolders == null ||
                currentDialogue.dialogueHolders.Count <= 0) return;

            // Generate
            for (int i = 0; i < currentDialogue.dialogueHolders.Count; i++)
            {
                nodes.Add(new DialogueNode(currentDialogue.dialogueHolders[i]));
            }
        }
        private void ShowDialogueSetting()
        {
            // Draw background
            EditorGUI.LabelField(new Rect(0, 0, position.width, 44), "", GUI.skin.window);

            EditorGUI.BeginChangeCheck();
            // Init dialogue
            currentDialogue = EditorGUILayout.ObjectField("Dialogue", currentDialogue, typeof(EDS_Dialogue), false) as EDS_Dialogue;
            if (EditorGUI.EndChangeCheck())
            {
                RegenerateSerializedData();
                RecreateNodeList();
            }

            // Button to create new dialogue
            if (GUILayout.Button("Create new dialogue"))
            {
                CreateNewDialogue();
            }
        }
        private void ShowHelp()
        {
            GUILayout.BeginArea(new Rect(0, 44, position.width, 80));

            StringBuilder _textBiulder = new StringBuilder("Help:\n");
            _textBiulder.Append("First you need to create or select a dialogue file at top\n");
            _textBiulder.Append("Mouse right click ===> Create node\n");
            _textBiulder.Append("Left mouse button ===> Drag node\n");
            _textBiulder.Append("Middle mouse button ===> Drag canvas");

            GUILayout.Label(_textBiulder.ToString(), EEditorUtility.BigLabel);

            GUILayout.EndArea();
        }
        private void CreateNewDialogue()
        {
            // Select path
            string _path = EditorUtility.SaveFilePanelInProject("Select save floder", New_Dialogue_Name, "asset", "Msg");

            // Create
            AssetDatabase.CreateAsset(CreateInstance<EDS_Dialogue>(), _path);
        }
        private void ShowNodes()
        {
            for (int i = 0; nodes.Count > 0 && i < nodes.Count; i++)
            {
                nodes[i].Draw();
            }
        }
        private void DrawGrid(Rect _rect, int _gridSpacing, Color _gridColor)
        {
            Vector2 _lineCount = new(
                (int)MathF.Ceiling(_rect.height / _gridSpacing),// X row
                (int)MathF.Ceiling(_rect.width / _gridSpacing));// Y column

            Vector2 _gridOffset = new(
                gridOffset.x % _gridSpacing,
                gridOffset.y % _gridSpacing);

            Handles.color = _gridColor;
            Handles.BeginGUI();
            GUILayout.BeginArea(new Rect(0, 0, _rect.width, _rect.height));

            // Horizontal lines
            for (float _posY = _gridOffset.y;
                _lineCount.x > 0;
                _lineCount.x--, _posY += _gridSpacing)
            {
                Handles.DrawLine(
                    new Vector2(-16, _posY),
                    new Vector2(_rect.width + 16, _posY));
            }

            // Vertical lines
            for (float _posX = _gridOffset.x;
                _lineCount.y > 0;
                _lineCount.y--, _posX += _gridSpacing)
            {
                Handles.DrawLine(
                    new Vector2(_posX, -16),
                    new Vector2(_posX, _rect.height + 16));
            }

            GUILayout.EndArea();
            Handles.EndGUI();
        }
        private void EventCheck()
        {
            Event _event = Event.current;
            switch (_event.type)
            {
                case EventType.MouseDown:
                    {
                        if (_event.button == 1) // Right click
                        {
                            if (currentDialogue != null)
                                RightClickMenu();
                        }
                        else if (_event.button == 2) // Middle click
                        {
                            isDragging = true;
                        }
                    }
                    break;

                case EventType.MouseUp:
                    {
                        if (_event.button == 2) // Middle click
                        {
                            isDragging = false;
                        }
                    }
                    break;

                case EventType.MouseDrag:
                    {
                        if (isDragging)
                        {
                            gridOffset += _event.delta;
                        }
                    }
                    break;
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].EventCheck();
            }
        }
        private void RightClickMenu()
        {
            GenericMenu _menu = new GenericMenu();

            _menu.AddItem(new GUIContent("New Node"), false, () => { NewNode(); });

            _menu.ShowAsContext();
        }
        private void NewNode()
        {
            Undo.RegisterCompleteObjectUndo(currentDialogue, "Add Dialogue");

            var _holder = new EDS_Dialogue.DialogueHolder();
            _holder.nodeInfo = new EDS_Dialogue.NodeInfo(new(position.width * 0.5f, position.height * 0.5f), Vector2.zero);
            currentDialogue.dialogueHolders.Add(_holder);

            RegenerateSerializedData();
            RecreateNodeList();
        }
        public void RemoveNode(EDS_Dialogue.DialogueHolder _holder)
        {
            if (currentDialogue.dialogueHolders.Contains(_holder))
            {
                Undo.RegisterCompleteObjectUndo(currentDialogue, "Delete Dialogue");

                currentDialogue.dialogueHolders.Remove(_holder);

                RegenerateSerializedData();
                RecreateNodeList();
            }
        }

    #endregion

    #region API

        public void ApplifyDialogueChange()
        {
            currentDialogueSObj.Update();
        }
        public DialogueNode GetNode(EDS_Dialogue.DialogueHolder _holder)
        {
            return nodes.Find((x) => { return x.holder == _holder; });
        }

    #endregion
    }
#endif

#if GRAPHVIEW
    public class EDS_Window : EditorWindow
    {
        #region Class
        public class DialogueGraphView : GraphView
        {
            public const string StyleSheetPath_EDSGraph = "Editor/Style_EDSGraph";
            public const string StyleSheetPath_EDSNode = "Editor/Style_EDSNode";
            public const string StyleSheetPath_EDSDialogueEditWindow = "Editor/Style_EDSDialogueEditWindow";
            public static readonly Color Back_Ground_Color = new Color(.2f, .2f, .2f, 1);
            private List<DialogueNode> dialogueNodes = new List<DialogueNode>();

            public DialogueGraphView(string _name = "Graph")
            {
                name = _name;
                Init();
            }

            // Override
            public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
            {
                List<Port> _ports = new List<Port>();

                ports.ForEach
                (
                    (_port) =>
                    {
                        if (startPort.node == _port.node ||
                            startPort.direction == _port.direction ||
                            startPort.portType != _port.portType)
                            return;
                        else
                            _ports.Add(_port);
                    }
                );

                return _ports;
            }

            private void Init()
            {
                // Style
                AddStyle();
                // Manipulators
                AddManipulators();
                // Grid BG
                AddGridBackground();
                // : Register callbacks
                SetCallBack_OnGraphViewChanged();
            }
            private void AddStyle()
            {
                styleSheets.Add((StyleSheet)Resources.Load(StyleSheetPath_EDSGraph));
                styleSheets.Add((StyleSheet)Resources.Load(StyleSheetPath_EDSNode));
                styleSheets.Add((StyleSheet)Resources.Load(StyleSheetPath_EDSDialogueEditWindow));
            }
            private void AddGridBackground()
            {
                GridBackground _grid = new GridBackground();

                _grid.StretchToParentSize();

                Insert(0, _grid);
            }
            private void AddManipulators()
            {
                // Zoom
                SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
                // Drag
                this.AddManipulator(new ContentDragger());
                // Selection drag
                this.AddManipulator(new SelectionDragger());
                // Rectangle selection
                this.AddManipulator(new RectangleSelector());
                // Menu add node
                this.AddManipulator(Manipulator_AddNewDialogue());
            }
            public void DrawNodes()
            {
                if (EDS_Window.current.currentDialogueHolder == null) return;
                if (EDS_Window.current.currentDialogueHolder.dialogues.Count <= 0) return;
    
                for (int i = 0; i < EDS_Window.current.currentDialogueHolder.dialogues.Count; i++)
                {
                    EDS_DialogueHolder.Dialogue _dialogue = EDS_Window.current.currentDialogueHolder.dialogues[i];

                    DialogueNode _node = new DialogueNode(_dialogue, _dialogue.nodeInfo.nodePos);

                    _node.SetPosition(_node.Dialogue.nodeInfo.nodePos);

                    this.AddElement(_node);

                    _node.Draw();

                    dialogueNodes.Add(_node);
                }
            }
            public void RemoveNodes()
            {
                foreach (var _node in dialogueNodes)
                {
                    this.RemoveElement(_node);
                }
            }
            public void DeleteNode(DialogueNode _node)
            {
                this.RemoveElement(_node);

                EDS_Window.current.currentDialogueHolder.dialogues.Remove(_node.Dialogue);
            }
            // Manipulators
            private IManipulator Manipulator_AddNewDialogue()
            {
                ContextualMenuManipulator _manipulator = new(
                    (menuEvent) =>
                    {
                        menuEvent.menu.AppendAction
                        ("Add Node", (menuAction) =>
                        {
                            if (current.currentDialogueHolder == null)
                            {
                                return;
                            }

                            // Create new dialogue
                            var _newDialogue = new EDS_DialogueHolder.Dialogue(
                                new Rect(menuAction.eventInfo.mousePosition.x, menuAction.eventInfo.mousePosition.y, 0, 0));

                            Debug.Log(menuAction.eventInfo.mousePosition);

                            // Add at scriptable object
                            EDS_Window.current.currentDialogueHolder.dialogues.Add(_newDialogue);

                            // Create new node
                            var _newNode = new DialogueNode(_newDialogue, _newDialogue.nodeInfo.nodePos);

                            // Regenerate nodes
                            RemoveNodes();
                            DrawNodes();
                        }
                        );
                    });

                return _manipulator;
            }
            // Callback
            private void SetCallBack_OnGraphViewChanged()
            {
                this.graphViewChanged = (_changeData) =>
                {
                    // Record these changes.
                    if (EDS_Window.current.currentDialogueHolder != null)
                        Undo.RecordObject(EDS_Window.current.currentDialogueHolder, "Dialogue Holder Changed");

                    if (_changeData.elementsToRemove != null && _changeData.elementsToRemove.Count > 0)
                    {
                        foreach (var _element in _changeData.elementsToRemove)
                        {
                            // Node
                            if (_element.GetType() == typeof(DialogueNode))
                                EDS_Window.current.currentDialogueHolder.dialogues.Remove(((DialogueNode)_element).Dialogue);
                        }
                    }

                    if (_changeData.movedElements != null && _changeData.movedElements.Count > 0)
                    {
                        foreach (var _element in _changeData.movedElements)
                        {
                            // Node
                            if (_element.GetType() == typeof(DialogueNode))
                            {
                                var _node = (DialogueNode)_element;
                                _node.Dialogue.nodeInfo.nodePos = _node.GetPosition();
                            }
                        }
                    }

                    return _changeData;
                };
            }
        }
        public class DialogueNode : Node
        {
            public EDS_DialogueHolder.Dialogue Dialogue { get; protected set; }

            public DialogueNode(EDS_DialogueHolder.Dialogue _dialogue, Rect _position)
            {
                Dialogue = _dialogue;
                Dialogue.nodeInfo.nodePos = _position;
            }

            public void Draw()
            {
                #region Containers

                // : Setup containers

                // Main

                mainContainer.style.width = 200;
                mainContainer.style.minHeight = 160;

                titleContainer.style.backgroundColor = new Color(40 / 255f, 40 / 255f, 40 / 255f, 1);

                titleContainer.Clear();

                titleContainer.style.backgroundColor = Dialogue.nodeInfo.nodeColor;

                // Sub container

                VisualElement _subContainer = mainContainer.ElementAt(1);

                _subContainer.Clear();

                VisualElement _contentContainer = new VisualElement();
                VisualElement _choicesContainer = new VisualElement();
                VisualElement _buttonContainer = new VisualElement();

                _subContainer.Add(_contentContainer);
                _subContainer.Add(_choicesContainer);
                _subContainer.Add(_buttonContainer);

                // Setup styles

                titleContainer.AddToClassList("EDS-Node-Title-Contianer");
                _subContainer.AddToClassList("EDS-Node-Sub-Container");
                _contentContainer.AddToClassList("EDS-Node-Content-Container");
                _choicesContainer.AddToClassList("EDS-Node-Choices-Container");
                _buttonContainer.AddToClassList("EDS-Node-Button-Container");

                #endregion

                #region Top

                // : Input Port

                Port _inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(string));

                titleContainer.Insert(0, _inputPort);

                _inputPort.name = "EDS_InputPort";
                _inputPort.portName = "";

                // : Title

                Label _title = new Label(Dialogue.title);

                _title.name = "EDS_TitleLabel";

                // :: Color

                Color.RGBToHSV(titleContainer.style.backgroundColor.value, out float H, out float S, out float V);

                _title.style.color = V >= 0.5f ? Color.black : Color.white;

                titleContainer.Add(_title);

                // Auto cut
                // _title.RegisterValueChangedCallback(
                //     (_changeData) =>
                //     {
                //         if (_changeData.newValue.Length >= 8)
                //         {
                //             var _biulder = new System.Text.StringBuilder(_title.text.Substring(0, 8));

                //             _biulder.Append("...");
                            
                //             _title.text = _biulder.ToString();
                //         }
                //     }
                // );

                // : Title color

                ColorField _titleColorField = new ColorField();

                _titleColorField.name = "EDS_TitleColorField";
                _titleColorField.value = titleContainer.style.backgroundColor.value;
                _titleColorField.showAlpha = false;

                _titleColorField.RegisterValueChangedCallback
                (
                    (_changeData) =>
                    {
                        titleContainer.style.backgroundColor = _changeData.newValue;

                        Color.RGBToHSV(_changeData.newValue, out float H, out float S, out float V);

                        _title.style.color = V >= 0.5f ? Color.black : Color.white;

                        // Save
                        Dialogue.nodeInfo.nodeColor = _changeData.newValue;
                        Dialogue.nodeInfo.nodeColor.a = 1;
                    }
                );

                titleContainer.Add(_titleColorField);

                #endregion

                #region Middle

                // : Content

                // Actor

                Label _actorText = new Label(Dialogue.actor);

                _actorText.name = "EDS_Actor";

                _contentContainer.Add(_actorText);

                // Content

                Label _contentText = new Label(Dialogue.content.content);

                _contentText.name = "EDS_Content";

                // _contentContainer.Add(_contentText);

                ScrollView _scrollView = new ScrollView(ScrollViewMode.Vertical);

                _scrollView.verticalScrollerVisibility = ScrollerVisibility.Auto;

                _scrollView.contentContainer.Add(_contentText);

                _contentContainer.Add(_scrollView);

                _scrollView.name = "EDS_ContentScrollView";

                #endregion

                #region Bottom

                // : Output port

                if (Dialogue.choices.Count > 0)
                {
                    for (int i = 0; i < Dialogue.choices.Count; i++)
                    {
                        Port _outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(string));

                        _choicesContainer.Add(_outputPort);

                        _outputPort.userData = Dialogue.choices[i];
                        _outputPort.name = "EDS_OutputPort";
                        _outputPort.portName = Dialogue.choices[i].content;
                    }
                }
                else
                {
                    // Give some tips
                }

                // : Button

                var _editButton = new EDSButton("Edit");

                _editButton.style.width = new StyleLength(new Length(80, LengthUnit.Percent));
                _editButton.style.backgroundColor = new Color(60 / 255f, 60 / 255f, 60 / 255f, 255);

                _editButton.clicked += () =>
                {
                    EDS_Window.current.CreateEditWindow(Dialogue);
                };

                var _removeButton = new EDSButton("x");

                _removeButton.style.backgroundColor = new Color(80 / 255f, 40 / 255f, 40 / 255f, 255);
                _removeButton.style.width = new StyleLength(new Length(20, LengthUnit.Percent));

                _removeButton.clicked += () =>
                {
                    current.DialogueGraph.DeleteNode(this);
                };
                

                _buttonContainer.Add(_editButton);
                _buttonContainer.Add(_removeButton);

                #endregion

                // : End
                RefreshExpandedState();
            }
        }
        public class DialogueEditWindow : VisualElement
        {
            #region Class
            public class EDSReoderableList : VisualElement
            {
                public ListView InnerListView { get; protected set; }

                public event Action beforAdd;
                public event Action afterAdd;
                public event Action<int> beforeRemove;
                public event Action<int> afterRemove;

                private VisualElement elementContainer;

                public EDSReoderableList(System.Collections.IList _source, float itemHeight = -1, Func<VisualElement> makeItem = null, Action<VisualElement, int> bindItem = null)
                {
                    this.name = "EDSReoderableList";

                    InnerListView = new ListView(_source, itemHeight, makeItem, bindItem);

                    InnerListView.showFoldoutHeader = true;
                    InnerListView.selectionType = SelectionType.Single;
                    InnerListView.reorderable = true;
                    InnerListView.reorderMode = ListViewReorderMode.Simple;

                    elementContainer = InnerListView.Q("unity-content-container");

                    this.contentContainer.Add(InnerListView);

                    // : Button container

                    VisualElement _buttonContainer = new VisualElement();

                    _buttonContainer.name = "ButtonContainer";

                    InnerListView.Q("unity-list-view__foldout-header").Add(_buttonContainer);

                    EDSButton _addButton = new EDSButton("+");

                    _addButton.clicked += () =>
                    {
                        beforAdd?.Invoke();

                        InnerListView.Rebuild();

                        afterAdd?.Invoke();
                    };

                    _buttonContainer.Add(_addButton);

                    EDSButton _removeButton = new EDSButton("-");

                    _removeButton.clicked += () =>
                    {
                        int _index = InnerListView.selectedIndex;

                        if (_index < 0 ||
                            _index >= elementContainer.childCount)
                            _index = elementContainer.childCount - 1;

                        beforeRemove?.Invoke(_index);

                        RemoveElement(_index);

                        InnerListView.Rebuild();

                        afterRemove?.Invoke(_index);
                    };

                    _buttonContainer.Add(_removeButton);
                }

                private void AddElement()
                {
                    // InnerListView.makeItem?.Invoke();

                    // int _index = elementContainer.childCount - 1 > 0? 
                    //     elementContainer.childCount - 1: 0;

                    // InnerListView.bindItem?.Invoke(elementContainer.ElementAt(_index), _index);
                }
                private void RemoveElement(int _index)
                {
                    elementContainer.RemoveAt(_index);
                }
            }
            public class ListConditionHolder : VisualElement
            {
                private EDS_DialogueHolder.DialogueCondition condition;
                public EDS_DialogueHolder.DialogueCondition Condition 
                {
                    get 
                    {
                        if (condition != null)
                        {
                            contentContainer.Clear();

                            InitSearchButton();
                        }

                        return condition;
                    }
                    set
                    {
                        condition = value;
                    }
                }

                public ListConditionHolder()
                {
                    name = "EDS_ListConditionHolder";
                }

                private void InitSearchButton()
                {
                    TextField _nameField = new TextField();

                    _nameField.value = condition.itemName;
                    
                    _nameField.SetEnabled(false);

                    contentContainer.Add(_nameField);
                }
            }
            #endregion

            #region Params
            public EDS_DialogueHolder.Dialogue Dialogue { get; protected set; }

            #endregion

            public DialogueEditWindow(EDS_DialogueHolder.Dialogue _dialogue)
            {
                Dialogue = _dialogue;

                Init();
            }

            public void Init()
            {
                styleSheets.Add((StyleSheet)Resources.Load(DialogueGraphView.StyleSheetPath_EDSDialogueEditWindow));

                this.name = "EDS_DialogueEditWindow";
                this.AddToClassList("EDS-Dialogue-Edit-Window");

                AddManipulators();

                this.Add(ContentConditionList());

                    // Dialogue.content.conditions.Add(new EDS_DialogueHolder.DialogueCondition());

            }
            private void AddManipulators()
            {
            }
            private EDSReoderableList ContentConditionList()
            {
                // : Make callbacks

                Func<VisualElement> _makeItem = () => {
                    Debug.Log("MAKE");
                    return new ListConditionHolder();
                };

                Action<VisualElement, int> _bindItem = (_item, _index) => 
                {
                    Debug.Log("BIND");
                    (_item as ListConditionHolder).Condition = Dialogue.content.conditions[index: _index];
                };

                // : Create list

                var _list = new EDSReoderableList(Dialogue.content.conditions, -1, _makeItem, _bindItem);

                // : Register callbacks

                _list.beforAdd += () =>
                {
                    Dialogue.content.conditions.Add(new EDS_DialogueHolder.DialogueCondition());
                };
                _list.beforeRemove += (index) =>
                {
                    Debug.Log(index);
                    Dialogue.content.conditions.RemoveAt(index);
                };

                // : Return

                return _list;
            }
        }
        public class EDSButton : Button
        {
            public EDSButton(string _label, string _elementName = "EDS_Button", System.Action _clickEvent = null)
            {
                this.AddToClassList("EDS_Button");

                this.name = _elementName;

                Label _buttonLabel = new Label(_label);

                _buttonLabel.name = "EDS_ButtonLabel";

                this.Add(_buttonLabel);
            }
        }
        #endregion

        #region Params
        public static EDS_Window current;
        public EDS_DialogueHolder currentDialogueHolder;
        public Vector2 mousePosition;

        public DialogueGraphView DialogueGraph { get; protected set; }
        public Toolbar TopBar { get; protected set; }
        public DialogueEditWindow EditWindow { get; protected set; }
        #endregion

        #region Main
        [MenuItem("ETK/DialogueSystem")]
        private static void OpenWindow()
        {
            EDS_Window _window = GetWindow<EDS_Window>();

            _window.titleContent = new GUIContent("Easiest Dialogue System");

            _window.minSize = new Vector2(100, 100);

            _window.Show();
        }
        public void OnEnable()
        {
            Enable();
        }
        public void OnDisable()
        {
            Disable();
        }
        private void OnGUI() 
        {
            Event e = Event.current;
            mousePosition = e.mousePosition;
        }
        #endregion

        #region Func
        public void Enable()
        {
            current = this;
            InitializeGraphView();
            CreateTopMenu();
        }
        public void Disable()
        {
            RemoveGraphView();
            RemoveTopMenu();
            current = null;
        }
        private void CreateTopMenu()
        {
            TopBar = new Toolbar();

            // topMenu.style.backgroundColor = new Color(.2f, .2f, .2f, 1);;
            // topMenu.style.height = 30;

            rootVisualElement.Add(TopBar);

            // : Dialogue holder object field
            { 
                ObjectField _objectField = new ObjectField("Dialogue Holder");

                _objectField.objectType = typeof(EDS_DialogueHolder);

                _objectField.RegisterValueChangedCallback
                (
                    (_changeData) =>
                    {
                        if (_changeData.newValue == null)
                        {
                            DialogueGraph.RemoveNodes();

                            return;
                        }

                        if (_changeData.newValue.GetType() == typeof(EDS_DialogueHolder))
                        {
                            currentDialogueHolder = (EDS_DialogueHolder) _changeData.newValue;

                            // Clear all nodes
                            DialogueGraph.RemoveNodes();

                            // Recreate nodes
                            DialogueGraph.DrawNodes();
                        }
                        else
                        {
                            Debug.LogError("Failed to set dialogue holder!", this);
                        }
                    }
                );

                TopBar.Add(_objectField);
            }
        }
        private void RemoveTopMenu()
        {
            rootVisualElement.Remove(TopBar);
        }
        public void CreateEditWindow(EDS_DialogueHolder.Dialogue _dialogue)
        {
            if (EditWindow != null)
                rootVisualElement.Remove(EditWindow);

            EditWindow = new DialogueEditWindow(_dialogue);

            rootVisualElement.Add(EditWindow);
        }
        public void RemoveEditWindow()
        {
            rootVisualElement.Remove(EditWindow);
        }
        private void InitializeGraphView()
        {
            DialogueGraph = new DialogueGraphView("Dialogue Graph");

            DialogueGraph.StretchToParentSize();

            rootVisualElement.Add(DialogueGraph);
        }
        private void RemoveGraphView()
        {
            rootVisualElement.Add(DialogueGraph);
        }
        
        #endregion
    }
#endif

    public class EDS_DialogueAdvancedDropDown : AdvancedDropdown
    {
        public enum DropDownType
        {
            All = 0, DialogueCondition, DialogueEvent
        }
        public DropDownType type;
        public System.Action<AdvancedDropdownItem> OnItemSelectOnce;
        public System.Action<AdvancedDropdownItem> OnItemselect;

        public EDS_DialogueAdvancedDropDown(AdvancedDropdownState state) : base(state)
        {
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var _root = new AdvancedDropdownItem("Dialogue Items");

            if (type == DropDownType.All ||
                type == DropDownType.DialogueCondition)
            {
                // Conditions
                var _conditionRoot = new AdvancedDropdownItem("Conditions");
                _root.AddChild(_conditionRoot);

                var _conditionKeys = EDS_DialogueCondition.GetAllKeys();
                foreach (var _key in _conditionKeys)
                {
                    _conditionRoot.AddChild(new AdvancedDropdownItem(_key));
                }
            }

            if (type == DropDownType.All ||
                type == DropDownType.DialogueEvent)
            {
                // Events
                var _eventRoot = new AdvancedDropdownItem("Events");
                _root.AddChild(_eventRoot);

                var _eventKeys = EDS_DialogueEvent.GetAllKeys();
                foreach (var _key in _eventKeys)
                {
                    _eventRoot.AddChild(new AdvancedDropdownItem(_key));
                }
            }

            return _root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            base.ItemSelected(item);

            // Invoke
            OnItemSelectOnce?.Invoke(item);
            OnItemselect?.Invoke(item);

            // Clear delegates
            var _delegates = OnItemSelectOnce.GetInvocationList();
            foreach (var _delegate in _delegates)
            {
                OnItemSelectOnce -= _delegate as System.Action<AdvancedDropdownItem>;
            }
        }
    }
    public class EDS_SearchWindow : ScriptableObject, ISearchWindowProvider
    {
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> _tree = new List<SearchTreeEntry>();

            var _entry = new SearchTreeGroupEntry(new GUIContent("Group"));
            _tree.Add(_entry);

            return _tree;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            Debug.Log(SearchTreeEntry.name);
            return true;
        }
    }
}