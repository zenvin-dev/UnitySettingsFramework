using UnityEditor;
using UnityEngine;

namespace Zenvin.Settings.Framework {
	[CustomPropertyDrawer (typeof (SettingBase), true)]
	public class SettingBasePropertyDrawer : PropertyDrawer {

		private const string PrefKey = "Zenvin.Settings.DisablePropertyDrawer";

		private static GUIContent openSettingContent;
		private static GUIContent OpenSettingContent {
			get {
				if (openSettingContent == null) {
					openSettingContent = EditorGUIUtility.IconContent ("d_Grid.Default");
					openSettingContent.tooltip = "Open selected Setting in editor.";
				}
				return openSettingContent;
			}
		}
		private static GUIStyle openSettingStyle;
		private static GUIStyle OpenSettingStyle {
			get {
				if (openSettingStyle == null) {
					openSettingStyle = new GUIStyle (EditorStyles.miniButton);
					openSettingStyle.padding = new RectOffset ();
				}
				return openSettingStyle;
			}
		}
		private static GUIStyle dropdownButtonStyle;
		private static GUIStyle DropdownButtonStyle {
			get {
				if (dropdownButtonStyle == null) {
					dropdownButtonStyle = new GUIStyle (EditorStyles.layerMaskField);
					dropdownButtonStyle.richText = true;
				}
				return dropdownButtonStyle;
			}
		}


		[MenuItem ("Tools/Zenvin/Toggle Settings Drawer")]
		private static void ToggleDrawer () {
			EditorPrefs.SetBool (PrefKey, !EditorPrefs.GetBool (PrefKey, false));
		}


		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
			if (EditorPrefs.GetBool (PrefKey, false)) {
				EditorGUI.PropertyField (position, property, label);
				property.serializedObject.ApplyModifiedProperties ();
				return;
			}

			SettingBase setting = property.objectReferenceValue as SettingBase;

			position = EditorGUI.PrefixLabel (position, label);
			Rect buttonRect = new Rect (position);
			buttonRect.width -= EditorGUIUtility.singleLineHeight;
			Rect selectRect = new Rect (position);
			selectRect.x += buttonRect.width;
			selectRect.width = EditorGUIUtility.singleLineHeight;

			if (GUI.Button (buttonRect, GetDropdownButtonContent (setting, true), DropdownButtonStyle)) {
				OpenMenu (property, setting, position);
			}
			if (GUI.Button (selectRect, OpenSettingContent, OpenSettingStyle)) {
				SettingsEditorWindow.Open (setting);
			}
		}

		private GUIContent GetDropdownButtonContent (SettingBase setting, bool richText = false) {
			return new GUIContent (
				setting == null ? "None" : GetSettingName (setting, richText),
				setting == null ? "" : $"Managed Type: {setting.ValueType}"
			);
		}

		private void OpenMenu (SerializedProperty property, SettingBase setting, Rect rect) {
			GenericMenu gm = new GenericMenu ();

			var assets = FindSettingsAssets ();
			for (int i = 0; i < assets.Length; i++) {
				var settings = assets[i].GetAllSettings ();
				for (int j = 0; j < settings.Count; j++) {
					if (!fieldInfo.FieldType.IsAssignableFrom (settings[j].GetType ())) {
						continue;
					}

					int k = j;
					gm.AddItem (
						new GUIContent (assets[i].Name + "/" + GetSettingName (settings[k], false)),
						settings[k] == setting,
						(s) => {
							property.objectReferenceValue = s as SettingBase;
							property.serializedObject.ApplyModifiedProperties ();
						},
						settings[k]
					);
				}
			}

			gm.DropDown (rect);
		}

		private SettingsAsset[] FindSettingsAssets () {
			var guids = AssetDatabase.FindAssets ($"t:{typeof (SettingsAsset)}");
			SettingsAsset[] assets = new SettingsAsset[guids.Length];
			for (int i = 0; i < assets.Length; i++) {
				string path = AssetDatabase.GUIDToAssetPath (guids[i]);
				assets[i] = AssetDatabase.LoadAssetAtPath<SettingsAsset> (path);
			}
			return assets;
		}

		private static string GetSettingName (SettingBase setting, bool richText) {
			if (setting == null) {
				return "";
			}
			if (richText) {
				return $"<b>{setting.Name}</b> ({setting.GUID})";
			}
			return $"{setting.Name} ({setting.GUID})";
		}
	}
}