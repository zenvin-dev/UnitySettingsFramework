using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

using static UnityEngine.InputSystem.InputBinding.DisplayStringOptions;

namespace Zenvin.Settings.Samples {
	[CustomPropertyDrawer (typeof (InputActionBinding))]
	internal class InputActionBindingPropertyDrawer : PropertyDrawer {

		private static readonly List<GUIContent> entryNamesList = new List<GUIContent> ();
		private static readonly List<string> entryIdsList = new List<string> ();

		private static GUIContent[] entryNames;
		private static string[] entryIds;


		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {

			var actionRefProp = property.FindPropertyRelative ("actionReference");
			var bindingIdProp = property.FindPropertyRelative ("bindingId");

			label = EditorGUI.BeginProperty (position, label, property);
			position = EditorGUI.PrefixLabel (position, label);


			var actionRefPosition = GetPosition (position, 0);
			EditorGUI.ObjectField (actionRefPosition, actionRefProp, typeof (InputActionReference), GUIContent.none);

			var bindingIdPosition = GetPosition (position, 1);
			DrawBindingMenu (bindingIdPosition, bindingIdProp, actionRefProp.objectReferenceValue as InputActionReference);


			EditorGUI.EndProperty ();
		}

		public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
			return EditorGUIUtility.singleLineHeight * 2f + EditorGUIUtility.standardVerticalSpacing;
		}


		private void DrawBindingMenu (Rect position, SerializedProperty prop, InputActionReference actionRef) {
			if (prop == null || prop.propertyType != SerializedPropertyType.String) {
				return;
			}

			if (actionRef == null) {
				prop.stringValue = "";
				EditorGUI.Popup (position, -1, Array.Empty<GUIContent> ());
				return;
			}

			UpdateEntries (actionRef, prop.stringValue, out var index);
			var newIndex = EditorGUI.Popup (position, index, entryNames);

			if (newIndex != index) {
				prop.stringValue = newIndex >= 0 && newIndex < entryIds.Length ? entryIds[newIndex] : "";
			}
		}

		private void UpdateEntries (InputActionReference actionRef, string currentBindingId, out int bindingIndex) {
			bindingIndex = -1;

			// If there is no InputActionReference assigned, updating is not possible.
			if (actionRef == null) {
				return;
			}

			// Cache references & values for later use
			var action = actionRef.action;
			var bindings = action.bindings;
			var bindingsCount = bindings.Count;

			// Clear cache lists
			entryIdsList.Clear ();
			entryNamesList.Clear ();

			// Iterate over all elements inside the InputActionReference's bindings
			for (int i = 0; i < bindingsCount; i++) {

				// Cache currently iterated binding
				var binding = bindings[i];
				var bindingId = binding.id.ToString ();
				var hasBindingGroups = !string.IsNullOrEmpty (binding.groups);

				var displayOptions = DontUseShortDisplayNames | IgnoreBindingOverrides;
				if (!hasBindingGroups) {
					displayOptions |= DontOmitDevice;
					continue;
				}

				var displayString = action.GetBindingDisplayString (i, displayOptions);

				if (binding.isPartOfComposite) {
					displayString = $"{ObjectNames.NicifyVariableName (binding.name)}: {displayString}";
				}

				if (hasBindingGroups) {
					var asset = action.actionMap?.asset;
					if (asset != null) {
						var groupNames = binding.groups.Split (InputBinding.Separator);
						var controlSchemes = string.Join (", ", groupNames.Select (x => asset.controlSchemes.FirstOrDefault (c => c.bindingGroup == x).name));

						displayString = $"{displayString} ({controlSchemes})";
					}
				}

				displayString = displayString.Replace ('/', '\\');

				//if (currentBindingId == bindingId) {
				//	bindingIndex = i;
				//}

				//entryNames[i] = new GUIContent (displayString);
				//entryIds[i] = bindingId;
				entryNamesList.Add (new GUIContent (displayString));
				entryIdsList.Add (bindingId);
			}

			// Copy constructed entries to caches
			Fill (ref entryIds, entryIdsList);
			Fill (ref entryNames, entryNamesList);

			bindingIndex = entryIdsList.IndexOf (currentBindingId);
		}

		private static void Fill<T> (ref T[] array, List<T> list) {
			Resize (ref array, list.Count);
			for (int i = 0; i < list.Count; i++) {
				array[i] = list[i];
			}

		}

		private static Rect GetPosition (Rect totalPosition, int index) {
			var line = EditorGUIUtility.singleLineHeight;
			totalPosition.height = line;
			totalPosition.y += (line + EditorGUIUtility.standardVerticalSpacing) * index;
			return totalPosition;
		}

		private static void Resize<T> (ref T[] array, int length) {
			if (length < 0) {
				length = 0;
			}
			if (array == null || array.Length != length) {
				array = new T[length];
			}
		}
	}
}