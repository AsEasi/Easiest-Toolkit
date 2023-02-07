namespace Easiest.DialogueSystem
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public class EDS_EditorUtility
    {
        public static void DrawBigLabel(string _text)
        {
            EEditorUtility.GUIHorizontalLine(Color.black);
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.Label(_text, EDS_GUIStyleProvider.BigLabel());

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
    public class EDS_GridDrawer
    {
        public EDS_GridDrawer() { }

        public static void DrawGrid(Rect _area, Vector2 _offset, int _spacing, Color _lineColor)
        {
            Handles.color = _lineColor;

            Vector2 _lineCount = new Vector2(
                Mathf.Abs(Mathf.CeilToInt(_area.width / _spacing)),
                Mathf.Abs(Mathf.CeilToInt(_area.height / _spacing))
            );

            // : Horizontal

            float _posX = _offset.x % _spacing;

            for (int i = 0; i < _lineCount.x; i++)
            {
                Handles.DrawLine(
                    new Vector2(_posX, 0),
                    new Vector2(_posX, _area.height)
                );

                _posX += _spacing;
            }

            // : Vertical

            float _posY = _offset.y % _spacing;

            for (int i = 0; i < _lineCount.y; i++)
            {
                Handles.DrawLine(
                    new Vector2(0, _posY),
                    new Vector2(_area.width, _posY)
                );

                _posY += _spacing;
            }
        }
    }
    public class EDS_EventMonitor
    {
        // :: Creat a instance and call EventCheck() in OnGUI().

        public Action<Event> onMouseDown;
        public Action<Event> onMouseUp;
        public Action<Event> onMouseMove;
        public Action<Event> onMouseDrag;

        public void EventMonitor()
        {
            Event _event = Event.current;

            switch (_event.type)
            {
                case EventType.MouseDown:
                    onMouseDown?.Invoke(_event);
                    break;

                case EventType.MouseUp:
                    onMouseUp?.Invoke(_event);
                    break;

                case EventType.MouseMove:
                    onMouseMove?.Invoke(_event);
                    break;

                case EventType.MouseDrag:
                    onMouseDrag?.Invoke(_event);
                    break;
            }
        }
    }
    public class EDS_GUILineDrawer
    {
        public static void GUIHorizontalLine(int _holderHeight, int _lineThickness, Color _lineColor)
        {
            Rect _rect = EditorGUILayout.GetControlRect(GUILayout.Height(_holderHeight + _lineThickness));

            _rect.x -= 4;
            _rect.y += _holderHeight * .5f;
            _rect.width += 8;
            _rect.height = _lineThickness;

            EditorGUI.DrawRect(_rect, _lineColor);
        }
    }
    public class EDS_GUIStyleProvider
    {
        public static readonly Color Grid_Background_Color = new Color(.1f, .1f, .1f, 1);
        public static readonly Color General_Background_Color = new Color(.1f, .1f, .1f, .7f);
        public static readonly Color Node_Normal_Title_Default_Background_Color = new Color(.2f, .2f, .2f, .8f);
        public static readonly Color Node_Element_Background_Color = new Color(.2f, .2f, .2f, .4f);
        public static readonly Color Node_Output_Background_Color = new Color(.1f, .1f, .1f, 1f);

        public static GUIStyle NodeTitleLabel()
        {
            GUIStyle _style = new GUIStyle(GUI.skin.label);

            _style.fontSize = 14;
            _style.alignment = TextAnchor.MiddleLeft;
            // _style.fontStyle = FontStyle.Bold;

            return _style;
        }
        public static GUIStyle MiddleLeftAlignLabel()
        {
            GUIStyle _style = new GUIStyle(GUI.skin.label);

            _style.alignment = TextAnchor.MiddleLeft;

            return _style;
        }
        public static GUIStyle BigLabel()
        {
            GUIStyle _style = new GUIStyle(GUI.skin.label);

            _style.alignment = TextAnchor.MiddleCenter;
            _style.fontSize = 14;

            return _style;
        }
    }
    public class EDS_GUIAreaDrawer
    {
        public static void BeginCenterAlignArea(Rect _rect, bool _horizontal, bool _vertical)
        {
            GUILayout.BeginArea(_rect);

            if (_horizontal)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
            }

            if (_vertical)
            {
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();
            }
        }
        public static void EndCenterAlignArea(bool _horizontal, bool _vertical)
        {
            if (_vertical)
            {
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
            }
            if (_horizontal)
            {
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndArea();
        }
    }
    public class EDS_EdgeDrawer
    {
        public static void DrawEdge()
        {
            throw new NotImplementedException();
        }
    }
    public class EDS_Caculator
    {
        public static int[] SortIntList(List<int> _list)
        {
            List<int> _sortedList = new List<int>(_list.Count);

            int _current;

            for(int i = 0; i < _list.Count; i++)
            {
                _current = _list[i];

                for (int a = i + 1; a < _list.Count; a++)
                {
                    if(_current <= _list[a])
                    {
                        _current = _list[a];
                    }

                    // Last one
                    if (a == _list.Count - 1)
                    {
                        _sortedList.Insert(0, _current);
                    }
                }
            }

            return _sortedList.ToArray();
        }

        public static void QuickSort(ref List<int> nums, int left, int right)
        {
            if (left < right)
            {
                int i = left;
                int j = right;
                int middle = nums[(left + right) / 2];
                while (true)
                {
                    while (i < right && nums[i] < middle) { i++; };
                    while (j > 0 && nums[j] > middle) { j--; };
                    if (i == j) break;
                    int temp = nums[i];
                    nums[i] = nums[j];
                    nums[j] = temp;
                    if (nums[i] == nums[j]) j--;
                }
                QuickSort(ref nums, left, i);
                QuickSort(ref nums, i + 1, right);
            }
        }
    }
}