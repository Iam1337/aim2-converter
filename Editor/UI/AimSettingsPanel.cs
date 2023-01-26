/* Copyright (c) 2023 dr. ext (Vladimir Sigalkin) */

using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AimConverter.UI
{
    public class AimSettingsPanel : AimPanel
    {
        public event Action<IAimProcessor, AimModel, string, string> OnRefreshModel;

        private AimModel _model;
        private IAimProcessor _modelProcessor;
        private AimSubMesh _selectedSubMesh;
        private string _sourceModelsDirectory;
        private string _sourceTextureDirectory;
        private Vector2 _scrollPosition;
        private GUIContent[] _processorsNames;
        private IAimProcessor[] _processors;
        
        private readonly float _itemHeight = 20f;
        private readonly float _materialHeight = 200f;
        private readonly float _importHeight = 100f;

        public AimSettingsPanel(EditorWindow window) : base(window) => RefreshProcessors();

        public void SelectModel(string model, string sourceModelsDirectory, string sourceTextureDirectory)
        {
            if (string.IsNullOrEmpty(model))
                return;
            
            _model = AimConverter.LoadModel(_modelProcessor, model);
            _sourceModelsDirectory = sourceModelsDirectory;
            _sourceTextureDirectory = sourceTextureDirectory;

            OnRefreshModel?.Invoke(_modelProcessor, _model, _sourceModelsDirectory, _sourceTextureDirectory);
        }
        
        protected override void DrawContent(Rect contentRect)
        {  
            GUILayout.Label("Import Settings:", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope("box"))
            {
                var currentIndex = Math.Max(Array.IndexOf(_processors, _modelProcessor), 0);
                var processorIndex = EditorGUILayout.Popup(new GUIContent("Processor"), currentIndex, _processorsNames);
                if (processorIndex != currentIndex)
                {
                    _modelProcessor = _processors[processorIndex];
                    
                    OnRefreshModel?.Invoke(_modelProcessor, _model, _sourceModelsDirectory, _sourceTextureDirectory);
                }
            }
            
            GUILayout.Label("Sub Meshes:", EditorStyles.boldLabel);
            var fieldsRect = GUILayoutUtility.GetLastRect();
            contentRect.y += fieldsRect.y + fieldsRect.height;
            contentRect.height -= fieldsRect.y + fieldsRect.height;
            
            // draw import type
            var importRect = new Rect();
            importRect.x = contentRect.width - _importHeight - 10f;
            importRect.y = fieldsRect.y;
            importRect.height = fieldsRect.height;
            importRect.width = _importHeight;
            GUI.Label(importRect, "Import Type", EditorStyles.miniLabel);
            
            // draw material type
            var materialRect = new Rect();
            materialRect.x = importRect.x - _materialHeight - 5f;
            materialRect.y = fieldsRect.y;
            materialRect.width = _materialHeight;
            materialRect.height = fieldsRect.height;
            GUI.Label(materialRect, "Material Type", EditorStyles.miniLabel);
            
            var texturesRect = new Rect();
            texturesRect.x = materialRect.x - 15f * 4f - 4f;
            texturesRect.y = fieldsRect.y;
            texturesRect.width = 15f * 4f;
            texturesRect.height = fieldsRect.height;
            GUI.Label(texturesRect, "Tex. set", EditorStyles.miniLabel);

            GUI.Box(contentRect, GUIContent.none);
            
            if (_model != null)
            {
                var subMeshes = _model.SubMeshes;
                var viewRect = new Rect(contentRect);
                viewRect.width = contentRect.width;
                viewRect.height = subMeshes.Length * _itemHeight;
                if (viewRect.height > contentRect.height)
                    viewRect.width -= GUI.skin.verticalScrollbar.fixedWidth;

                var itemRect = new Rect(0, contentRect.y, contentRect.width, _itemHeight);

                using (var scroll = new GUI.ScrollViewScope(contentRect, _scrollPosition, viewRect))
                {
                    for (var index = 0; index < subMeshes.Length; index++)
                    {
                        var subMesh = subMeshes[index];
                        var modelSelected = _selectedSubMesh == subMesh;

                        var cacheColor = GUI.color;
                        DrawItem(itemRect, index, subMesh, modelSelected);
                        GUI.color = cacheColor;

                        if (Event.current.type == EventType.MouseDown && itemRect.Contains(Event.current.mousePosition))
                        {
                            GUIUtility.keyboardControl = 0;

                            if (Event.current.button == 0)
                            {
                                if (_selectedSubMesh != subMesh)
                                {
                                    _selectedSubMesh = subMesh;
                                    Repaint();
                                }

                                Event.current.Use();
                            }
                        }

                        itemRect.y += itemRect.height;
                    }

                    _scrollPosition = scroll.scrollPosition;
                }
            }
        }
        
        protected void DrawItem(Rect itemRect, int index, AimSubMesh subMesh, bool selected)
        {
            using (new GUI.GroupScope(itemRect))
            {
                itemRect.x = itemRect.y = 0;

                // draw background
                if (Event.current.type == EventType.Repaint)
                {
                    var cacheColor = GUI.color;

                    if (selected) 
                        GUI.color = Color.yellow;

                    var backStyle = index % 2 != 0 ? AimEditorStyles.ConsoleItemBackEven : AimEditorStyles.ConsoleItemBackOdd;
                    backStyle.Draw(itemRect, false, false, selected, false);

                    GUI.color = cacheColor;
                }

                // draw name
                var labelRect = new Rect();
                labelRect.x = 10f;
                labelRect.width = itemRect.width - labelRect.x;
                labelRect.height = itemRect.height;
                GUI.Label(labelRect, subMesh.NicifyName, AimEditorStyles.ConsoleLabel);
                
                // draw import type
                var importRect = new Rect();
                importRect.x = itemRect.width - _importHeight - 10f;
                importRect.width = _importHeight;
                importRect.height = itemRect.height;
                var importType = (ImportType)EditorGUI.EnumFlagsField(importRect, subMesh.ImportType);
                if (importType != subMesh.ImportType)
                {
                    subMesh.ImportType = importType;
                    OnRefreshModel?.Invoke(_modelProcessor, _model, _sourceModelsDirectory, _sourceTextureDirectory);
                    Repaint();
                }
                
                // draw material type
                var materialRect = new Rect();
                materialRect.x = importRect.x - _materialHeight - 5f;
                materialRect.width = _materialHeight;
                materialRect.height = itemRect.height;
                var materialType = (MaterialType)EditorGUI.EnumPopup(materialRect, subMesh.MaterialType);
                if (materialType != subMesh.MaterialType)
                {
                    subMesh.MaterialType = materialType;
                    OnRefreshModel?.Invoke(_modelProcessor, _model, _sourceModelsDirectory, _sourceTextureDirectory);
                    Repaint();
                }
                
                // draw textures
                var texturesRect = new Rect();
                texturesRect.x = materialRect.x - 15f * 4f - 4f;
                texturesRect.width = 14f;
                texturesRect.height = itemRect.height;
                GUI.Toggle(texturesRect, IsTextureAvailable(subMesh.AlbedoName), new GUIContent(string.Empty, subMesh.AlbedoName));
                texturesRect.x += 15f;
                GUI.Toggle(texturesRect, IsTextureAvailable(subMesh.SpecularName), new GUIContent(string.Empty, subMesh.SpecularName));
                texturesRect.x += 15f;
                GUI.Toggle(texturesRect, IsTextureAvailable(subMesh.Unknown3Name), new GUIContent(string.Empty, subMesh.Unknown3Name));
                texturesRect.x += 15f;
                GUI.Toggle(texturesRect, IsTextureAvailable(subMesh.Unknown4Name), new GUIContent(string.Empty, subMesh.Unknown4Name));
            }
        }

        private bool IsTextureAvailable(string textureName)
        {
            return !string.IsNullOrWhiteSpace(textureName) && textureName != "_DEFAULT_";
        }

        private void RefreshProcessors()
        {
            try
            {
                IAimProcessor Instantiate(Type type)
                {
                    var constructor = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                                          .FirstOrDefault(p => p.GetParameters().Length == 0);
                    if (constructor == null)
                        constructor = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                                          .FirstOrDefault(p => p.GetParameters().Length == 0);

                    var t = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                    return (IAimProcessor)constructor.Invoke(Array.Empty<object>());
                }

                var processorType = typeof(IAimProcessor);
                bool Predicate(Type p) => processorType.IsAssignableFrom(p) && p != processorType && !p.IsAbstract;
                var processorTypes = AppDomain.CurrentDomain.GetAssemblies()
                                              .SelectMany(s => s.GetTypes())
                                              .Where(Predicate).ToList();

                var processors = processorTypes.Select(Instantiate).ToList();
                processors.Sort((a, b) => a.Index.CompareTo(b.Index));


                _processorsNames = processors.Select(p => new GUIContent(p.Name)).ToArray();
                _processors = processors.ToArray();
                
                _modelProcessor = _processors.First();
            }
            catch (Exception e)
            {
                Debug.LogError($"[CONVERTER] Exception: {e}");

                _processorsNames = new[] { new GUIContent(AimConverter.DefaultProcessor.Name) };
                _processors = new[] { AimConverter.DefaultProcessor };

                _modelProcessor = AimConverter.DefaultProcessor;
            }
        }
    }
}