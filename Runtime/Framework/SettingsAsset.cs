using System.Collections.Generic;
using Zenvin.Settings.Utility;
using UnityEngine;
using System.IO;
using System;

namespace Zenvin.Settings.Framework {
	public sealed class SettingsAsset : SettingsGroup {

		public delegate void SettingsAssetEvt (SettingsAsset asset);

		public static event SettingsAssetEvt OnInitialize;

		[NonSerialized] private readonly Dictionary<string, SettingBase> settingsDict = new Dictionary<string, SettingBase> ();
		[NonSerialized] private readonly Dictionary<string, SettingsGroup> groupsDict = new Dictionary<string, SettingsGroup> ();

		[NonSerialized] private readonly HashSet<SettingBase> dirtySettings = new HashSet<SettingBase> ();
		[NonSerialized] private bool initialized = false;

		[SerializeField, Min (0)] private int orderStep = 100;

		public bool Initialized => initialized;
		public int RegisteredSettingsCount => settingsDict.Count;
		public int RegisteredGroupsCount => groupsDict.Count;
		public int DirtySettingsCount => dirtySettings.Count;


		// Initialization

		public void Initialize () {
			if (!Application.isPlaying) {
				Debug.LogWarning ("Cannot initialize SettingsAsset in edit-time.");
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

		public bool TryGetSettingByGUID (string guid, out SettingBase setting) {
			if (settingsDict.TryGetValue (guid, out setting)) {
				return true;
			}
			setting = null;
			return false;
		}

		public bool TryGetSettingByGUID<T> (string guid, out SettingBase<T> setting) {
			if (settingsDict.TryGetValue (guid, out SettingBase sb)) {
				setting = sb as SettingBase<T>;
				return setting != null;
			}
			setting = null;
			return false;
		}

		public bool TryGetGroupByGUID (string guid, out SettingsGroup group) {
			if (string.IsNullOrWhiteSpace (guid)) {
				group = this;
				return true;
			}
			return groupsDict.TryGetValue (guid, out group);
		}

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
		/// Saves all settings to a stream.<br></br>
		/// Returns the number of settings saved, or -1 if there was an error.<br></br>
		/// The asset needs to be initialized before settings can be saved.
		/// </summary>
		/// <param name="stream"> The <see cref="Stream"/> the method will write to. </param>
		public int SaveAllSettings (Stream stream) {
			if (stream == null || !Initialized) {
				return -1;
			}

			using (BinaryWriter writer = new BinaryWriter (stream)) {
				List<SettingData> data = new List<SettingData> (settingsDict.Count);

				foreach (SettingBase set in settingsDict.Values) {
					if (set != null && set.TrySerialize (out SettingData sd)) {
						data.Add (sd);
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
		/// Loads all settings from a stream.<br></br>
		/// Returns the number of settings loaded, or -1 if there was an error.<br></br>
		/// The asset needs to be initialized before settings can be loaded.
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