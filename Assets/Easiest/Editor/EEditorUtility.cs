using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Easiest
{
    public class EEditorUtility
    {
        public static GUIStyle BigLabel
        {
            get
            {
                return new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                };
            }
        }

        public static void GUIHorizontalLine(Color _color, int _thickness = 1, int _padding = 8)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(_padding + _thickness));
            r.height = _thickness;
            r.y += _padding / 2;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, _color);
        }
        public static void LayoutHorizontalTitle(string _title)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(_title, BigLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        public static GUIStyle WordWrapTextArea()
        {
            var _style = new GUIStyle(GUI.skin.textArea)
            {
                wordWrap = true
            };
            return _style;
        }
        public static void ListAllClassOfUIElement(VisualElement _element)
        {
            var _list = _element.GetClasses();
            foreach( var _name in _list)
            {
                Debug.Log(_name);
            }
        }
    }
}
