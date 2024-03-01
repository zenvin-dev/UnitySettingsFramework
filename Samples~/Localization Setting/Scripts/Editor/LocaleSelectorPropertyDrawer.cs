using System;
using System.Collections.ObjectModel;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;

namespace Zenvin.Settings.Samples {
	[CustomPropertyDrawer(typeof(LocaleSelectorAttribute))]
	internal class LocaleSelectorPropertyDrawer : PropertyDrawer {

		private GUIContent[] options;


		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
			if (property.propertyType != SerializedPropertyType.Integer) {
				EditorGUI.PropertyField (position, property, label);
				return;
			}

			var locales = LocalizationEditorSettings.GetLocales ();
			UpdateOptions (locales);

			property.intValue = EditorGUI.Popup (position, label, property.intValue, options);
			property.serializedObject.ApplyModifiedProperties ();
		}

		private void UpdateOptions (ReadOnlyCollection<UnityEngine.Localization.Locale> locales) {
			if (options == null) {
				options = new GUIContent[locales.Count];
			} else if (options.Length != locales.Count) {
				Array.Resize (ref options, locales.Count);
			}

			for (int i = 0; i < options.Length; i++) {
				options[i] = new GUIContent(locales[i].LocaleName);
			}
		}
	}
}
