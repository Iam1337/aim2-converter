/* Copyright (c) 2023 dr. ext (Vladimir Sigalkin) */

using UnityEngine;
using UnityEditor;

namespace AimConverter.UI
{
	public class AimPanelSplit : AimPanel
	{
		#region Extensions

		public enum SplitOrientation
		{
			Horizontal,
            Vertical
		}
        
		#endregion

        #region Private Vars
        
        private SplitOrientation _orientation;
        private AimPanel _firstPanel;
        private AimPanel _secondPanel;
        private float _splitterPosition;
        private Rect _splitterRect;
        private bool _splitterDrag;
        private readonly float _splitterSize = 1f;
        private readonly float _splitterMargin = 2f;

        #endregion

		#region Public Methods

        public AimPanelSplit(EditorWindow window) : base(window)
        { }

        public void Setup(SplitOrientation orientation, AimPanel firstPanel, AimPanel secondPanel, float splitterPosition)
        {
            _orientation = orientation;
            _firstPanel = firstPanel;
            _secondPanel = secondPanel;

            _splitterRect = new Rect();
            _splitterPosition = splitterPosition;
            
            Repaint();
        }

        public override void Dispose()
        {
            base.Dispose();
            
            _firstPanel?.Dispose();
            _secondPanel?.Dispose();
        }

        #endregion

		#region Protected Methods

        protected override void DrawContent(Rect contentRect)
        {
            var offset = 0f;

            // first panel
            var firstPanelRect = new Rect();
            if (_orientation == SplitOrientation.Horizontal)
            {
                firstPanelRect.x = 0;
                firstPanelRect.y = 0;
                firstPanelRect.width = contentRect.width * _splitterPosition - _splitterMargin - _splitterSize * 0.5f;
                firstPanelRect.height = contentRect.height;
            }
            else
            {
                firstPanelRect.x = 0;
                firstPanelRect.y = 0;
                firstPanelRect.width = contentRect.width;
                firstPanelRect.height = contentRect.height * _splitterPosition - _splitterMargin - _splitterSize * 0.5f;
            }
            
            _firstPanel?.Draw(firstPanelRect);

            // second panel
            var secondPanelRect = new Rect();
            if (_orientation == SplitOrientation.Horizontal)
            {
                secondPanelRect.x = firstPanelRect.x + firstPanelRect.width + _splitterMargin * 2 + _splitterSize;
                secondPanelRect.y = 0;
                secondPanelRect.width = contentRect.width - secondPanelRect.x;
                secondPanelRect.height = contentRect.height;
            }
            else
            {
                secondPanelRect.x = 0;
                secondPanelRect.y = firstPanelRect.y + firstPanelRect.height + _splitterMargin * 2 + _splitterSize;
                secondPanelRect.width = contentRect.width;
                secondPanelRect.height = contentRect.height - secondPanelRect.y;
            }
            
            _secondPanel?.Draw(secondPanelRect);
            
            // splitter
            if (_orientation == SplitOrientation.Horizontal)
            {
                _splitterRect.x = firstPanelRect.width + _splitterMargin;
                _splitterRect.width = _splitterSize;
                _splitterRect.height = contentRect.height;
            }
            else if (_orientation == SplitOrientation.Vertical)
            {
                _splitterRect.y = firstPanelRect.height + _splitterMargin;
                _splitterRect.height = _splitterSize;
                _splitterRect.width = contentRect.width;
            }
            
            GUI.DrawTexture(_splitterRect, AimTextures.Splitter);

            if (_orientation == SplitOrientation.Horizontal)
            {
                _splitterRect.x -= _splitterMargin;
                _splitterRect.width += _splitterMargin * 2;
                _splitterRect.height = contentRect.height;
            }
            else
            {
                _splitterRect.y -= _splitterMargin;
                _splitterRect.height += _splitterMargin * 2;
                _splitterRect.width = contentRect.width;
            }
            
            EditorGUIUtility.AddCursorRect(_splitterRect, _orientation == SplitOrientation.Horizontal ? MouseCursor.ResizeHorizontal : MouseCursor.ResizeVertical);
        }

        protected override void PostDrawContent(Rect contentRect)
        {
            if (Event.current == null) 
                return;

            switch (Event.current.type)
            {
                case EventType.MouseDown:
                {
                    if (_splitterRect.Contains(Event.current.mousePosition))
                        _splitterDrag = true;
                }
                    break;
                case EventType.MouseDrag:
                {
                    if (_splitterDrag)
                    {
                        _splitterPosition = _orientation == SplitOrientation.Horizontal ? 
                            Mathf.Clamp01(Event.current.mousePosition.x / contentRect.width) : 
                            Mathf.Clamp01(Event.current.mousePosition.y / contentRect.height);

                        Repaint();
                    }
                }
                    break;
                case EventType.MouseUp:
                {
                    _splitterDrag = false;
                }
                    break;
            }
        }

		#endregion
    }
}