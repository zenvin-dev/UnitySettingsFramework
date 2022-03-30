using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Zenvin.Settings.Framework {
	public class SettingsGroup : IdentifiableScriptableObject {

		[NonSerialized] private readonly List<SettingBase> externalSettings = new List<SettingBase> ();
		[NonSerialized] private readonly List<SettingsGroup> externalGroups = new List<SettingsGroup> ();

		[SerializeField, HideInInspector] internal string groupName;
		[SerializeField, HideInInspector] internal string groupNameLocKey;
		[SerializeField, HideInInspector] internal Sprite groupIcon;

		[SerializeField, HideInInspector] private SettingsGroup parent;
		[SerializeField, HideInInspector] private List<SettingsGroup> groups;
		[SerializeField, HideInInspector] private List<SettingBase> settings;


		public string Name {
			get => groupName;
			internal set => groupName = value;
		}
		public string NameLocalizationKey {
			get => groupNameLocKey;
			internal set => groupNameLocKey = value;
		}
		public Sprite Icon {
			get => groupIcon;
			internal set => groupIcon = value;
		}

		private int InternalChildGroupCount => groups?.Count ?? 0;
		public int ChildGroupCount => InternalChildGroupCount + externalGroups.Count;

		private int InternalSettingCount => settings?.Count ?? 0;
		public int SettingCount => InternalSettingCount + externalSettings.Count;

		protected internal List<SettingsGroup> Groups => groups;
		protected internal List<SettingsGroup> ExternalGroups => externalGroups;
		protected internal List<SettingBase> Settings => settings;
		protected internal List<SettingBase> ExternalSettings => externalSettings;

		public SettingsGroup Parent {
			get => parent;
			private set => parent = value;
		}


		public SettingsGroup GetGroupAt (int index) {
			if (index < 0 || index >= ChildGroupCount) {
				return null;
			}
			if (index < InternalChildGroupCount) {
				return groups[index];
			}
			return externalGroups[index - InternalChildGroupCount];
		}

		public ReadOnlyCollection<SettingsGroup> GetGroups () {
			List<SettingsGroup> groupsList = new List<SettingsGroup> ();

			groupsList.AddRange (groups);
			groupsList.AddRange (externalGroups);

			return groupsList.AsReadOnly ();
		}

		public ReadOnlyCollection<SettingsGroup> GetAllGroups () {
			List<SettingsGroup> groups = new List<SettingsGroup> ();
			GetGroupsRecursively (this, groups);
			return groups.AsReadOnly ();
		}

		internal void AddChildGroup (SettingsGroup group) {
			if (group == null || group.Parent == this || group == this) {
				return;
			}
			if (group.Parent != null) {
				group.Parent.RemoveChildGroup (group);
			}
			if (groups == null) {
				groups = new List<SettingsGroup> ();
			}
			group.Parent = this;
			groups.Add (group);
		}

		internal void AddChildGroup (SettingsGroup group, int index) {
			if (groups == null) {
				groups = new List<SettingsGroup> ();
			}

			index = Mathf.Clamp (index, 0, groups.Count);

			if (group.Parent == this) {
				groups.Remove (group);
			} else if (group.Parent != null) {
				group.Parent.RemoveChildGroup (group);
				group.Parent = this;
			}

			groups.Insert (index, group);
		}

		internal void IntegrateChildGroup (SettingsGroup group) {
			if (group == null) {
				return;
			}
			group.Parent = this;
			externalGroups.Add (group);
			OnIntegratedChildGroup (group);
		}

		private protected virtual void OnIntegratedChildGroup (SettingsGroup group) { }

		internal void RemoveChildGroup (SettingsGroup group) {
			if (group == null || group.Parent != this) {
				return;
			}
			groups.Remove (group);
		}

		internal bool IsIndirectChildGroup (SettingsGroup group) {
			return !IsDirectChildGroup (group) && IsNestedChildGroup (group);
		}

		internal bool IsDirectChildGroup (SettingsGroup group) {
			return groups?.Contains (group) ?? false;
		}

		internal bool IsNestedChildGroup (SettingsGroup group) {
			return IsNestedChildGroupInternal (group, this);
		}

		private bool IsNestedChildGroupInternal (SettingsGroup child, SettingsGroup group) {
			if (group?.groups?.Contains (child) ?? false) {
				return true;
			}
			if (group == null || group.groups == null) {
				return false;
			}
			foreach (var g in group.groups) {
				return IsNestedChildGroupInternal (child, g);
			}
			return false;
		}

		private void GetGroupsRecursively (SettingsGroup group, List<SettingsGroup> groupList) {
			if (group == null) {
				return;
			}

			if (group.groups != null) {
				groupList.AddRange (group.groups);
			}
			if (group.externalGroups != null) {
				groupList.AddRange (group.externalGroups);
			}

			if (group.groups != null) {
				foreach (SettingsGroup g in group.groups) {
					GetGroupsRecursively (g, groupList);
				}
			}
		}


		public SettingBase GetSettingAt (int index) {
			if (index < 0 || index >= SettingCount) {
				return null;
			}
			if (index < InternalSettingCount) {
				return settings[index];
			}
			return externalSettings[index - InternalSettingCount];
		}

		public virtual List<SettingBase> GetSettings () {
			List<SettingBase> settingsList = new List<SettingBase> ();

			if (settings != null) {
				settingsList.AddRange (settings);
			}
			settingsList.AddRange (externalSettings);

			return settingsList;
		}

		public List<SettingBase> GetAllSettings () {
			List<SettingBase> settings = new List<SettingBase> ();
			GetSettingsRecursively (this, settings);
			return settings;
		}

		internal void AddSetting (SettingBase setting) {
			if (setting == null || setting.group == this) {
				return;
			}
			if (setting.group != null) {
				setting.group.RemoveSetting (setting);
			}
			if (settings == null) {
				settings = new List<SettingBase> ();
			}
			setting.group = this;
			settings.Add (setting);
		}

		internal void AddSetting (SettingBase setting, int index) {
			if (settings == null) {
				settings = new List<SettingBase> ();
			}
			if (setting == null) {
				return;
			}

			Debug.Log ($"Inserting Setting '{setting.Name}' into {index}");
			index = Mathf.Clamp (index, 0, settings.Count);

			if (setting.group != this) {
				setting.group.RemoveSetting (setting);
			} else {
				if (settings[index] == setting) {
					return;
				}
				settings.Remove (setting);
				if (index > settings.Count) {
					index--;
				}
			}

			setting.group = this;
			settings.Insert (index, setting);
		}

		internal void IntegrateSetting (SettingBase setting) {
			if (setting == null) {
				return;
			}
			setting.group = this;
			externalSettings.Add (setting);
		}

		internal void RemoveSetting (SettingBase setting) {
			if (setting == null || setting.group != this) {
				return;
			}
			settings.Remove (setting);
		}

		private void GetSettingsRecursively (SettingsGroup group, List<SettingBase> settingsList) {
			if (group == null) {
				return;
			}

			if (group.settings != null) {
				settingsList.AddRange (group.settings);
			}
			if (group.externalSettings != null) {
				settingsList.AddRange (group.externalSettings);
			}

			if (group.groups != null) {
				foreach (SettingsGroup g in group.groups) {
					GetSettingsRecursively (g, settingsList);
				}
			}
		}


		/// <summary>
		/// Applies all dirty Settings within this group.<br></br>
		/// Note that <see cref="SettingsAsset.ApplyDirtySettings"/> can be more performant, because of how dirty Settings are managed internally.
		/// </summary>
		/// <param name="includeChildGroups"> If true, this method will also apply dirty Settings in all child groups. </param>
		public void ApplyDirtyGroupSettings (bool includeChildGroups) {
			var settings = includeChildGroups ? GetAllSettings () : GetSettings ();
			foreach (var setting in settings) {
				if (setting.IsDirty) {
					setting.ApplyValue ();
				}
			}
		}

		/// <summary>
		/// Reverts all dirty Settings within this group to their current, unapplied values.<br></br>
		/// Note that <see cref="SettingsAsset.RevertDirtySettings"/> can be more performant, because of how dirty Settings are managed internally.
		/// </summary>
		/// <param name="includeChildGroups"> If true, this method will also apply dirty Settings in all child groups. </param>
		public void RevertDirtyGroupSettings (bool includeChildGroups) {
			var settings = includeChildGroups ? GetAllSettings () : GetSettings ();
			foreach (var setting in settings) {
				if (setting.IsDirty) {
					setting.RevertValue ();
				}
			}
		}

		/// <summary>
		/// Resets all Settings within this group to their default values.<br></br>
		/// Note that <see cref="SettingsAsset.ResetAllSettings(bool)"/> can be more performant, because of how dirty Settings are managed internally.
		/// </summary>
		/// <param name="includeChildGroups"> If true, this method will also apply dirty Settings in all child groups. </param>
		public void ResetAllGroupSettings (bool includeChildGroups, bool applyAfterReset) {
			var settings = includeChildGroups ? GetAllSettings () : GetSettings ();
			foreach (var setting in settings) {
				setting.ResetValue (applyAfterReset);
			}
		}


		public override string ToString () {
			return $"Group '{Name}' ('{GUID}')";
		}

	}
}