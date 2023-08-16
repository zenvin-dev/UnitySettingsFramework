using System;
using System.Collections.Generic;
using UnityEngine;
using Zenvin.Settings.Framework;

namespace Zenvin.Settings.Loading {
	/// <summary>
	/// Utility class for loading additional Settings and Setting Groups into <see cref="SettingsAsset"/>s during runtime.
	/// </summary>
	public static class RuntimeSettingLoader {

		// allocate resources
		private static readonly Dictionary<string, string> desiredParents = new Dictionary<string, string> ();
		private static readonly Dictionary<string, SettingsGroup> groups = new Dictionary<string, SettingsGroup> ();
		private static readonly List<(SettingsGroup parent, SettingsGroup group)> rootGroups = new List<(SettingsGroup parent, SettingsGroup group)> ();


		/// <summary>
		/// Attempts to add Settings and Setting Groups to a given <see cref="SettingsAsset"/>.
		/// </summary>
		/// <param name="asset"> The <see cref="SettingsAsset"/> to load the additional Settings and Setting Groups into. </param>
		/// <param name="json"> Information about the new Settings and Setting Groups. </param>
		/// <param name="iconLoader"> Object for loading Setting Group icons. May be <see langword="null"/>. </param>
		/// <param name="factories"> An arbitrary number of <see cref="IGroupFactory"/> and <see cref="ISettingFactory"/> instances, along with their names. </param>
		/// <remarks>
		/// Can only be used while the game is running.
		/// </remarks>
		public static bool LoadSettingsIntoAsset (SettingsAsset asset, string json, IGroupIconLoader iconLoader, params TypeFactoryWrapper[] factories) {
			if (string.IsNullOrWhiteSpace (json)) {
				return false;
			}

			// parse data from JSON
			SettingsImportData data;
			try {
				data = JsonUtility.FromJson<SettingsImportData> (json);
			} catch {
				return false;
			}

			return LoadSettingsIntoAsset (asset, data, iconLoader, factories);
		}

		/// <summary>
		/// Attempts to add Settings and Setting Groups to a given <see cref="SettingsAsset"/>.<br></br>
		/// </summary>
		/// <param name="asset"> The <see cref="SettingsAsset"/> to load the additional Settings and Setting Groups into. </param>
		/// <param name="data"> Information about the new Settings and Setting Groups. </param>
		/// <param name="iconLoader"> Object for loading Setting Group icons. May be <see langword="null"/>. </param>
		/// <param name="factories"> An arbitrary number of <see cref="IGroupFactory"/> and <see cref="ISettingFactory"/> instances, along with their names. </param>
		/// <remarks>
		/// Can only be used while the game is running.
		/// </remarks>
		public static bool LoadSettingsIntoAsset (SettingsAsset asset, SettingsImportData data, IGroupIconLoader iconLoader, params TypeFactoryWrapper[] factories) {
			if (!Application.isPlaying) {
				return false;
			}

			if (asset == null || factories == null || factories.Length == 0) {
				return false;
			}

			if (data.Settings == null || data.Settings.Length == 0 || data.Groups == null) {
				return false;
			}

			var options = new SettingLoaderOptions (asset)
				.WithData (data)
				.WithTypeFactories (factories)
				.WithDefaultGroupType<SettingsGroup>()
				.WithIconLoader(iconLoader);

			return LoadSettingsIntoAsset (options);
		}

		/// <summary>
		/// Attempts to add Settings and Setting Groups to a given <see cref="SettingsAsset"/>.<br></br>
		/// </summary>
		/// <remarks>
		/// Can only be used while the game is running.
		/// </remarks>
		public static bool LoadSettingsIntoAsset (SettingLoaderOptions options) {
			if (!Application.isPlaying) {
				return false;
			}

			if (options == null || !options.IsValid) {
				return false;
			}

			// create group instances from parsed data
			PopulateGroupDict (options.Asset, options.Data.Groups, options.IconLoader, options.GroupFactories, options.DefaultGroupType);

			// link group instances in hierarchy & store groups that will be integrated directly
			EstablishGroupRelationships (options.Asset);

			// clear relationship dictionary for reuse with settings
			desiredParents.Clear ();

			// create settings instances from parsed data
			IntegrateSettings (options.Asset, options.Data.Settings, options.SettingFactories);

			// integrate created root groups into asset
			IntegrateRootGroups ();

			// run post-integration event
			options.Asset.ProcessRuntimeSettingsIntegration ();

			// reset loader state
			ResetLoaderState ();

			return true;
		}

		private static void PopulateGroupDict (SettingsAsset asset, GroupData[] groupsData, IGroupIconLoader loader, Dictionary<string, IGroupFactory> groupFactories, Type groupDefault) {
			foreach (var g in groupsData) {
				if (asset.IsValidGuid (g.GUID, true) && !groups.ContainsKey (g.GUID)) {
					SettingsGroup obj;
					if (!string.IsNullOrEmpty (g.Type) && groupFactories.TryGetValue (g.Type, out IGroupFactory fact)) {
						obj = fact.CreateGroupFromType (g.Values);
					} else {
						if (groupDefault == null) {
							continue;
						}
						obj = ScriptableObject.CreateInstance (groupDefault) as SettingsGroup;
						obj.External = true;
					}

					if (obj == null || !obj.External) {
						continue;
					}

					obj.GUID = g.GUID;

					obj.Name = g.Name;
					obj.NameLocalizationKey = g.NameLocalizationKey;
					obj.Icon = loader?.LoadIconResource (g.IconResource);

					obj.Description = g.Description;
					obj.DescriptionLocalizationKey = g.DescriptionLocalizationKey;

					obj.SetVisibilityWithoutNotify (g.InitialVisibility);

					groups.Add (g.GUID, obj);
					desiredParents[g.GUID] = g.ParentGroupGUID;
				}
			}
		}

		private static void EstablishGroupRelationships (SettingsAsset asset) {
			foreach (var _rel in desiredParents) {
				SettingsGroup child = groups[_rel.Key];

				if (groups.TryGetValue (_rel.Value, out SettingsGroup g)) {
					if (g != child) {
						g.IntegrateChildGroup (child);
					}
				} else if (asset.TryGetGroupByGUID (_rel.Value, out g)) {
					rootGroups.Add ((g, child));
				}
			}
		}

		private static void IntegrateSettings (SettingsAsset asset, SettingData[] settingsData, Dictionary<string, ISettingFactory> settingFactories) {
			foreach (var s in settingsData) {
				if (settingFactories.TryGetValue (s.Type, out ISettingFactory fact) && asset.IsValidGuid (s.GUID, false)) {
					if (!groups.TryGetValue (s.ParentGroupGUID, out SettingsGroup parent)) {
						asset.TryGetGroupByGUID (s.ParentGroupGUID, out parent);
					}

					if (parent != null) {
						SettingBase obj = fact.CreateSettingFromType (s.DefaultValue, s.Values);
						if (obj != null && obj.External) {

							obj.asset = asset;
							obj.GUID = s.GUID;
							obj.OrderInGroup = s.OrderInGroup;

							obj.Name = s.Name;
							obj.NameLocalizationKey = s.NameLocalizationKey;

							obj.Description = s.Description;
							obj.DescriptionLocalizationKey = s.DescriptionLocalizationKey;

							obj.SetVisibilityWithoutNotify (s.InitialVisibility);

							parent.IntegrateSetting (obj);

						}
					}
				}
			}
		}

		private static void IntegrateRootGroups () {
			foreach (var g in rootGroups) {
				if (g.group.ChildGroupCount > 0 || g.group.SettingCount > 0) {
					g.parent.IntegrateChildGroup (g.group);
				}
			}
		}

		private static void ResetLoaderState () {
			desiredParents.Clear ();
			groups.Clear ();
			rootGroups.Clear ();
		}


		public struct TypeFactoryWrapper {
			public string Type;
			public ISettingFactory SettingFactory;
			public IGroupFactory GroupFactory;
			public readonly bool IsGroupFactory;

			public TypeFactoryWrapper (ISettingFactory factory) : this (null, factory, null) { IsGroupFactory = false; }

			public TypeFactoryWrapper (IGroupFactory factory) : this (null, null, factory) { IsGroupFactory = true; }

			private TypeFactoryWrapper (string type, ISettingFactory settingFactory, IGroupFactory groupFactory) {
				Type = type;
				SettingFactory = settingFactory;
				GroupFactory = groupFactory;
				IsGroupFactory = settingFactory == null;
			}

			public static implicit operator TypeFactoryWrapper ((string, ISettingFactory) tuple) {
				return new TypeFactoryWrapper (tuple.Item1, tuple.Item2, null);
			}

			public static implicit operator TypeFactoryWrapper ((string, IGroupFactory) tuple) {
				return new TypeFactoryWrapper (tuple.Item1, null, tuple.Item2);
			}
		}
	}
}