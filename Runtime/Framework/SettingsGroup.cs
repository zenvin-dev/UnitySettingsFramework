using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Zenvin.Settings.Framework {
	public class SettingsGroup : ScriptableObject {

		[SerializeField, HideInInspector] private string guid = null;

		[SerializeField, HideInInspector] internal string groupName;
		[SerializeField, HideInInspector] internal string groupNameLocKey;
		[SerializeField, HideInInspector] internal Sprite groupIcon;

		[SerializeField, HideInInspector] private SettingsGroup parent;
		[SerializeField, HideInInspector] private List<SettingsGroup> groups;
		[SerializeField, HideInInspector] private List<SettingBase> settings;


		public string GUID {
			get => guid;
			internal set => guid = value;
		}
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
		public int ChildGroupCount => groups?.Count ?? 0;
		public int SettingCount => settings?.Count ?? 0;

		public SettingsGroup Parent {
			get => parent;
			private set => parent = value;
		}


		public SettingsGroup GetGroupAt (int index) {
			if (index < 0 || groups == null || index >= groups.Count) {
				return null;
			}
			return groups[index];
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


		public SettingBase GetSettingAt (int index) {
			if (index < 0 || settings == null || index >= settings.Count) {
				return null;
			}
			return settings[index];
		}

		public ReadOnlyCollection<SettingBase> GetSettings () {
			return settings.AsReadOnly ();
		}

		public ReadOnlyCollection<SettingBase> GetAllSettings () {
			List<SettingBase> settings = new List<SettingBase> ();
			GetSettingsRecursively (this, settings);
			return settings.AsReadOnly ();
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

		internal void RemoveSetting (SettingBase setting) {
			if (setting == null || setting.group != this) {
				return;
			}
			settings.Remove (setting);
		}

		private void GetSettingsRecursively (SettingsGroup group, List<SettingBase> settings) {
			settings.AddRange (group.settings);
			foreach (SettingsGroup g in group.groups) {
				GetSettingsRecursively (g, settings);
			}
		}

	}
}