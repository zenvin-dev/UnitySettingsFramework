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

			var allAssets = AssetDatabase.LoadAllAssetsAtPath (path);
			Log ($"Found {allAssets.Length - 1} sub-assets.", 1);

			List<SettingBase> settings = new List<SettingBase> ();
			List<SettingsGroup> groups = new List<SettingsGroup> ();

			Log ("Pass 1: Delete assets with missing/invalid types...", 1, 1);
			int deleted = 0;
			foreach (var subAsset in allAssets) {
				if (subAsset is SettingsAsset) {
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
						groups.Add (group);
						break;
					case SettingBase setting:
						settings.Add (setting);
						break;
				}
			}
			Log ($"Deleted {deleted} assets.", 2);

			Log ("Pass 2: Restore references...", 1, 1);
			int restored = 0;
			for (int i = 0; i < settings.Count; i++) {
				SettingBase set = settings[i];

				if (set == null) {
					continue;
				}
				if (set.asset != null && set.group != null) {
					continue;
				}
				if (set.group == null) {
					SettingsGroup parentGroup = asset;
					foreach (var grp in groups) {
						if (grp != null && grp.Settings.Contains (set)) {
							parentGroup = grp;
							break;
						}
					}
					set.group = parentGroup;
					if (!parentGroup.settings.Contains (set)) {
						parentGroup.settings.Add (set);
					}
					restored++;
					Log ($"Established setting {set} as child of group {parentGroup}.", 2);
				}
				if (set.asset == null) {
					set.asset = asset;
					restored++;
					Log ($"Restored asset reference in setting {set}.", 2);
				}
			}
			for (int i = 0; i < groups.Count; i++) {
				SettingsGroup group = groups[i];

				if (group == null) {
					continue;
				}
				if (group.Parent == null) {
					SettingsGroup parentGroup = asset;
					foreach (var grp in groups) {
						if (grp != group && grp.Groups.Contains (group)) {
							parentGroup = grp;
							break;
						}
					}

					group.Parent = parentGroup;
					if (!parentGroup.groups.Contains (group)) {
						parentGroup.groups.Add (group);
					}
					restored++;
					Log ($"Re-established group {group} as child of group {parentGroup}.", 2);
				}

			}
			Log ($"Restored {restored} references.", 2);


			Log ("Pass 3: Clear null references...", 1, 1);
			groups.Add (asset);
			int cleaned = 0;
			for (int i = groups.Count - 1; i >= 0; i--) {
				if (groups[i] == null) {
					groups.RemoveAt (i);
					i--;
					cleaned++;
					continue;
				}

				for (int j = groups[i].settings.Count - 1; j >= 0; j--) {
					if (groups[i].settings[j] == null) {
						groups[i].settings.RemoveAt (j);
						j--;
						cleaned++;
					}
				}
			}
			Log ($"Cleared {cleaned} null references.", 2);

			obj.ApplyModifiedProperties ();
			AssetDatabase.Refresh ();
			AssetDatabase.SaveAssets ();
			Log ("Asset integrity verified.", 0, 1);
		}


		private static void Log (string message, int indent, int lineBreaks = 0) {
			sb.AppendLine (new string ('\n', lineBreaks) + new string (' ', indent * 4) + message);
		}

	}
}