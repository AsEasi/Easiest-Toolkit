namespace Easiest.DialogueSystem
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;
    using UnityEditor;
    using UnityEditorInternal;
    using UnityEditor.IMGUI.Controls;
    using UnityEngine;

    public class EDS_DialogueEditWindow : EditorWindow
    {
        public static EDS_DialogueEditWindow OpenWindow()
        {
            EDS_DialogueEditWindow _window = GetWindow<EDS_DialogueEditWindow>("Edit Dialogue");

            _window.minSize = new Vector2(100, 100);

            return _window;
        }

        #region Params
        private EDS_DialogueHolder.Dialogue dialogue;
        public EDS_DialogueHolder.Dialogue Dialogue
        { 
            get
            {
                return dialogue;
            }
            set
            {
                dialogue = value;
                RecreateOutputList();
            } 
        }
        private Vector2 scrollPos;
        private Vector2 contentScrollPos;

        // Lists
        public EDS_OutputList OutputList { get; protected set; }
        public EDS_ExtraContentList ConditionList { get; protected set; }
        public EDS_OutputList EventList { get; protected set; }
        #endregion

        #region Main
        private void OnGUI()
        {
            if (Dialogue == null) EditorGUILayout.HelpBox("Select a dialogue to Edit", MessageType.Warning, true);
            
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(false));

            DrawIDInfo();
            DrawTitle();
            DrawContent();
            DrawOutputs();
            DrawOutputInfo();

            EditorGUILayout.EndScrollView();
        }
        #endregion

        #region Func
        private void RecreateOutputList()
        {
            OutputList = new EDS_OutputList(Dialogue.outputs, typeof(EDS_DialogueHolder.DialogueOutput), Dialogue.dialogueType);
        }
        private void DrawIDInfo()
        {
            GUI.enabled = false;

            EditorGUILayout.TextField(Dialogue.ID);
            
            GUI.enabled = true;
        }
        private void DrawTitle()
        {
            EDS_EditorUtility.DrawBigLabel("Base Info");

            EditorGUILayout.LabelField("Title");

            Dialogue.title = EditorGUILayout.TextField(Dialogue.title);
        }
        private void DrawContent()
        {
            if (Dialogue.dialogueType == Enum_DialogueType.Branch || Dialogue.dialogueType == Enum_DialogueType.Events) return;

            EDS_EditorUtility.DrawBigLabel("Content");

            Dialogue.hasContent = EditorGUILayout.ToggleLeft("Has content", Dialogue.hasContent);
            GUI.enabled = Dialogue.hasContent;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Actor", GUILayout.Width(100));
            Dialogue.usePreviousActor = EditorGUILayout.ToggleLeft("Use previous", Dialogue.usePreviousActor);
            EditorGUILayout.EndHorizontal();

            GUI.enabled = Dialogue.hasContent && !Dialogue.usePreviousActor;
            Dialogue.actor = EditorGUILayout.TextField(Dialogue.actor);
            GUI.enabled = Dialogue.hasContent;

            EditorGUILayout.LabelField("Content");

            contentScrollPos = EditorGUILayout.BeginScrollView(contentScrollPos, GUILayout.Height(120), GUILayout.ExpandWidth(false));
            Dialogue.content.content = EditorGUILayout.TextArea(Dialogue.content.content, EDS_GUIStyleProvider.WordWrapMultilineTextArea(), GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            GUI.enabled = true;
        }
        private void DrawOutputs()
        {
            EDS_EditorUtility.DrawBigLabel("Outputs");

            switch (Dialogue.dialogueType)
            {
                case Enum_DialogueType.Normal:
                    EditorGUILayout.LabelField("Choices");
                    break;
                case Enum_DialogueType.Straight:
                    EditorGUILayout.LabelField("Outputs");
                    break;
                case Enum_DialogueType.Branch:
                    EditorGUILayout.LabelField("Branches");
                    break;
                case Enum_DialogueType.Events:
                    EditorGUILayout.LabelField("Events");
                    break;
            }

            OutputList.DoLayoutList();
        }
        private void DrawOutputInfo()
        {
            if (Dialogue.dialogueType == Enum_DialogueType.Straight) return;

            EDS_EditorUtility.DrawBigLabel("Output Info");

            // Check if out of index

            if (OutputList.selectedIndices.Count <= 0) return;
            if (OutputList.selectedIndices[0] < 0 || OutputList.selectedIndices[0] >= OutputList.count) return;

            // Shortcut : get selecting output

            var _output = (EDS_DialogueHolder.DialogueOutput)OutputList.list[OutputList.selectedIndices[0]];

            // Begin

            EditorGUILayout.LabelField("Connection:");

            Rect _rect = EditorGUILayout.GetControlRect(GUILayout.Height(24));

            DrawOutputConnectionInfo(_rect, _output);
        }

        private void DrawOutputConnectionInfo(Rect _rect, EDS_DialogueHolder.DialogueOutput _output)
        {
            int _buttonWidth = 60;
            int _textFieldWidth = ((int)(_rect.width - _buttonWidth));

            // Connection dialogue title

            Rect _rectA = new Rect(_rect.x, _rect.y, _rect.width - _buttonWidth, _rect.height);

            GUI.enabled = false;

            EDS_DialogueHolder.Dialogue _connectionDialogue = EDS_GraphWindow.Current.CurrentDialogueHolder.GetDialogue(_output.nextDialogueID);

            if (_connectionDialogue != null)
            {
                EditorGUI.TextField(_rectA, EDS_GraphWindow.Current.CurrentDialogueHolder.GetDialogue(_output.nextDialogueID).title);
            }
            else
            {
                EditorGUI.TextField(_rectA, "***No Connection***");
            }

            GUI.enabled = true;

            // Search button

            Rect _rectB = new Rect(_rect.x + _textFieldWidth, _rect.y, _buttonWidth, _rect.height);

            if (GUI.Button(_rectB, "Search"))
            {
                EDS_DropDown _dropDown = new EDS_DropDown(
                    new AdvancedDropdownState(), EDS_DropDown.DropDownType.Dialogue,
                    (_value) =>
                    {
                        string _id;

                        if (_value.StartsWith("*Disconnect*"))
                        {
                            _id = "";
                        }
                        else
                        {
                            _id = _value.Substring(_value.IndexOf("(") + 1, 20);
                        }

                        _output.nextDialogueID = _id;
                    },
                    EDS_GraphWindow.Current.CurrentDialogueHolder
                );

                _dropDown.Show(_rect);
            }
        }
        #endregion
    }
    public class EDS_ExtraContentEditWindow : EditorWindow
    {
        public IList ExtraContents { get; protected set; }

        public EDS_ExtraContentList ExtraContentROList { get; protected set; }
        public Enum_ExtraContentType ContentType { get; protected set; }

        private Vector2 scrollPos;

        public static EDS_ExtraContentEditWindow OpenWindow(IList _list, Type _listItemType, Enum_ExtraContentType _contentType)
        {
            EDS_ExtraContentEditWindow _window = GetWindow<EDS_ExtraContentEditWindow>("Extra");
            _window.minSize = new Vector2(100, 100);
            _window.ExtraContents = _list;
            _window.ContentType = _contentType;
            _window.SetupList(_listItemType);
            return _window;
        }

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(true));

            EDS_EditorUtility.DrawBigLabel("Extras");

            // Draw main list
            ExtraContentROList.DoLayoutList();

            // Draw settings
            if (ExtraContentROList.selectedIndices.Count > 0)
            {
                if (ExtraContentROList.selectedIndices[0] >= 0 && ExtraContentROList.selectedIndices[0] < ExtraContents.Count)
                {
                    DrawContentSettings((EDS_DialogueHolder.DialogueExtraContent)ExtraContents[ExtraContentROList.selectedIndices[0]]);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void SetupList(Type _listItemType)
        {
            ExtraContentROList = new EDS_ExtraContentList(ExtraContents, _listItemType, ContentType);
        }

        private void DrawContentSettings(EDS_DialogueHolder.DialogueExtraContent _extraContent)
        {
            EDS_EditorUtility.DrawBigLabel("Parameters");

            var _parameters = _extraContent.parameters;

            if (_parameters.Count <= 0)
            {
                EditorGUILayout.LabelField("No Parameter.");;
            }
            else
            for (int i = 0; i < _parameters.Count; i++)
            {
                if (_parameters[i].parameterType == EDS_DialogueHolder.ExtraContentParameters.ParameterType.Int)
                {
                    _parameters[i] = new (_parameters[i].nameString, EditorGUILayout.IntField(_parameters[i].nameString, _parameters[i].valueInt));
                }
                else if (_parameters[i].parameterType == EDS_DialogueHolder.ExtraContentParameters.ParameterType.Float)
                {
                    _parameters[i] = new (_parameters[i].nameString, EditorGUILayout.FloatField(_parameters[i].nameString, _parameters[i].valueFloat));
                }
                else if (_parameters[i].parameterType == EDS_DialogueHolder.ExtraContentParameters.ParameterType.String)
                {
                    _parameters[i] = new (_parameters[i].nameString, EditorGUILayout.TextField(_parameters[i].nameString, _parameters[i].valueString));
                }
                else if (_parameters[i].parameterType == EDS_DialogueHolder.ExtraContentParameters.ParameterType.Bool)
                {
                    _parameters[i] = new (_parameters[i].nameString, EditorGUILayout.Toggle(_parameters[i].nameString, _parameters[i].valueBool));
                }
                else if (_parameters[i].parameterType == EDS_DialogueHolder.ExtraContentParameters.ParameterType.Object)
                {
                    _parameters[i] = new (_parameters[i].nameString, EditorGUILayout.ObjectField(_parameters[i].nameString,
                        _parameters[i].valueObject, typeof(UnityEngine.Object), false));
                }
                else
                {
                    Debug.LogError($"Index: {i}. Invalid variable type. Only support int, float, string, bool, UnityEngine.Object");
                    return;
                }
            }
        }
        private void DrawEventSettings(EDS_DialogueHolder.DialogueEvent _event)
        {

        }
    }
    public class EDS_TextAreaWindow : EditorWindow
    {
        public Action<string> onSave;
        private Vector2 scrollPos;

        public string Text { get; protected set; }
        
        private EDS_TextAreaWindow(){}

        public static void OpenWindow(string _text, Action<string> _onSaveCallback)
        {
            EDS_TextAreaWindow _window = GetWindow<EDS_TextAreaWindow>("Text Area");
            _window.Text = _text;
            _window.onSave = _onSaveCallback;
        }

        private void OnGUI()
        {
            // Text area

            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(true));

            Text = EditorGUILayout.TextArea(Text, EDS_GUIStyleProvider.WordWrapMultilineTextArea(), GUILayout.ExpandHeight(true));

            GUILayout.EndScrollView();

            // Buttons

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Save"))
            {
                onSave?.Invoke(Text);
                this.Close();
            }

            if (GUILayout.Button("Cancel"))
            {
                this.Close();
            }

            GUILayout.EndHorizontal();
        }
    }
    
    public class EDS_OutputList : ReorderableList
    {
        public List<EDS_DialogueHolder.DialogueOutput> Outputs { get; }

        public EDS_OutputList(IList elements, Type elementType, Enum_DialogueType _dialogueType) : base(elements, elementType)
        {
            Outputs = (List<EDS_DialogueHolder.DialogueOutput>)elements;
            Reset(_dialogueType);
        }

        public void Reset(Enum_DialogueType _dialogueType)
        {
            switch(_dialogueType)
            {
                case Enum_DialogueType.Normal:
                    Init_Normal();
                    break;
                case Enum_DialogueType.Straight:
                    Init_Staight();
                    break;
                case Enum_DialogueType.Branch:
                    Init_Branch();
                    break;
                case Enum_DialogueType.Events:
                    Init_Events();
                    break;
            }
        }

        private void Init_Normal()
        {
            headerHeight = 0;
            multiSelect = false;

            drawHeaderCallback = (Rect rect) => {};
            drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                Rect _elementRect = new Rect(rect.x, rect.y + 2, rect.width, rect.height - 4);

                int _buttonWidth = 24;
                int _buttonHeight = ((int)(_elementRect.height / 2));
                int _textFieldWidth = ((int)(_elementRect.width - _buttonWidth));
                int _textFieldHeight = ((int)_elementRect.height);

                Rect _rectA = new Rect(_elementRect.x, _elementRect.y, _buttonWidth, _buttonHeight);
                if (GUI.Button(_rectA, "C"))
                {
                    EDS_ExtraContentEditWindow.OpenWindow(Outputs[index].conditions, typeof(EDS_DialogueHolder.DialogueCondition), Enum_ExtraContentType.Condition);
                }

                Rect _rectB = new Rect(_elementRect.x, _elementRect.y + _buttonHeight, _buttonWidth, _buttonHeight);
                if (GUI.Button(_rectB, "T"))
                {
                    EDS_TextAreaWindow.OpenWindow(Outputs[index].content, (_text) => { Outputs[index].content = _text; });
                }

                Rect _rectC = new Rect(_elementRect.x + _buttonWidth, _elementRect.y, _textFieldWidth, _textFieldHeight);
                Outputs[index].content = EditorGUI.TextField(_rectC, Outputs[index].content);
            };
            onAddDropdownCallback = (Rect buttonRect, ReorderableList list) =>
            {
                GenericMenu _menu = new GenericMenu();

                _menu.AddItem(new GUIContent("Output"), false, () => 
                {
                    Outputs.Add(new EDS_DialogueHolder.DialogueOutput());
                });

                _menu.DropDown(buttonRect);
            };
            onAddCallback = (ReorderableList list) =>
            {
                Outputs.Add(new EDS_DialogueHolder.DialogueOutput());
            };
            onRemoveCallback = (ReorderableList list) =>
            {
                if (selectedIndices.Count > 0)
                {
                    for (int i = selectedIndices.Count - 1; i >= 0; i--)
                    {
                        Outputs.RemoveAt(selectedIndices[i]);
                    }
                }
                else
                {
                    Outputs.RemoveAt(Outputs.Count - 1);
                }
            };
            elementHeightCallback = (int index) =>
            {
                return 30;
            };
        }
        private void Init_Staight()
        {
            headerHeight = 0;
            footerHeight = 0;
            multiSelect = false;

            drawHeaderCallback = (Rect rect) => {};
            drawFooterCallback = (Rect rect) => {};
            drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                Rect _elementRect = new Rect(rect.x, rect.y + 2, rect.width, rect.height - 4);

                DrawOutputConnectionInfo(_elementRect, Outputs[index]);
            };
            elementHeightCallback = (int index) =>
            {
                return 30;
            };
        }
        private void Init_Branch()
        {
            headerHeight = 0;
            multiSelect = false;

            drawHeaderCallback = (Rect rect) => {};
            drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                Rect _elementRect = new Rect(rect.x, rect.y + 2, rect.width, rect.height - 4);

                int _buttonWidth = 40;
                int _infoContainerWidth = ((int)(_elementRect.width - _buttonWidth));
                int _singleLineHeight = 20;

                Rect _rectA = new Rect(_elementRect.x, _elementRect.y, _buttonWidth, _elementRect.height);

                if (GUI.Button(_rectA, "Edit"))
                {
                    EDS_ExtraContentEditWindow.OpenWindow(Outputs[index].conditions, typeof(EDS_DialogueHolder.DialogueCondition), Enum_ExtraContentType.Condition);
                }

                Rect _rectB = new Rect(_elementRect.x + _buttonWidth + 4, _elementRect.y, _infoContainerWidth, _singleLineHeight);

                if (Outputs[index].conditions.Count > 0)
                for (int i = 0; i < Outputs[index].conditions.Count; i++)
                {
                    EditorGUI.LabelField(_rectB, Outputs[index].conditions[i].itemName);

                    _rectB.y += 20;
                }
            };
            onAddCallback = (ReorderableList list) =>
            {
                Outputs.Add(new EDS_DialogueHolder.DialogueOutput());
            };
            onRemoveCallback = (ReorderableList list) =>
            {
                if (selectedIndices.Count > 0)
                {
                    for (int i = selectedIndices.Count - 1; i >= 0; i--)
                    {
                        Outputs.RemoveAt(selectedIndices[i]);
                    }
                }
                else
                {
                    Outputs.RemoveAt(Outputs.Count - 1);
                }
            };
            elementHeightCallback = (int index) =>
            {
                if (Outputs[index].conditions.Count > 0)
                {
                    return 20 * Outputs[index].conditions.Count;
                }
                else
                {
                    return 20;
                }
            };
        }
        private void Init_Events()
        {
            headerHeight = 0;
            footerHeight = 0;
            multiSelect = false;

            drawHeaderCallback = (Rect rect) => {};
            drawFooterCallback = (Rect rect) => {};
            drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                Rect _elementRect = new Rect(rect.x, rect.y + 2, rect.width, rect.height - 4);

                int _buttonWidth = 40;
                int _infoContainerWidth = ((int)(_elementRect.width - _buttonWidth));
                int _singleLineHeight = 20;

                Rect _rectA = new Rect(_elementRect.x, _elementRect.y, _buttonWidth, _elementRect.height);

                if (GUI.Button(_rectA, "Edit"))
                {
                    EDS_ExtraContentEditWindow.OpenWindow(Outputs[index].events, typeof(EDS_DialogueHolder.DialogueEvent), Enum_ExtraContentType.Event);
                }

                Rect _rectB = new Rect(_elementRect.x + _buttonWidth + 4, _elementRect.y, _infoContainerWidth, _singleLineHeight);

                if (Outputs[index].events.Count > 0)
                for (int i = 0; i < Outputs[index].events.Count; i++)
                {
                    EditorGUI.LabelField(_rectB, Outputs[index].events[i].itemName);

                    _rectB.y += 20;
                }
            };
            elementHeightCallback = (int index) =>
            {
                if (Outputs[index].events.Count > 0)
                {
                    return 20 * Outputs[index].events.Count;
                }
                else
                {
                    return 20;
                }
            };
        }

        private void DrawOutputConnectionInfo(Rect _rect, EDS_DialogueHolder.DialogueOutput _output)
        {
            int _buttonWidth = 60;
            int _textFieldWidth = ((int)(_rect.width - _buttonWidth));

            // Connection dialogue title

            Rect _rectA = new Rect(_rect.x, _rect.y, _rect.width - _buttonWidth, _rect.height);

            GUI.enabled = false;

            EDS_DialogueHolder.Dialogue _connectionDialogue = EDS_GraphWindow.Current.CurrentDialogueHolder.GetDialogue(_output.nextDialogueID);

            if (_connectionDialogue != null)
            {
                EditorGUI.TextField(_rectA, EDS_GraphWindow.Current.CurrentDialogueHolder.GetDialogue(_output.nextDialogueID).title);
            }
            else
            {
                EditorGUI.TextField(_rectA, "***No Connection***");
            }

            GUI.enabled = true;

            // Search button

            Rect _rectB = new Rect(_rect.x + _textFieldWidth, _rect.y, _buttonWidth, _rect.height);

            if (GUI.Button(_rectB, "Search"))
            {
                EDS_DropDown _dropDown = new EDS_DropDown(
                    new AdvancedDropdownState(), EDS_DropDown.DropDownType.Dialogue,
                    (_value) =>
                    {
                        string _id;

                        if (_value.StartsWith("*Disconnect*"))
                        {
                            _id = "";
                        }
                        else
                        {
                            _id = _value.Substring(_value.IndexOf("(") + 1, 20);
                        }

                        _output.nextDialogueID = _id;
                    },
                    EDS_GraphWindow.Current.CurrentDialogueHolder
                );

                _dropDown.Show(_rect);
            }
        }
    }
    public class EDS_ExtraContentList : ReorderableList
    {
        public IList ExtraContents { get; }
        public Enum_ExtraContentType ContentType { get; }

        public EDS_ExtraContentList(IList elements, Type _type, Enum_ExtraContentType _exType) : base(elements, _type)
        {
            ExtraContents = elements;
            ContentType = _exType;
            Setup();
        }

        private void Setup()
        {
            headerHeight = 0;
            multiSelect = false;

            drawHeaderCallback = (Rect rect) => {};
            drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                // Set item type

                EDS_DialogueHolder.DialogueExtraContent _item;

                if (ContentType == Enum_ExtraContentType.Condition) _item = (EDS_DialogueHolder.DialogueCondition)ExtraContents[index];
                else if (ContentType == Enum_ExtraContentType.Event) _item = (EDS_DialogueHolder.DialogueEvent)ExtraContents[index];
                else
                {
                    Debug.LogError("Invalid type");
                    return;
                }

                // : Layouts

                Rect _elementRect = new Rect(rect.x, rect.y + 2, rect.width, rect.height - 4);

                int _buttonWidth = 80;
                int _labelWidth = ((int)(_elementRect.width - _buttonWidth));
                
                // Text

                Rect _rectA = new Rect(_elementRect.x, _elementRect.y, _labelWidth, _elementRect.height);

                EditorGUI.LabelField(_rectA, _item.itemName);

                // Button

                Rect _rectB = new Rect(_elementRect.x + _labelWidth, _elementRect.y, _buttonWidth, _elementRect.height);

                if (GUI.Button(_rectB, "Search"))
                {
                    EDS_DropDown _dropDown = new EDS_DropDown(
                        new AdvancedDropdownState(), ContentType == Enum_ExtraContentType.Condition ? EDS_DropDown.DropDownType.Condition : EDS_DropDown.DropDownType.Evnet,
                        (selectName) => 
                        {
                            if (ContentType == Enum_ExtraContentType.Condition ?
                                EDS_DialogueCondition.conditionInfos.ContainsKey(selectName) : EDS_DialogueEvent.eventInfos.ContainsKey(selectName))
                            {
                                _item.itemName = selectName;
                                _item.itemName = selectName;

                                SetupParameterInfos(_item);
                            }
                            else
                            {
                                EditorUtility.DisplayDialog("Failed", "Extra content not found, maybe the lists need to update .", "OK");
                            }
                        });

                    _dropDown.Show(_elementRect);
                }
            };
            onAddCallback = (ReorderableList list) =>
            {
                if (ContentType == Enum_ExtraContentType.Condition)
                {
                    ExtraContents.Add(new EDS_DialogueHolder.DialogueCondition());
                }
                else if (ContentType == Enum_ExtraContentType.Event)
                {
                    ExtraContents.Add(new EDS_DialogueHolder.DialogueEvent());
                }
                else { Debug.LogError("Invalid type"); }
            };
            onRemoveCallback = (ReorderableList list) =>
            {
                if (selectedIndices.Count > 0)
                {
                    for (int i = selectedIndices.Count - 1; i >= 0; i--)
                    {
                        ExtraContents.RemoveAt(selectedIndices[i]);
                    }
                }
                else
                {
                    ExtraContents.RemoveAt(ExtraContents.Count - 1);
                }
            };
            elementHeightCallback = (int index) =>
            {
                return 30;
            };
        }

        private void SetupParameterInfos(EDS_DialogueHolder.DialogueExtraContent _exContent)
        {
            if (_exContent.itemName == null)
            {
                Debug.LogError("Invalid ex content type.");
                return;
            }

            _exContent.parameters.Clear();

            // Get field infos.

            FieldInfo[] _fieldInfos;

            if (EDS_DialogueCondition.conditionInfos.ContainsKey(_exContent.itemName))
            {
                _fieldInfos = EDS_DialogueCondition.conditionInfos[_exContent.itemName].GetFields();
            }
            else if (EDS_DialogueEvent.eventInfos.ContainsKey(_exContent.itemName))
            {
                _fieldInfos = EDS_DialogueEvent.eventInfos[_exContent.itemName].GetFields();
            }
            else
            {
                Debug.LogError("Invalid key");
                return;
            }

            // Init parameters.

            if (_fieldInfos.Length > 0)
            for (int i = 0; i < _fieldInfos.Length; i++)
            {
                if (_fieldInfos[i].FieldType == typeof(int))
                {
                    _exContent.parameters.Add(new EDS_DialogueHolder.ExtraContentParameters(_fieldInfos[i].Name, 0));
                }
                else if (_fieldInfos[i].FieldType == typeof(float))
                {
                    _exContent.parameters.Add(new EDS_DialogueHolder.ExtraContentParameters(_fieldInfos[i].Name, 0f));
                }
                else if (_fieldInfos[i].FieldType == typeof(string))
                {
                    _exContent.parameters.Add(new EDS_DialogueHolder.ExtraContentParameters(_fieldInfos[i].Name, ""));
                }
                else if (_fieldInfos[i].FieldType == typeof(bool))
                {
                    _exContent.parameters.Add(new EDS_DialogueHolder.ExtraContentParameters(_fieldInfos[i].Name, false));;
                }
                else if (_fieldInfos[i].FieldType == typeof(UnityEngine.Object))
                {
                    _exContent.parameters.Add(new EDS_DialogueHolder.ExtraContentParameters(_fieldInfos[i].Name, new UnityEngine.Object()));
                }
                else
                {
                    Debug.LogError($"Index: {i}. Invalid variable type. Only support int, float, string, bool, UnityEngine.Object");
                    return;
                }
            }
        }
    }

    public class EDS_DropDown : AdvancedDropdown
    {
        public enum DropDownType { Condition, Evnet, Dialogue}

        public DropDownType DDType { get; }
        public Action<string> OnSelect { get; }
        public EDS_DialogueHolder DialogueHolder { get; }

        public EDS_DropDown(AdvancedDropdownState _state, DropDownType _type, Action<string> _onSelectCallback, EDS_DialogueHolder _dialogueHolder = null) : base(_state)
        {
            minimumSize = new Vector2(160, 300);
            DDType = _type;
            OnSelect = _onSelectCallback;
            DialogueHolder = _dialogueHolder;
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            AdvancedDropdownItem _root = new AdvancedDropdownItem("Root");

            if (DDType == DropDownType.Condition)
            {
                AdvancedDropdownItem _conditionRoot = new AdvancedDropdownItem("Conditions");

                _root.AddChild(_conditionRoot);

                List<string> _keys = EDS_DialogueCondition.GetAllKeys();

                if (_keys.Count > 0)
                for (int i = 0; i < _keys.Count; i++)
                {
                    _conditionRoot.AddChild(new AdvancedDropdownItem(_keys[i]));
                }
            }
            if (DDType == DropDownType.Evnet)
            {
                AdvancedDropdownItem _eventRoot = new AdvancedDropdownItem("Events");

                _root.AddChild(_eventRoot);

                List<string> _keys = EDS_DialogueEvent.GetAllKeys();

                if (_keys.Count > 0)
                for (int i = 0; i < _keys.Count; i++)
                {
                    _eventRoot.AddChild(new AdvancedDropdownItem(_keys[i]));
                }
            }
            if (DDType == DropDownType.Dialogue)
            {
                AdvancedDropdownItem _dialogueRoot = new AdvancedDropdownItem("Dialogues");

                _root.AddChild(_dialogueRoot);

                var _dialogues = DialogueHolder.dialogues;

                _dialogueRoot.AddChild(new AdvancedDropdownItem("*Disconnect*"));

                if (_dialogues.Count > 0)
                for (int i = 0; i < _dialogues.Count; i++)
                {
                    StringBuilder _name = new StringBuilder(_dialogues[i].title);
                    _name.Append("(");
                    _name.Append(_dialogues[i].ID);
                    _name.Append(")");

                    _dialogueRoot.AddChild(new AdvancedDropdownItem(_name.ToString()));
                }
            }

            return _root;
        }

        protected override void ItemSelected(AdvancedDropdownItem _item)
        {
            base.ItemSelected(_item);
            OnSelect?.Invoke(_item.name);
        }
    }
}