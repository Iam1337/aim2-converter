using System;
using UnityEditor;
using UnityEngine;

namespace AimConverter.UI
{
    public class AimPanel : IDisposable
    {
        #region Public Vars
        
        private readonly EditorWindow _window;

        #endregion

        #region Private Vars

        private Rect _drawRect;

        #endregion

        #region Public Methods

        public void Draw(Rect drawRect)
        {
            using (new GUILayout.AreaScope(drawRect))
            {
                var contentRect = drawRect;
                contentRect.x = contentRect.y = 0;

                DrawContent(contentRect);
                PostDrawContent(contentRect);
            }
        }
        
        public virtual void Dispose()
        { }

        #endregion

        #region Protected Methods
        
        protected AimPanel(EditorWindow window) => _window = window;

        protected void Repaint() => _window.Repaint();

        protected virtual void DrawContent(Rect contentRect)
        { }

        protected virtual void PostDrawContent(Rect contentRect)
        { }

        #endregion
    }
}