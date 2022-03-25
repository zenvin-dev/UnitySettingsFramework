using System.Collections.Generic;
using UnityEngine;

namespace Zenvin.Settings.Framework {
	public class SettingsGroup : ScriptableObject {

		[SerializeField, HideInInspector] private string guid = null;

		[SerializeField] internal string groupName;
		[SerializeField] internal string groupNameLocKey;
		[SerializeField] internal Sprite groupIcon;

		[SerializeField, HideInInspector] private SettingsGroup parent;
		[SerializeField, HideInInspector] private List<SettingsGroup> groups;
		[SerializeField, HideInInspector] private List<SettingBase> settings;


		public string GUID => guid;

		public string Name => groupName;
		public string NameLocalizationKey => groupNameLocKey;
		public Sprite Icon => groupIcon;

		public int ChildGroupCount => groups?.Count ?? 0;
		public int SettingCount => settings?.Count ?? 0;

		public SettingsGroup Parent {
			get => parent;
			private set => parent = value;
		}


		private void OnEnable () {
			if (guid == null) {
				guid = System.Guid.NewGuid ().ToString ();
			}
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
			group.Parent = this;
			groups.Add (group);
		}

		internal void RemoveChildGroup (SettingsGroup group) {
			if (group == null || group.Parent != this) {
				return;
			}
			groups.Remove (group);
		}


		public SettingBase GetSettingAt (int index) {
			if (index < 0 || settings == null || index >= settings.Count) {
				return null;
			}
			return settings[index];
		}

		internal void AddSetting (SettingBase setting) {
			if (setting == null || setting.group == this || setting == this) {
				return;
			}
			if (setting.group != null) {
				setting.group.RemoveSetting (setting);
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

	}
}