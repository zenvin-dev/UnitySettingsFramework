using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace Zenvin.Settings.Samples {
	[CustomPropertyDrawer (typeof (MixerVariable))]
	public class MixerVariablePropertyDrawer : PropertyDrawer {

		private static readonly List<string> paramNames = new List<string> ();
		private SerializedObject mixerObject;


		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
			// Get serialized properties
			var mixerProp = property.FindPropertyRelative ("mixer");
			var paramProp = property.FindPropertyRelative ("variable");

			// Draw prefix label
			position.height = EditorGUIUtility.singleLineHeight;
			position = EditorGUI.PrefixLabel (position, label);

			// Calculate property field rects
			var mixerRect = new Rect (position);
			var paramRect = new Rect (position);
			paramRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

			// Draw property fields
			DrawMixer (mixerRect, mixerProp);
			DrawParam (paramRect, paramProp, mixerProp?.objectReferenceValue as AudioMixer);
		}

		private void DrawMixer (Rect position, SerializedProperty prop) {
			if (prop == null) {
				EditorGUI.LabelField (position, "Error: Mixer property not found.");
				return;
			}
			// Draw object field for mixer reference
			EditorGUI.PropertyField (position, prop, GUIContent.none);
		}

		private void DrawParam (Rect position, SerializedProperty prop, AudioMixer mixer) {
			if (prop == null || mixer == null) {
				GUI.enabled = false;
				EditorGUI.LabelField (position, "", EditorStyles.popup);
				GUI.enabled = true;
				return;
			}

			if (mixerObject == null || mixerObject.targetObject != mixer) {
				mixerObject = new SerializedObject (mixer);
			}

			// Find the serialized list of exposed parameters
			var paramsProp = mixerObject.FindProperty ("m_ExposedParameters");
			if (paramsProp == null) {
				EditorGUI.LabelField (position, "Error: ExposedParameters property not found.");
				return;
			}

			if (GUI.Button (position, prop.stringValue, EditorStyles.popup)) {
				ShowParameterSelection (position, prop, paramsProp);
			}
		}

		public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
			return EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
		}


		private void ShowParameterSelection (Rect position, SerializedProperty originalParamsProp, SerializedProperty paramsProp) {
			paramNames.Clear ();
			while (paramsProp.Next (true)) {
				if (paramsProp.depth == 2 && paramsProp.name == "name") {
					paramNames.Add (paramsProp.stringValue);
				}
			}

			var menu = new GenericMenu ();
			for (int i = 0; i < paramNames.Count - 1; i++) {
				var name = paramNames[i];
				menu.AddItem (new GUIContent (name), false, SelectParameterName, (originalParamsProp, name));
			}
			menu.DropDown (position);
		}

		private static void SelectParameterName (object obj) {
			var values = obj as (SerializedProperty prop, string value)?;
			if (!values.HasValue) {
				return;
			}
			if (values.Value.prop != null && values.Value.prop.propertyType == SerializedPropertyType.String) {
				values.Value.prop.stringValue = values.Value.value;
				values.Value.prop.serializedObject.ApplyModifiedProperties ();
			}
		}
	}
}