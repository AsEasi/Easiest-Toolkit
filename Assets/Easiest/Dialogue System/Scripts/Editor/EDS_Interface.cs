namespace Easiest.DialogueSystem
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public interface IEDSLayerDrawer
    {
        public Dictionary<int, List<EDS_GraphElement>> LayerElements { get; set; }
        
        public void DrawLayerElements()
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
        }

        public void AddElement(EDS_GraphElement _element, int _layer = 0, bool _autoRemove = true)
        {
            if (!LayerElements.ContainsKey(_layer))
                LayerElements.Add(_layer, new List<EDS_GraphElement>());

            // Repeat check, don't know if its nessessary. ?? Maybe think this in future.
            if (LayerElements[_layer].Contains(_element) || _element == null) return;

            _element.AutoRemove = _autoRemove;

            LayerElements[key: _layer].Add(_element);
        }

        public void RemoveElement(EDS_GraphElement _element, int _layer = 0)
        {
            List<int> _keyList = new List<int>(LayerElements.Keys);

            for (int i = 0; i < _keyList.Count; i++)
            {
                var _elementList = LayerElements[_keyList[i]];

                if (_elementList.Contains(_element))
                {
                    _elementList.Remove(_element);

                    return;
                }
            }
        }

    }
    public interface IEDSGraphElement
    {
        public void OnGUI();
    }
}