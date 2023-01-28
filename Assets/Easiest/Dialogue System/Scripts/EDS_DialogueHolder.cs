using System;
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
            public Color nodeColor;
        }
#endif

        [System.Serializable]
        public class Dialogue
        {
#if UNITY_EDITOR
            public NodeInfo nodeInfo;
#endif
            public string ID 
            { 
                get 
                {
                    if (id == "" || id == null)
                       id = EUtility.GenerateRandomeAlphabet(20);

                    return id;
                } 
            }
            [SerializeField] public string id = "";
            public string title = "Title";
            public string actor = "Actor";
            public DialogueContent content = new();
            public List<DialogueChoice> choices = new();

            public string ResetID()
            {
                id = "";
                return ID;
            }

            public Dialogue()
            {
                choices.Add(new DialogueChoice());

                nodeInfo.nodePos = Rect.zero;
                nodeInfo.nodeColor = new Color32(80, 80, 80, 255);
            }

#if UNITY_EDITOR
            public Dialogue(Rect _nodePos)
            {
                choices.Add(new DialogueChoice());
                
                nodeInfo.nodePos = _nodePos;
                nodeInfo.nodeColor = new Color32(80, 80, 80, 255);
            }
#endif
        }
        #region first classes
        [System.Serializable]
        public class DialogueContent
        {
#if UNITY_EDITOR
            public bool showExContent = false;
#endif
            public bool enable = false;
            public List<DialogueCondition> conditions = new();
            public string content = "Content";
            public List<DialogueEvent> events = new();
        }
        [System.Serializable]
        public class DialogueChoice
        {
            public enum ChoiceStatus
            {
                Normal = 0, Unenable, Used
            }

#if UNITY_EDITOR
            public bool showExContent;
#endif
            public bool enable = false;
            public ChoiceStatus status = new();
            public List<DialogueCondition> conditions = new();
            public string content = "Choice";
            public List<DialogueEvent> events = new();
            public string nextDialogueID = "";
        }
        #endregion
        #region second classes
        [System.Serializable]
        public class DialogueCondition
        {
#if UNITY_EDITOR
            public bool open = false;
#endif
            public string itemName = "*Condition*";
            public Type itemType = typeof(object);
            public Dictionary<FieldInfo, object> parameters = new();

        }
        [System.Serializable]
        public class DialogueEvent
        {
            public enum EventExecuteTime { Befor = 0, After }

#if UNITY_EDITOR
            public bool open = false;
#endif
            public EventExecuteTime executeTime;
            public string itemName = "*Event*";
            public Type itemType = typeof(object);
            public Dictionary<FieldInfo, object> parameters = new();
        }
        #endregion

        public List<Dialogue> dialogues;
    }
}

