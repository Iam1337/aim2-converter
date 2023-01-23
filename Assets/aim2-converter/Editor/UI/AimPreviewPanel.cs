/* Copyright (c) 2023 dr. ext (Vladimir Sigalkin) */

using UnityEditor;
using UnityEngine;

namespace AimConverter.UI
{
    public class AimPreviewPanel : AimPanel
    {
        private IAimProcessor _modelProcessor;
        private AimModel _model;
        private string _sourceModelsDirectory;
        private string _sourceTextureDirectory;
        
        private GameObject _previewObject;
        private Editor _previewEditor;
        
        public AimPreviewPanel(EditorWindow window) : base(window)
        { }

        public void RefreshModel(IAimProcessor processor, AimModel model, string sourceModelsDirectory, string sourceTextureDirectory)
        {
            if (_previewObject != null)
                Object.DestroyImmediate(_previewObject);
            if (_previewEditor != null)
                Object.DestroyImmediate(_previewEditor);

            _modelProcessor = processor;
            _model = model;
            _sourceModelsDirectory = sourceModelsDirectory;
            _sourceTextureDirectory = sourceTextureDirectory;

            if (_model == null)
                return;

            _previewObject = AimConverter.Preview(processor, model, sourceTextureDirectory);
            if (_previewObject != null)
            {
                HideObject(_previewObject);
                _previewEditor = Editor.CreateEditor(_previewObject);
            }
        }

        public override void Dispose()
        {
            _model = null;
            if (_previewObject != null)
                Object.DestroyImmediate(_previewObject);
            if (_previewEditor != null) 
                Object.DestroyImmediate(_previewEditor);
        }
        
        protected override void DrawContent(Rect contentRect)
        {
            DrawToolbar(ref contentRect);
            
            var style = new GUIStyle();
            style.normal.background = EditorGUIUtility.whiteTexture;

            if (_previewEditor != null) 
                _previewEditor.OnInteractivePreviewGUI(contentRect, style);
        }

        private void HideObject(GameObject gameObject)
        {
            gameObject.hideFlags = HideFlags.HideAndDontSave;
            foreach (Transform child in gameObject.transform) 
                HideObject(child.gameObject);
        }
        
        private void DrawToolbar(ref Rect contentRect)
        {
            var importButton = false;

#if UNITY_2019_3_OR_NEWER
            var toolbarSize = 22;
#else
			var toolbarSize = 18;
#endif

            contentRect.y += toolbarSize;
            contentRect.height -= toolbarSize;

            using (new GUILayout.AreaScope(new Rect(0, 0, contentRect.width, toolbarSize)))
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    using (new EditorGUILayout.HorizontalScope(UnityEditor.EditorStyles.toolbar))
                    {
                        GUI.enabled = _model != null;
                        GUILayout.FlexibleSpace();
                        importButton = GUILayout.Button("Import", UnityEditor.EditorStyles.toolbarButton);
                        GUILayout.Space(5f);
                        GUI.enabled = true;
                    }
                }
            }

            if (importButton)
            {
                var prefabObject = AimConverter.Import(_modelProcessor, _model, AimConverter.ContentDirectory, _sourceTextureDirectory);
                Object.DestroyImmediate(prefabObject);
            }
        }
    }
}