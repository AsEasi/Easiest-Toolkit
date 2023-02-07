namespace Easiest
{
    using UnityEditor.UIElements;
    using UnityEngine.UIElements;
    using UnityEngine;
    using System;

    public class MouseMonitorManipulator : MouseManipulator
    {
        public Action<MouseDownEvent> mouseDown;
        public Action<MouseUpEvent> mouseUp;
        public Action<MouseMoveEvent> mouseMove;
        public Action<MouseMoveEvent> mouseDrag;

        public bool isMouseDown { get; protected set; }
         
        public MouseMonitorManipulator()
        {
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDownHandler);
            target.RegisterCallback<MouseUpEvent>(OnMouseUpHandler);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDownHandler);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUpHandler);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
        }

        protected void OnMouseDownHandler(MouseDownEvent evt)
        {
            isMouseDown = true;

            mouseDown?.Invoke(evt);
        }
        protected void OnMouseUpHandler(MouseUpEvent evt)
        {
            isMouseDown = false;

            mouseUp?.Invoke(evt);
        }
        protected void OnMouseMove(MouseMoveEvent evt)
        {
            mouseMove?.Invoke(evt);

            if (isMouseDown)
                mouseDrag?.Invoke(evt);
        }
    }
    public class Draggable : MouseManipulator
    {
        Action<Vector2> m_Handler;

        bool m_Active;

        bool m_OutputDeltaMovement;

        public Draggable(Action<Vector2> handler, bool outputDeltaMovement = false)
        {
            m_Handler = handler;
            m_Active = false;
            m_OutputDeltaMovement = outputDeltaMovement;
            activators.Add(new ManipulatorActivationFilter()
            {
                button = MouseButton.LeftMouse
            });
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback(new EventCallback<MouseDownEvent>(OnMouseDown));
            target.RegisterCallback(new EventCallback<MouseMoveEvent>(OnMouseMove));
            target.RegisterCallback(new EventCallback<MouseUpEvent>(OnMouseUp));
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback(new EventCallback<MouseDownEvent>(OnMouseDown));
            target.UnregisterCallback(new EventCallback<MouseMoveEvent>(OnMouseMove));
            target.UnregisterCallback(new EventCallback<MouseUpEvent>(OnMouseUp));
        }

        void OnMouseDown(MouseDownEvent evt)
        {
            target.CaptureMouse();
            m_Active = true;
            evt.StopPropagation();
        }

        void OnMouseMove(MouseMoveEvent evt)
        {
            if (m_Active)
            {
                if (m_OutputDeltaMovement)
                {
                    m_Handler?.Invoke(evt.mouseDelta);
                }
                else
                {
                    m_Handler?.Invoke(evt.localMousePosition);
                }
            }
        }

        void OnMouseUp(MouseUpEvent evt)
        {
            m_Active = false;

            if (target.HasMouseCapture())
            {
                target.ReleaseMouse();
            }

            evt.StopPropagation();
        }
    }
}