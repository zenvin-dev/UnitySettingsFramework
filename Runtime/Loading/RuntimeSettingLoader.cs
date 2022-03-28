using System.Collections.Generic;
using Zenvin.Settings.Framework;
using UnityEngine;

namespace Zenvin.Settings.Loading {
	public static class RuntimeSettingLoader {

		// allocate resources
		private static readonly Dictionary<string, ISettingFactory> fDict = new Dictionary<string, ISettingFactory> ();
		private static readonly Dictionary<string, string> desiredParents = new Dictionary<string, string> ();
		private static readonly Dictionary<string, SettingsGroup> groups = new Dictionary<string, SettingsGroup> ();
		private static readonly List<(SettingsGroup parent, SettingsGroup group)> rootGroups = new List<(SettingsGroup parent, SettingsGroup group)> ();


		public static bool LoadSettingsIntoAsset (SettingsAsset asset, string json, IGroupIconLoader iconLoader, params ISettingFactory[] factories) {
			if (asset == null || asset.Initialized || string.IsNullOrWhiteSpace (json) || factories == null || factories.Length == 0) {
				return false;
			}

			// parse data from JSON
			SettingsImportData data = null;
			try {
				data = JsonUtility.FromJson<SettingsImportData> (json);
			} catch {
				return false;
			}

			if (data.Settings == null || data.Settings.Length == 0 || data.Groups == null) {
				return false;
			}


			// reset loader state
			fDict.Clear ();
			desiredParents.Clear ();
			groups.Clear ();
			rootGroups.Clear ();

			// initialize factories
			PopulateFactoryDict (factories);

			// create group instances from parsed data
			PopulateGroupDict (asset, data.Groups, iconLoader);

			// link group instances in hierarchy & store groups that will be integrated directly
			EstablishGroupRelationships (asset);

			// clear relationship dictionary for reuse with settings
			desiredParents.Clear ();

			// create settings instances from parsed data
			IntegrateSettings (asset, data.Settings);

			// integrate created root groups into asset
			IntegrateRootGroups (asset);

			return true;
		}

		private static void PopulateFactoryDict (ISettingFactory[] factories) {
			foreach (var f in factories) {
				if (f != null) {
					string fType = f.GetValidType ();
					if (!string.IsNullOrEmpty (fType)) {
						fDict[fType] = f;
					}
				}
			}
		}

		private static void PopulateGroupDict (SettingsAsset asset, GroupData[] groupsData, IGroupIconLoader loader) {
			foreach (var g in groupsData) {
				if (!string.IsNullOrWhiteSpace (g.GUID) && !groups.ContainsKey (g.GUID) && asset.IsValidGuid (g.GUID, true)) {
					SettingsGroup obj = ScriptableObject.CreateInstance<SettingsGroup> ();

					obj.GUID = g.GUID;
					obj.External = true;

					obj.Name = g.Name;
					obj.NameLocalizationKey = g.LocalizationKey;
					obj.Icon = loader?.LoadIconResource (g.IconResource);

					groups.Add (g.GUID, obj);
					desiredParents[g.GUID] = g.ParentGroupGUID;
				}
			}
		}

		private static void EstablishGroupRelationships (SettingsAsset asset) {
			foreach (var _rel in desiredParents) {
				SettingsGroup child = groups[_rel.Key];
				SettingsGroup g;

				if (groups.TryGetValue (_rel.Value, out g)) {
					if (g != child) {
						g.IntegrateChildGroup (child);
					}
				} else if (asset.TryGetGroupByGUID (_rel.Value, out g)) {
					rootGroups.Add ((g, child));
				}
			}
		}

		private static void IntegrateSettings (SettingsAsset asset, SettingData[] settingsData) {
			foreach (var s in settingsData) {
				if (fDict.TryGetValue (s.Type, out ISettingFactory fact) && asset.IsValidGuid (s.GUID, false)) {
					SettingsGroup parent;
					if (!groups.TryGetValue (s.ParentGroupGUID, out parent)) {
						asset.TryGetGroupByGUID (s.ParentGroupGUID, out parent);
					}

					if (parent != null) {
						SettingBase obj = fact.CreateSettingFromType (s.DefaultValue, s.Values);
						if (obj != null) {

							obj.asset = asset;
							obj.GUID = s.GUID;
							obj.External = true;

							obj.Name = s.Name;
							obj.NameLocalizationKey = s.LocalizationKey;

							parent.IntegrateSetting (obj);

						}
					}
				}
			}
		}

		private static void IntegrateRootGroups (SettingsAsset asset) {
			foreach (var g in rootGroups) {
				if (g.group.ChildGroupCount > 0 || g.group.SettingCount > 0) {
					g.parent.IntegrateChildGroup (g.group);
				}
			}
		}

	}
}