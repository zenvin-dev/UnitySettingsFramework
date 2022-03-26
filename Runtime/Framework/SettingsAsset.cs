using System.Collections.Generic;
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
		[NonSerialized] private bool initialized;

		[SerializeField] private bool prefixInternalGuids = true;
		[SerializeField] private char internalGuidPrefix = '$';
		[Space]
		[SerializeField] private bool prefixExternalGuids = false;
		[SerializeField] private char externalGuidPrefix = ' ';


		public bool Initialized => initialized;


		// Initialization

		public void Initialize () {
			if (!Initialized) {
				InitializeSettings ();
			}
		}

		private void InitializeSettings () {
			Debug.Log ("Start register  Settings.");

			RegisterSettingsRecursively (this, settingsDict, false);
			RegisterGroupsRecursively (this, groupsDict, false);

			OnInitialize?.Invoke (this);

			RegisterSettingsRecursively (this, settingsDict, true);
			RegisterGroupsRecursively (this, groupsDict, true);

			Debug.Log ($"Registered {settingsDict.Count} Settings, {groupsDict.Count} Groups.");
			initialized = true;
		}

		private void RegisterSettingsRecursively (SettingsGroup group, Dictionary<string, SettingBase> dict, bool external) {
			if (group == null) {
				return;
			}

			var settingList = external ? group.ExternalSettings : group.Settings;
			if (settingList != null) {
				foreach (var s in settingList) {
					bool canAdd = !external || !dict.ContainsKey (s.GUID);
					if (canAdd) {
						dict[s.GUID] = s;
						s.Initialize ();
					}
				}
			}

			var groupList = external ? group.ExternalGroups : group.Groups;
			if (groupList != null) {
				foreach (var g in groupList) {
					RegisterSettingsRecursively (g, dict, external);
				}
			}
		}

		private void RegisterGroupsRecursively (SettingsGroup group, Dictionary<string, SettingsGroup> dict, bool external) {
			if (group == null) {
				return;
			}

			if (!external && group == this) {
				dict[group.GUID] = group;
			} else {
				if (!external || !dict.ContainsKey (group.GUID)) {
					dict[group.GUID] = group;
				}
			}

			var groupList = external ? group.ExternalGroups : group.Groups;
			foreach (var g in groupList) {
				RegisterGroupsRecursively (g, dict, external);
			}
		}


		// Component Access

		public bool TryGetSettingByGUID (string guid, out SettingBase setting) {
			if (settingsDict.TryGetValue (guid, out setting)) {
				return true;
			}
			setting = null;
			return false;
		}

		public bool TryGetSettingByGUID<T> (string guid, out SettingBase<T> setting) where T : struct {
			if (settingsDict.TryGetValue (guid, out SettingBase sb)) {
				setting = sb as SettingBase<T>;
				return setting != null;
			}
			setting = null;
			return false;
		}

		public bool TryGetGroupByGUID (string guid, out SettingsGroup group) {
			if (guid == "") {
				group = this;
				return true;
			}
			return groupsDict.TryGetValue (guid, out group);
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

		public void ApplyDirtySettings () {
			SettingBase[] _dirtySettings = new SettingBase[dirtySettings.Count];
			dirtySettings.CopyTo (_dirtySettings);

			foreach (var set in _dirtySettings) {
				set.ApplyValue ();
			}
		}

		public void RevertDirtySettings () {
			SettingBase[] _dirtySettings = new SettingBase[dirtySettings.Count];
			dirtySettings.CopyTo (_dirtySettings);

			foreach (var set in _dirtySettings) {
				set.RevertValue ();
			}
		}


		// Saving & Loading

		public void SaveAllSettings (StreamWriter writer) {

		}

		public void LoadAllSettings (StreamReader reader) {

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