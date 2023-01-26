using System;
using UnityEngine;

namespace AimConverter.UI
{
    public class AimFilter
    {
        public string FilterValue
		{
			get => _filterValue;
            set => _filterValue = value;
        }

        private readonly string _controlName;
        private string _filterValue = "";

        public AimFilter() => _controlName = "aimfilter_" + Guid.NewGuid();

        public void Draw()
		{
			var fieldPosition = GUILayoutUtility.GetRect(0, 200, 0, 100);
			fieldPosition.y = 2;

			var controlId = GUIUtility.GetControlID("TextField".GetHashCode(), FocusType.Keyboard) + 1;

			GUI.SetNextControlName(_controlName);

			_filterValue = GUI.TextField(fieldPosition, _filterValue, AimEditorStyles.SearchField);

			ProcessKeys(controlId);

			var controlName = GUI.GetNameOfFocusedControl();
			if (controlName != _controlName && string.IsNullOrEmpty(_filterValue))
			{
				GUI.Label(fieldPosition, "Models Filter", AimEditorStyles.SearchFieldPlaceholder);
			}
        }
        
		public void ProcessKeys(int controlId)
		{
			if (controlId == GUIUtility.keyboardControl)
			{
				if (Event.current.type == EventType.KeyUp && (Event.current.modifiers == EventModifiers.Control || Event.current.modifiers == EventModifiers.Command))
				{
					if (Event.current.keyCode == KeyCode.C)
					{
						var editor = (TextEditor) GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
						editor.Copy();

						Event.current.Use();
					}
					else if (Event.current.keyCode == KeyCode.V)
					{
						var textEditor = (TextEditor) GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
						textEditor.Paste();

						_filterValue = textEditor.text;

						Event.current.Use();
					}
					else if (Event.current.keyCode == KeyCode.A)
					{
						var textEditor = (TextEditor) GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
						textEditor.SelectAll();

						Event.current.Use();
					}
				}
			}
		}
    }
}