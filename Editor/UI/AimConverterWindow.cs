using UnityEditor;
using UnityEngine;

namespace AimConverter.UI
{
    public class AimConverterWindow : EditorWindow
    {
        public static readonly string SourceModelsDirectoryKey = "aim2-converter.sourcemodelsdirectory";
        public static readonly string SourceTextureDirectoryKey = "aim2-converter.sourcetexturesdirectory";

        [MenuItem("Window/AIM2 Converter")]
        private static void Open()
        {
            var instance = GetWindow<AimConverterWindow>(true, "AIM2 Converter", true);
            instance.minSize = new Vector2(610, 200);
            instance.Show();
        }

        private AimPanelSplit _rootPanel;
        private AimListPanel _listPanel;
        private AimPreviewPanel _previewPanel;
        private AimSettingsPanel _settingsPanel;
        
        protected void OnEnable()
        {
            _listPanel = new AimListPanel(this);
            _listPanel.SourceModelsDirectory = EditorPrefs.GetString(SourceModelsDirectoryKey, "E:\\");
            _listPanel.SourceTextureDirectory = EditorPrefs.GetString(SourceTextureDirectoryKey, "E:\\");
            
            _previewPanel = new AimPreviewPanel(this);
            _settingsPanel = new AimSettingsPanel(this);

            _listPanel.OnSelectModel += _settingsPanel.SelectModel;
            _settingsPanel.OnRefreshModel += _previewPanel.RefreshModel;
            
            var splitPanel = new AimPanelSplit(this);
            splitPanel.Setup(AimPanelSplit.SplitOrientation.Vertical, _previewPanel, _settingsPanel, 0.5f);

            _rootPanel = new AimPanelSplit(this);
            _rootPanel.Setup(AimPanelSplit.SplitOrientation.Horizontal, _listPanel, splitPanel, 0.5f);
        }

        protected void OnGUI() => _rootPanel.Draw(new Rect(0, 0, position.width, position.height));

        protected void OnDisable() => _rootPanel.Dispose();
    }
}