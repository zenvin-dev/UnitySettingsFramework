using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Zenvin.Settings.Framework {
	internal static class ReferenceFixUtility {

		private static readonly StringBuilder sb = new StringBuilder();

		public static void FixAsset (SerializedObject obj) {
			if (obj == null || !(obj.targetObject is SettingsAsset sa)) {
				return;
			}
			FixAsset (obj, sa);
			Debug.Log (sb.ToString ());
			sb.Clear ();
		}

		private static void FixAsset (SerializedObject obj, SettingsAsset asset) {
			Log ("<b>SettingsAsset verification log</b>", 0);
			Log ($"Start verifying asset '{asset.name}'...", 0, 1);

			if (Application.isPlaying) {
				Log ("Error: Application is in play mode.", 1);
				return;
			}

			string path = AssetDatabase.GetAssetPath (asset);
			if (path == null) {
				Log ("Error: No associated asset path found.", 1);
				return;
			}

			List<SettingBase> settings = new List<SettingBase> ();
			List<SettingsGroup> groups = new List<SettingsGroup> ();

			Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath (path);
			Log ($"Found {allAssets.Length - 1} sub-assets.", 1);


			Log ("Pass 1: Delete assets with missing/invalid types...", 1, 1);
			ClearInvalidObjects (allAssets, groups, settings, out int deleted, out int cleared);
			Log ($"Deleted {deleted} assets and cleared {cleared} null references.", 2);

			Log ($"Evaluation found {groups.Count} total groups and {settings.Count} total settings.", 1, 1);

			Log ("Pass 2: Restore references...", 1, 1);
			int groupsRestored = RestoreGroupReferences (asset, groups);
			int settingsRestored = RestoreSettingReferences (asset, groups, settings);
			Log ($"Restored {groupsRestored} group references and {settingsRestored} setting references.", 2);

			obj.ApplyModifiedProperties ();
			AssetDatabase.Refresh ();
			AssetDatabase.SaveAssets ();
			Log ("Asset integrity verified.", 0, 1);
		}

		private static void ClearInvalidObjects (Object[] allAssets, List<SettingsGroup> groups, List<SettingBase> settings, out int deleted, out int cleared) {
			deleted = 0;
			cleared = 0;
			foreach (var subAsset in allAssets) {
				if (subAsset is SettingsAsset sa) {
					ClearNullReferences (sa, ref cleared);
					continue;
				}

				if (subAsset is ScriptableObject so && MonoScript.FromScriptableObject (so) == null) {
					Log ($"Deleting sub-asset {so.name} ({so.GetInstanceID ()}), due to missing MonoScript.", 2);
					Object.DestroyImmediate (so);
					deleted++;
					continue;
				}

				switch (subAsset) {
					case SettingsGroup group:
						ClearNullReferences (group, ref cleared);
						groups.Add (group);
						break;
					case SettingBase setting:
						settings.Add (setting);
						break;
				}
			}
		}

		private static void ClearNullReferences (SettingsGroup group, ref int cleared) {
			for (int i = group.groups.Count - 1; i >= 0; i--) {
				if (group.groups[i] == null) {
					group.groups.RemoveAt (i);
					i--;
					cleared++;
				}
			}
			for (int i = group.settings.Count - 1; i >= 0; i--) {
				if (group.settings[i] == null) {
					group.settings.RemoveAt (i);
					i--;
					cleared++;
				}
			}
		}

		private static int RestoreGroupReferences (SettingsAsset asset, List<SettingsGroup> groups) {
			int referenced = 0;

			foreach (var group in groups) {
				if (group.Parent != null) {
					continue;
				}

				SettingsGroup parentGroup = asset;
				foreach (var _group in groups) {
					if (_group != group && _group.groups.Contains (group)) {
						parentGroup = _group;
						break;
					}
				}

				group.Parent = parentGroup;
				if (parentGroup == asset && !parentGroup.groups.Contains (group)) {
					parentGroup.groups.Add (group);
				}

				referenced++;
				Log ($"Established group {group} as child of group {parentGroup}.", 2);
			}

			return referenced;
		}

		private static int RestoreSettingReferences (SettingsAsset asset, List<SettingsGroup> groups, List<SettingBase> settings) {
			int referenced = 0;

			foreach (var setting in settings) {
				if (setting.asset == null) {
					setting.asset = asset;
				}

				if (setting.group != null) {
					continue;
				}

				SettingsGroup parentGroup = asset;
				foreach (var group in groups) {
					if (group.settings.Contains (setting)) {
						parentGroup = group;
						break;
					}
				}

				setting.group = parentGroup;
				if (parentGroup == asset && !parentGroup.settings.Contains (setting)) {
					parentGroup.settings.Add (setting);
				}

				referenced++;
				Log ($"Established setting {setting} as child of group {parentGroup}.", 2);
			}

			return referenced;
		}

		private static void Log (string message, int indent, int lineBreaks = 0) {
			sb.AppendLine (new string ('\n', lineBreaks) + new string (' ', indent * 4) + message);
		}

	}
}