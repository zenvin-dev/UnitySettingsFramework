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
			groupList.AddRange (group.groups);
			groupList.AddRange (group.externalGroups);

			foreach (SettingsGroup g in group.groups) {
				GetGroupsRecursively (g, groupList);
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

		public virtual ReadOnlyCollection<SettingBase> GetSettings () {
			List<SettingBase> settingsList = new List<SettingBase> ();

			settingsList.AddRange (settings);
			settingsList.AddRange (externalSettings);

			return settingsList.AsReadOnly ();
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
			settingsList.AddRange (group.settings);
			settingsList.AddRange (group.externalSettings);

			foreach (SettingsGroup g in group.groups) {
				GetSettingsRecursively (g, settingsList);
			}
		}

	}
}