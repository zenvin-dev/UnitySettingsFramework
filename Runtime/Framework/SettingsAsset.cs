using System.Collections.Generic;
using Zenvin.Settings.Utility;
using UnityEngine;
using System.IO;
using System;
using Zenvin.Settings.Framework.Serialization;

namespace Zenvin.Settings.Framework {
	/// <summary>
	/// <see cref="ScriptableObject"/> that contains all Settings and Settings Groups.
	/// </summary>
	[CreateAssetMenu (menuName = "Scriptable Objects/Zenvin/Settings Asset", fileName = "New Settings")]
	public sealed class SettingsAsset : SettingsGroup {

		public delegate void SettingsAssetEvt (SettingsAsset asset);

		/// <summary> Invoked for every <see cref="SettingsAsset"/> during the initialization process. Preferably hook into this to load runtime settings. </summary>
		public static event SettingsAssetEvt OnInitialize;
		/// <summary> Invoked for every <see cref="SettingsAsset"/> after runtime settings has been loaded, but <b>only if the asset was already initialized</b>. </summary>
		public static event SettingsAssetEvt OnRuntimeSettingsLoaded;

		[NonSerialized] private readonly Dictionary<string, SettingBase> settingsDict = new Dictionary<string, SettingBase> ();
		[NonSerialized] private readonly Dictionary<string, SettingsGroup> groupsDict = new Dictionary<string, SettingsGroup> ();

		[NonSerialized] private readonly HashSet<SettingBase> dirtySettings = new HashSet<SettingBase> ();
		[NonSerialized] private bool initialized = false;

		[SerializeField, Min (0)] private int orderStep = 100;

		/// <summary> Whether the asset has been initialized. </summary>
		public bool Initialized => initialized;
		/// <summary> The number of Settings registered in the asset. Returns 0 if the asset is not initialized. </summary>
		public int RegisteredSettingsCount => settingsDict.Count;
		/// <summary> The number of Groups registered in the asset. Returns 0 if the asset is not initialized. </summary>
		public int RegisteredGroupsCount => groupsDict.Count;
		/// <summary> The number of Settings registered in the asset, which have been changed bot not applied yet. </summary>
		public int DirtySettingsCount => dirtySettings.Count;


		// Initialization

		/// <summary>
		/// Initializes the asset.
		/// </summary>
		public void Initialize () {
			if (!Application.isPlaying) {
				Debug.LogWarning ("Cannot initialize SettingsAsset during edit-time.");
				return;
			}
			if (!Initialized) {
				RegisterGroupsAndSettingsRecursively (this, groupsDict, settingsDict, false);

				OnInitialize?.Invoke (this);

				RegisterGroupsAndSettingsRecursively (this, groupsDict, settingsDict, true);

				initialized = true;
			}
		}

		private void RegisterGroupsAndSettingsRecursively (SettingsGroup group, Dictionary<string, SettingsGroup> groupDict, Dictionary<string, SettingBase> settingDict, bool external) {
			if (group == null) {
				return;
			}

			// if group is not root
			if (group != this) {
				// register group
				if (!group.External || !groupDict.ContainsKey (group.GUID)) {
					groupDict[group.GUID] = group;
				}
			}

			// register settings
			if (external) {
				// register external settings
				foreach (var s in group.ExternalSettings) {
					if (!settingsDict.ContainsKey (s.GUID)) {
						settingsDict[s.GUID] = s;
						s.Initialize ();
					}
				}
			} else if (group.Settings != null) {
				// register internal settings
				int i = group.SettingCount;
				foreach (var s in group.Settings) {
					settingsDict[s.GUID] = s;
					s.OrderInGroup = -i * orderStep;
					s.Initialize ();
					i--;
				}
			}

			// register sub-groups
			if (group.Groups != null) {
				foreach (var g in group.Groups) {
					RegisterGroupsAndSettingsRecursively (g, groupDict, settingDict, external);
				}
			}

			// register external sub-groups, if necessary
			if (external) {
				foreach (var g in group.ExternalGroups) {
					RegisterGroupsAndSettingsRecursively (g, groupDict, settingDict, external);
				}
			}
		}


		// Setting/Group Access
		/// <summary>
		/// Gets the setting with the associated GUID.
		/// </summary>
		/// <param name="guid"> The GUID of the setting to get. </param>
		/// <param name="setting"> Contains the found setting, if any. Otherwise null. </param>
		public bool TryGetSettingByGUID (string guid, out SettingBase setting) {
			if (settingsDict.TryGetValue (guid, out setting)) {
				return true;
			}
			setting = null;
			return false;
		}

		/// <summary>
		/// Gets a typed setting with the associated GUID.
		/// </summary>
		/// <param name="guid"> The GUID of the setting to get. </param>
		/// <param name="setting"> Contains the found setting, if any. Otherwise null. </param>
		public bool TryGetSettingByGUID<T> (string guid, out SettingBase<T> setting) {
			if (settingsDict.TryGetValue (guid, out SettingBase sb)) {
				setting = sb as SettingBase<T>;
				return setting != null;
			}
			setting = null;
			return false;
		}

		/// <summary>
		/// Gets the groupwith the associated GUID.
		/// </summary>
		/// <param name="guid"> The GUID of the group to get. </param>
		/// <param name="group"> Contains the found group, if any. Otherwise null. </param>
		public bool TryGetGroupByGUID (string guid, out SettingsGroup group) {
			if (string.IsNullOrWhiteSpace (guid)) {
				group = this;
				return true;
			}
			return groupsDict.TryGetValue (guid, out group);
		}

		/// <summary>
		/// Gets the groupwith the associated GUID.
		/// </summary>
		/// <param name="guid"> The GUID of the group to get. </param>
		/// <param name="group"> Contains the found group, if any. Otherwise null. </param>
		public bool TryGetGroupByGUID<T> (string guid, out T group) where T : SettingsGroup {
			if (groupsDict.TryGetValue (guid, out SettingsGroup _group)) {
				group = _group as T;
				return group != null;
			}
			group = null;
			return false;
		}

		/// <summary>
		/// Gets a list of all settings registered in the asset.
		/// </summary>
		/// <param name="sorted"> Whether the list should be sorted. </param>
		public override List<SettingBase> GetAllSettings (bool sorted = false) {
			if (initialized) {
				List<SettingBase> settings = new List<SettingBase> (settingsDict.Values);
				if (sorted) {
					settings.Sort ();
				}
				return settings;
			}
			return base.GetAllSettings (sorted);
		}


		// Dirtying settings

		internal void SetDirty (SettingBase setting, bool dirty) {
			if (setting == null) {
				return;
			}
			if (dirty) {
				dirtySettings.Add (setting);
			} else {
				dirtySettings.Remove (setting);
			}
		}

		/// <summary>
		/// Applies all dirty Settings.
		/// </summary>
		public void ApplyDirtySettings () {
			SettingBase[] _dirtySettings = new SettingBase[dirtySettings.Count];
			dirtySettings.CopyTo (_dirtySettings);

			foreach (var set in _dirtySettings) {
				set.ApplyValue ();
			}
		}

		/// <summary>
		/// Reverts all dirty Settings to their current, unapplied values.
		/// </summary>
		public void RevertDirtySettings () {
			SettingBase[] _dirtySettings = new SettingBase[dirtySettings.Count];
			dirtySettings.CopyTo (_dirtySettings);

			foreach (var set in _dirtySettings) {
				set.RevertValue ();
			}
		}

		/// <summary>
		/// Resets all dirty Settings to their default values.
		/// </summary>
		/// <param name="apply"> Apply each Setting after reset? </param>
		public void ResetAllSettings (bool apply) {
			foreach (var s in settingsDict.Values) {
				s.ResetValue (apply);
			}
		}


		// Saving & Loading

		/// <summary>
		/// Saves all Settings to a <see cref="Stream"/>.<br></br>
		/// Returns the number of saved Settings, or -1 if there was an error.<br></br>
		/// The asset needs to be initialized before Settings can be saved.
		/// </summary>
		/// <param name="stream"> The <see cref="Stream"/> the method will write to. </param>
		public int SaveAllSettings (Stream stream, SettingsGroupFilter filter = null) {
			if (stream == null || !Initialized) {
				return -1;
			}

			using (BinaryWriter writer = new BinaryWriter (stream)) {
				List<SettingData> data = new List<SettingData> (settingsDict.Count);

				foreach (SettingBase set in settingsDict.Values) {
					if (filter == null || filter (set.group)) {
						if (set != null && set.TrySerialize (out SettingData sd)) {
							data.Add (sd);
						}
					}
				}

				writer.Write (data.Count);
				for (int i = 0; i < data.Count; i++) {
					writer.Write (data[i].GUID);
					writer.WriteArray (data[i].Data);
				}

				return data.Count;
			}
		}

		/// <summary>
		/// Loads all Settings from a <see cref="Stream"/>.<br></br>
		/// Returns the number of loaded Settings, or -1 if there was an error.<br></br>
		/// The asset needs to be initialized before Settings can be loaded.
		/// </summary>
		/// <param name="reader"> The <see cref="Stream"/> the method will read from. </param>	
		public int LoadAllSettings (Stream stream) {
			if (!Initialized || stream == null || stream.Length - stream.Position == 0) {
				return -1;
			}

			using (BinaryReader reader = new BinaryReader (stream)) {
				int loaded = 0;

				int count = reader.ReadInt32 ();
				for (int i = 0; i < count; i++) {
					string guid = reader.ReadString ();
					byte[] data = reader.ReadArray ();

					if (settingsDict.TryGetValue (guid, out SettingBase setting)) {
						setting.Deserialize (data);
						loaded++;
					}
				}

				return loaded;
			}
		}


		/// <summary>
		/// Compiles a JSON string from the values of all Settings implementing the <see cref="IJsonSerializable"/> interface.<br></br>
		/// Returns the number of saved Settings, of -1 if there was an error.
		/// </summary>
		/// <param name="json">The full JSON string produced by the method.</param>
		/// <param name="filter">Can be used to save only Settings from specific Groups. Leave empty to save all Settings.</param>
		public int SaveAllSettingsJson (out string json, SettingsGroupFilter filter = null) {
			if (!Initialized) {
				json = string.Empty;
				return -1;
			}

			List<SettingDataJson> data = new List<SettingDataJson> ();

			foreach (var setting in settingsDict.Values) {
				if (setting is IJsonSerializable jSerializable) {
					if (filter == null || filter (setting.group)) {
						data.Add (new SettingDataJson (setting.GUID, jSerializable.OnSerializeJson ()));
					}
				}
			}

			json = JsonUtility.ToJson (data);
			return data.Count;
		}

		/// <summary>
		/// Loads all Settings from a JSON string.<br></br>
		/// Returns the number of loaded Settings, or -1 if there was an error.<br></br>
		/// The asset needs to be initialized before Settings can be saved.
		/// </summary>
		/// <param name="json">The JSON string containing Settings' save data.</param>
		public int LoadAllSettingsJson (string json) {
			if (!Initialized) {
				return -1;
			}
			if (json == null) {
				return -1;
			}

			List<SettingDataJson> data;
			try {
				data = JsonUtility.FromJson<List<SettingDataJson>> (json);
			} catch {
				return -1;
			}

			int loaded = 0;

			for (int i = 0; i < data.Count; i++) {
				if (data[i] != null) {
					if (settingsDict.TryGetValue (data[i].GUID, out SettingBase setting) && setting is IJsonSerializable jSerializable) {
						jSerializable.OnDeserializeJson (data[i].Value);
						loaded++;
					}
				}
			}

			return loaded;
		}


		// Integrating runtime settings post-initialization

		private protected override void OnIntegratedChildGroup (SettingsGroup group) {
			if (!initialized) {
				return;
			}
			if (!groupsDict.ContainsKey (group.GUID)) {
				groupsDict[group.GUID] = group;
			}
		}

		private protected override void OnIntegratedSetting (SettingBase setting) {
			if (!initialized) {
				return;
			}
			if (!settingsDict.ContainsKey (setting.GUID)) {
				settingsDict[setting.GUID] = setting;
			}
		}

		internal void RuntimeSettingsIntegrated () {
			if (initialized) {
				OnRuntimeSettingsLoaded?.Invoke (this);
			}
		}


		// Utility

		internal bool Editor_IsValidGUID (string guid, bool isGroup) {
			if (Initialized) {
				return false;
			}
			if (string.IsNullOrWhiteSpace (guid)) {
				return false;
			}

			HashSet<string> guids = new HashSet<string> ();
			guids.Add (guid);

			if (isGroup) {
				var allGroups = GetAllGroups ();
				foreach (var g in allGroups) {
					if (!guids.Add (g.GUID)) {
						return false;
					}
				}
			} else {
				var allSettings = GetAllSettings ();
				foreach (var s in allSettings) {
					if (!guids.Add (s.GUID)) {
						return false;
					}
				}
			}

			return true;
		}

		internal bool IsValidGuid (string guid, bool isGroup) {
			if (!Application.isPlaying) {
				return false;
			}
			if (string.IsNullOrWhiteSpace (guid)) {
				return false;
			}
			return isGroup ? !groupsDict.ContainsKey (guid) : !settingsDict.ContainsKey (guid);
		}

	}
}