/* Copyright (c) 2023 dr. ext (Vladimir Sigalkin) */

using UnityEngine;

namespace AimConverter.UI
{
	public static class AimEditorStyles
	{
        private static GUIStyle _consoleItemBackEven;
        private static GUIStyle _consoleItemBackOdd;
        private static GUIStyle _consoleLabel;
        private static GUIStyle _searchField;
        private static GUIStyle _searchFieldPlaceholder;

        public static GUIStyle ConsoleItemBackEven
		{
			get
			{
				if (_consoleItemBackEven == null)
                    _consoleItemBackEven = new GUIStyle("CN EntryBackEven");

                return _consoleItemBackEven;
			}
		}

		public static GUIStyle ConsoleItemBackOdd
		{
			get
			{
				if (_consoleItemBackOdd == null)
                    _consoleItemBackOdd = new GUIStyle("CN EntryBackOdd");

                return _consoleItemBackOdd;
			}
		}

		public static GUIStyle ConsoleLabel
		{
			get
			{
				if (_consoleLabel == null)
				{
					_consoleLabel = new GUIStyle(UnityEditor.EditorStyles.label);
					_consoleLabel.richText = true;
				}

				return _consoleLabel;
			}
		}
        
        public static GUIStyle SearchField
        {
            get
            {
                if (_searchField == null) 
                    _searchField = new GUIStyle("toolbarTextField");

                return _searchField;
            }
        }

        public static GUIStyle SearchFieldPlaceholder
        {
            get
            {
                if (_searchFieldPlaceholder == null)
                {
                    _searchFieldPlaceholder = new GUIStyle("toolbarTextField");
                    _searchFieldPlaceholder.active.textColor = Color.gray;
                    _searchFieldPlaceholder.normal.textColor = Color.gray;
                }

                return _searchFieldPlaceholder;
            }
        }
    }
}