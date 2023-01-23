/* Copyright (c) 2023 dr. ext (Vladimir Sigalkin) */

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AimConverter.UI
{
    public class AimListPanel : AimPanel
    {
        public event Action<string, string, string> OnSelectModel;

        public string SourceModelsDirectory
        {
            get => _sourceModelsDirectory;
            set
            {
                _sourceModelsDirectory = value;
                RefreshModels();
            }
        }

        public string SourceTextureDirectory
        {
            get => _sourceTextureDirectory;
            set => _sourceTextureDirectory = value;
        }


        private string _sourceModelsDirectory;
        private string _sourceTextureDirectory;
        private Vector2 _scrollPosition;
        private string[] _modelsPath = Array.Empty<string>();
        private string _selectedModelPath;

        private readonly float _itemHeight = 30f;
        
        public AimListPanel(EditorWindow window) : base(window) => RefreshModels();

        protected override void DrawContent(Rect contentRect)
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label($"Selected: {_selectedModelPath ?? "- none -"}");
            }

            using (new EditorGUILayout.VerticalScope("box"))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.TextField("Source Models", _sourceModelsDirectory, GUILayout.ExpandWidth(true));
                    if (GUILayout.Button("Browse", GUILayout.Width(75f)))
                    {
                        var directory = EditorUtility.OpenFolderPanel("Source Models Directory", _sourceModelsDirectory, "");
                        if (Directory.Exists(directory))
                        {
                            _sourceModelsDirectory = directory;
                            EditorPrefs.SetString(AimConverterWindow.SourceModelsDirectoryKey, _sourceModelsDirectory);
                            RefreshModels();
                        }
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.TextField("Source Materials", _sourceTextureDirectory, GUILayout.ExpandWidth(true));
                    if (GUILayout.Button("Browse", GUILayout.Width(75f)))
                    {
                        var directory = EditorUtility.OpenFolderPanel("Source Texture Directory", _sourceTextureDirectory, "");
                        if (Directory.Exists(directory))
                        {
                            _sourceTextureDirectory = directory;
                            EditorPrefs.SetString(AimConverterWindow.SourceTextureDirectoryKey, _sourceTextureDirectory);
                            RefreshModels();
                        }
                    }
                }

                GUI.enabled = false;
                EditorGUILayout.TextField("Target Directory", AimConverter.ContentDirectory);
                GUI.enabled = true;
            }
            
            GUILayout.Label("Models:");
            
            var fieldsRect = GUILayoutUtility.GetLastRect();
            contentRect.y += fieldsRect.y + fieldsRect.height;
            contentRect.height -= fieldsRect.y + fieldsRect.height;

            var viewRect = new Rect(contentRect);
            viewRect.height = _modelsPath.Length * _itemHeight;

            if (viewRect.height > contentRect.height)
                viewRect.width -= GUI.skin.verticalScrollbar.fixedWidth;

            var itemRect = new Rect(0, viewRect.y, viewRect.width, _itemHeight);

            using (var scroll = new GUI.ScrollViewScope(contentRect, _scrollPosition, viewRect))
            {
                var drawed = false;
                for (var index = 0; index < _modelsPath.Length; index++)
                {
                    if (itemRect.y > _scrollPosition.y - itemRect.height &&
                        itemRect.y < _scrollPosition.y + contentRect.y + contentRect.height)
                    {
                        drawed = true;
                        
                        var modelPath = _modelsPath[index];
                        var modelSelected = _selectedModelPath == modelPath;

                        var cacheColor = GUI.color;
                        DrawItem(itemRect, index, modelPath, modelSelected);
                        GUI.color = cacheColor;

                        if (Event.current.type == EventType.MouseDown && itemRect.Contains(Event.current.mousePosition))
                        {
                            GUIUtility.keyboardControl = 0;

                            if (Event.current.button == 0)
                            {
                                if (_selectedModelPath != modelPath)
                                {
                                    _selectedModelPath = modelPath;
                                    OnSelectModel?.Invoke(_selectedModelPath, _sourceModelsDirectory, _sourceTextureDirectory);

                                    Repaint();
                                }

                                Event.current.Use();
                            }
                        }
                    }
                    else if (drawed)
                    {
                        break;
                    }

                    itemRect.y += itemRect.height;
                }

                _scrollPosition = scroll.scrollPosition;
            }
        }

        private void DrawItem(Rect itemRect, int index, string item, bool selected)
        {
            using (new GUI.GroupScope(itemRect))
            {
                var localRect = itemRect;
                localRect.x = localRect.y = 0;

                if (Event.current.type == EventType.Repaint)
                {
                    var cacheColor = GUI.color;

                    if (selected) 
                        GUI.color = Color.yellow;

                    var backStyle = index % 2 != 0 ? AimEditorStyles.ConsoleItemBackEven : AimEditorStyles.ConsoleItemBackOdd;
                    backStyle.Draw(localRect, false, false, selected, false);

                    GUI.color = cacheColor;
                }

                localRect.x += 10;
                localRect.width -= localRect.height;
                GUI.Label(localRect, Path.GetFileName(item).Trim(), AimEditorStyles.ConsoleLabel);

            }
        }
        
        private void RefreshModels()
        {
            _modelsPath = !string.IsNullOrWhiteSpace(_sourceModelsDirectory) ? Directory.GetFiles(_sourceModelsDirectory) : Array.Empty<string>();
            _selectedModelPath = null;

            OnSelectModel?.Invoke(null, null, null);
        }
    }
}