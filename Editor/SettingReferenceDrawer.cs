using Zenvin.Settings.Framework;
using UnityEditor;
using UnityEngine;

namespace Zenvin.Settings {
	[CustomPropertyDrawer (typeof (SettingReference<>))]
	public class SettingReferenceDrawer : PropertyDrawer {

		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {

			using (new EditorGUI.DisabledGroupScope (Application.isPlaying)) {
				position.height = EditorGUIUtility.singleLineHeight;
				position = EditorGUI.PrefixLabel (position, label);
				EditorGUI.PropertyField (position, property.FindPropertyRelative ("settingObj"), GUIContent.none);
			}

			const float width = 60f;
			position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

			Rect labelRect = new Rect (position);
			labelRect.width = width;
			EditorGUI.LabelField (labelRect, "Fallback");

			position.width -= width;
			position.x += width;
			EditorGUI.PropertyField (position, property.FindPropertyRelative ("fallbackValue"), GUIContent.none);

			position.x += position.width;
			position.width = width;
		}

		public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
			return EditorGUIUtility.singleLineHeight * 2f + EditorGUIUtility.standardVerticalSpacing;
		}

	}
}