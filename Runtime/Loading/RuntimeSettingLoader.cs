using System;
using System.Collections.Generic;
using UnityEngine;
using Zenvin.Settings.Framework;
using Zenvin.Settings.Framework.Components;
using Object = UnityEngine.Object;

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
		[Obsolete ("Use LoadSettingsIntoAsset(SettingLoaderOptions) instead.", false)]
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
		[Obsolete ("Use LoadSettingsIntoAsset(SettingLoaderOptions) instead.", false)]
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
				.WithDefaultGroupType<SettingsGroup> ()
				.WithIconLoader (iconLoader);

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
			PopulateGroupDict (options);

			// link group instances in hierarchy & store groups that will be integrated directly
			EstablishGroupRelationships (options.Asset);

			// clear relationship dictionary for reuse with settings
			desiredParents.Clear ();

			// create settings instances from parsed data
			IntegrateSettings (options);

			// integrate created root groups into asset
			IntegrateRootGroups ();

			// run post-integration event
			options.Asset.ProcessRuntimeSettingsIntegration ();

			// apply default overrides, if necessary
			ApplyDefaultOverrides (options.Asset, options.Data.DefaultOverrides, options.OverrideDefaults);

			// reset loader state
			ResetLoaderState ();

			return true;
		}

		private static void PopulateGroupDict (SettingLoaderOptions options) {
			foreach (var g in options.Data.Groups) {
				if (!options.Asset.IsValidGuid (g.GUID, true) || groups.ContainsKey (g.GUID))
					continue;

				SettingsGroup newGroup;
				if (!string.IsNullOrEmpty (g.Type) && options.GroupFactories.TryGetValue (g.Type, out IGroupFactory fact)) {
					newGroup = fact.CreateGroupFromType (g.Values);
				} else {
					if (options.DefaultGroupType == null)
						continue;

					newGroup = ScriptableObject.CreateInstance (options.DefaultGroupType) as SettingsGroup;
					if (newGroup == null)
						continue;

					newGroup.External = true;
				}

				if (newGroup == null || !newGroup.External) {
					continue;
				}

				newGroup.GUID = g.GUID;

				newGroup.Name = g.Name;
				newGroup.NameLocalizationKey = g.NameLocalizationKey;
				newGroup.Icon = options.IconLoader?.LoadIconResource (g.IconResource);

				newGroup.Description = g.Description;
				newGroup.DescriptionLocalizationKey = g.DescriptionLocalizationKey;

				newGroup.SetVisibilityWithoutNotify (g.InitialVisibility);
				CreateComponents (newGroup, g.Components, options.ComponentTypes);

				groups.Add (g.GUID, newGroup);
				desiredParents[g.GUID] = g.ParentGroupGUID;
			}
		}

		private static void EstablishGroupRelationships (SettingsAsset asset) {
			foreach (var _rel in desiredParents) {
				var child = groups[_rel.Key];

				if (groups.TryGetValue (_rel.Value, out SettingsGroup g)) {
					if (g != child) {
						g.IntegrateChildGroup (child);
					}
				} else if (asset.TryGetGroupByGUID (_rel.Value, out g)) {
					rootGroups.Add ((g, child));
				}
			}
		}

		private static void IntegrateSettings (SettingLoaderOptions options) {
			var asset = options.Asset;

			foreach (var s in options.Data.Settings) {
				if (!options.SettingFactories.TryGetValue (s.Type, out ISettingFactory fact) || !asset.IsValidGuid (s.GUID, false))
					continue;

				if (!groups.TryGetValue (s.ParentGroupGUID, out SettingsGroup parent))
					asset.TryGetGroupByGUID (s.ParentGroupGUID, out parent);

				if (parent == null)
					continue;

				var newSetting = fact.CreateSettingFromType (s.DefaultValue, s.Values);
				if (newSetting == null || !newSetting.External) {
					Object.Destroy (newSetting);
					continue;
				}

				newSetting.asset = asset;
				newSetting.GUID = s.GUID;
				newSetting.OrderInGroup = s.OrderInGroup;

				newSetting.Name = s.Name;
				newSetting.NameLocalizationKey = s.NameLocalizationKey;

				newSetting.Description = s.Description;
				newSetting.DescriptionLocalizationKey = s.DescriptionLocalizationKey;

				newSetting.SetVisibilityWithoutNotify (s.InitialVisibility);
				CreateComponents (newSetting, s.Components, options.ComponentTypes);

				if (asset.TryIntegrateSetting (newSetting)) {
					parent.IntegrateSetting (newSetting);
				} else {
					Object.Destroy (newSetting);
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

		private static void ApplyDefaultOverrides (SettingsAsset asset, OverrideData[] defaultOverrides, bool overrideDefaults) {
			if (!overrideDefaults)
				return;
			if (defaultOverrides == null || defaultOverrides.Length == 0)
				return;

			foreach (var o in defaultOverrides) {
				if (o == null)
					continue;

				if (!asset.TryGetSettingByGUID (o.GUID, out var setting))
					continue;

				setting.OverrideDefaultValue (o.Values, o.Update);
			}
		}

		private static void ResetLoaderState () {
			desiredParents.Clear ();
			groups.Clear ();
			rootGroups.Clear ();
		}

		private static void CreateComponents (ComposedFrameworkObject target, ComponentData[] components, Dictionary<string, Type> componentTypes) {
			if (target == null || components == null || components.Length == 0 || componentTypes == null)
				return;

			foreach (var c in components) {
				if (!componentTypes.TryGetValue (c.Type, out var compType))
					continue;

				var newComponent = CreateComponent (target, compType, c.Values);
				if (newComponent == null)
					continue;

				if (!target.TryAddComponentNoContainerCheck (newComponent))
					Object.Destroy (newComponent);
			}
		}

		private static FrameworkComponent CreateComponent (ComposedFrameworkObject container, Type compType, StringValuePair[] values) {
			if (compType == null)
				return null;

			var component = ScriptableObject.CreateInstance (compType) as FrameworkComponent;
			if (component == null)
				return null;

			component.External = true;
			component.BaseContainer = container;
			component.OnCreateWithValues (values);
			return component;
		}


		/// <summary>
		/// A helper struct wrapping either a <see cref="ISettingFactory"/> or a <see cref="IGroupFactory"/> instance, so they can be passed into methods uniformly.
		/// </summary>
		public struct TypeFactoryWrapper {
			public enum FactoryResult : byte {
				Group,
				Setting,
			}

			/// <summary> The type to get from the wrapped factory. </summary>
			public string Type;
			/// <summary> An instance of a <see cref="ISettingFactory"/>. </summary>
			public ISettingFactory SettingFactory;
			/// <summary> An instance of a <see cref="IGroupFactory"/>. </summary>
			public IGroupFactory GroupFactory;
			/// <summary> Switch to determine whether this wrapper is a Group or Setting factory. </summary>
			public readonly FactoryResult? FactoryType;


			public TypeFactoryWrapper (ISettingFactory factory) : this (null, factory, null) { FactoryType = FactoryResult.Setting; }

			public TypeFactoryWrapper (IGroupFactory factory) : this (null, null, factory) { FactoryType = FactoryResult.Group; }

			private TypeFactoryWrapper (string type, ISettingFactory settingFactory, IGroupFactory groupFactory) {
				Type = type;
				FactoryType = null;
				SettingFactory = null;
				GroupFactory = null;

				if (settingFactory != null) {
					SettingFactory = settingFactory;
					FactoryType = FactoryResult.Setting;

				} else if (groupFactory != null) {
					GroupFactory = groupFactory;
					FactoryType = FactoryResult.Group;

				}
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
