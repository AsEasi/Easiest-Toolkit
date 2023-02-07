using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Easiest.DialogueSystem
{
    public class EDS_DialogueHolder : ScriptableObject
    {
#if UNITY_EDITOR
        [System.Serializable]
        public struct NodeInfo
        {
            public Rect nodePos;
        }
#endif

        [System.Serializable]
        public class Dialogue
        {
#if UNITY_EDITOR
            public NodeInfo nodeInfo;
#endif
            public Enum_DialogueType dialogueType = Enum_DialogueType.Normal; 
            public string ID 
            { 
                get 
                {
                    if (id == "" || id == null)
                       id = EUtility.GenerateRandomeAlphabet(20);

                    return id;
                } 
            }
            [SerializeField] private string id = "";
            public string title = "Title";
            public bool hasContent = true;
            public bool usePreviousActor = false;
            public string actor = "Actor";
            public DialogueContent content = new();
            public List<DialogueOutput> outputs = new();

            public string ResetID()
            {
                id = "";
                return ID;
            }

            public Dialogue(Enum_DialogueType _type = Enum_DialogueType.Normal)
            {
                dialogueType = _type;

                title = dialogueType.ToString();

                outputs.Add(new DialogueOutput());

                nodeInfo.nodePos = Rect.zero;

                ResetID();
            }

#if UNITY_EDITOR
            public Dialogue(Rect _nodePos, Enum_DialogueType _type = Enum_DialogueType.Normal)
            {
                dialogueType = _type;

                title = dialogueType.ToString();

                outputs.Add(new DialogueOutput());
                
                nodeInfo.nodePos = _nodePos;

                ResetID();
            }
#endif
        }
        #region first classes
        [System.Serializable]
        public class DialogueContent
        {
            public bool enable = false;
            [TextArea]
            public string content = "Content";
        }
        [System.Serializable]
        public class DialogueOutput
        {
            public Enum_OutputStatus status = 0;
            public List<DialogueCondition> conditions = new();
            public string content = "Output";
            public List<DialogueEvent> events = new();
            public string nextDialogueID = "";

            public DialogueOutput(Enum_OutputStatus _status = 0)
            {
                status = _status;
            }
        }
        #endregion
        #region second classes
        [System.Serializable]
        public class DialogueExtraContent
        {
            public string itemName = "*Extra*";
            public List<ExtraContentParameters> parameters = new();
        }
        [System.Serializable]
        public class DialogueCondition : DialogueExtraContent
        {
            public DialogueCondition()
            {
                itemName = "*Condition*";
            }
        }
        [System.Serializable]
        public class DialogueEvent : DialogueExtraContent
        {
            public DialogueEvent()
            {
                itemName = "*Event*";
            }
        }
        #endregion
        #region third classes
        [System.Serializable]
        public struct ExtraContentParameters
        {
            public enum ParameterType { Int = 0, Float = 1, String = 2, Bool = 3, Object = 4 }
            // Field info
            public ParameterType parameterType;
            public string nameString;
            // Values
            public int valueInt;
            public float valueFloat;
            public string valueString;
            public bool valueBool;
            public UnityEngine.Object valueObject;

            public ExtraContentParameters(string _name, int _value)
            {
                parameterType = ParameterType.Int;
                nameString = _name;

                valueInt = _value;
                valueFloat = 0;
                valueString = "";
                valueBool = false;
                valueObject = new UnityEngine.Object();
            }
            public ExtraContentParameters(string _name, float _value)
            {
                parameterType = ParameterType.Float;
                nameString = _name;

                valueInt = 0;
                valueFloat = _value;
                valueString = "";
                valueBool = false;
                valueObject = new UnityEngine.Object();
            }
            public ExtraContentParameters(string _name, bool _value)
            {
                parameterType = ParameterType.Bool;
                nameString = _name;

                valueInt = 0;
                valueFloat = 0;
                valueString = "";
                valueBool = _value;
                valueObject = new UnityEngine.Object();
            }
            public ExtraContentParameters(string _name, string _value)
            {
                parameterType = ParameterType.String;
                nameString = _name;

                valueInt = 0;
                valueFloat = 0;
                valueString = _value;
                valueBool = false;
                valueObject = new UnityEngine.Object();
            }
            public ExtraContentParameters(string _name, UnityEngine.Object _value)
            {
                parameterType = ParameterType.Object;
                nameString = _name;

                valueInt = 0;
                valueFloat = 0;
                valueString = "";
                valueBool = false;
                valueObject = _value;
            }
        
        }
        #endregion

        public List<Dialogue> dialogues = new();

        public Dialogue GetDialogue(string _id)
        {
            if (dialogues.Count > 0)
            for (int i = 0; i < dialogues.Count; i++)
            {
                if (dialogues[i].ID == _id)
                {
                    return dialogues[i];
                }
            }
            return null;
        }
    }
}

