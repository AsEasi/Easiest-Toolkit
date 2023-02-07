namespace Easiest.DialogueSystem
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEditorInternal;
    using System.Collections;

    public abstract class EDS_GraphElement : IEDSGraphElement
    {
        [Obsolete("Might be remove in the future, use ParentWindow.AddElement and ParentWindow.RemoveElement instead.")]
        public List<EDS_GraphElement> DirectChildren { get; protected set; }
        public EDS_GraphWindow ParentWindow { get; protected set; }
        public Rect ElementRect { get; set; }
        public bool CanHandle { get; set; }
        public bool AutoRemove { get; set; }
        public Dictionary<int, List<EDS_GraphElement>> LayerElements;
        public abstract string ElementID {get;}

        public virtual void OnGUI()
        {
            CatchEvent();
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

        protected void CatchEvent()
        {
            switch (Event.current.type)
            {
                case EventType.MouseDown:
                    OnMouseDown(Event.current);
                    break;
                case EventType.MouseUp:
                    OnMouseUp(Event.current);
                    break;
                case EventType.MouseMove:
                    OnMouseMove(Event.current);
                    break;
                case EventType.MouseDrag:
                    OnMouseDrag(Event.current);
                    break;
            }
        }

        protected virtual void OnMouseDown(Event _event) {}
        protected virtual void OnMouseUp(Event _event) {}
        protected virtual void OnMouseMove(Event _event) {}
        protected virtual void OnMouseDrag(Event _event) {}
    }
    public abstract class EDS_DialogueSimulator
    {

    }
}