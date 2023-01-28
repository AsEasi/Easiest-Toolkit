namespace Easiest
{
    using UnityEditor.UIElements;
    using UnityEngine.UIElements;
    using System;

    public class MouseMonitorManipulator : MouseManipulator
    {

        public Action OnMouseUp;
        public Action OnMouseMove;
        public Action OnMouseDown;

        public MouseMonitorManipulator(Action _handler)
        {
            var _filter = new ManipulatorActivationFilter();
            _filter.button = MouseButton.RightMouse;

            activators.Add(_filter);
        }

        protected override void RegisterCallbacksOnTarget()
        {
            // target.RegisterCallback(ChangeEvent<>));
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            throw new System.NotImplementedException();
        }
    }
}