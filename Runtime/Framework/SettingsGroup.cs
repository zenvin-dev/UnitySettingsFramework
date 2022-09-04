using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Zenvin.Settings.Framework {
	public class SettingsGroup : FrameworkObject {

		public delegate bool SettingBaseFilter (SettingBase setting);
		public delegate bool SettingsGroupFilter (SettingsGroup group);


		[NonSerialized] private readonly List<SettingBase> externalSettings = new List<SettingBase> ();
		[NonSerialized] private readonly List<SettingsGroup> externalGroups = new List<SettingsGroup> ();

		[SerializeField, HideInInspector] internal Sprite groupIcon;

		[SerializeField, HideInInspector] private SettingsGroup parent;
		[SerializeField, HideInInspector] internal List<SettingsGroup> groups;
		[SerializeField, HideInInspector] internal List<SettingBase> settings;


		/// <summary> The icon assigned in the inspector. </summary>
		public Sprite Icon {
			get => groupIcon;
			internal set => groupIcon = value;
		}

		private int InternalChildGroupCount => groups?.Count ?? 0;
		/// <summary> The total count of child Groups, including both internal and external Groups. </summary>
		public int ChildGroupCount => InternalChildGroupCount + externalGroups.Count;

		private int InternalSettingCount => settings?.Count ?? 0;
		/// <summary> The total count of child Settings, including both internal and external Settings. </summary>
		public int SettingCount => InternalSettingCount + externalSettings.Count;

		protected internal List<SettingsGroup> Groups => groups;
		protected internal List<SettingsGroup> ExternalGroups => externalGroups;
		protected internal List<SettingBase> Settings => settings;
		protected internal List<SettingBase> ExternalSettings => externalSettings;

		/// <summary> The Group's parent Group. </summary>
		public SettingsGroup Parent {
			get => parent;
			internal set => parent = value;
		}


		/// <summary>
		/// Creates a new, external instance of a given <see cref="SettingBase{T}"/> and initializes with the given values.
		/// </summary>
		public static T CreateInstanceWithValues<T> (StringValuePair[] values = null) where T : SettingsGroup {
			if (!Application.isPlaying) {
				return null;
			}
			if (typeof (T) == typeof (SettingsAsset)) {
				return null;
			}

			T group = CreateInstance<T> ();
			group.External = true;

			group.OnCreateWithValues (values);

			return group;
		}

		/// <summary>
		/// Called during <see cref="CreateInstanceWithValues{T}(StringValuePair[])"/>.
		/// </summary>
		protected virtual void OnCreateWithValues (StringValuePair[] values) { }

		/// <summary>
		/// Get the child Setting at the given index. Null if the index is invalid.
		/// </summary>
		public SettingsGroup GetGroupAt (int index) {
			if (index < 0 || index >= ChildGroupCount) {
				return null;
			}
			if (index < InternalChildGroupCount) {
				return groups[index];
			}
			return externalGroups[index - InternalChildGroupCount];
		}

		/// <summary>
		/// Gets all direct child Groups.
		/// </summary>
		public ReadOnlyCollection<SettingsGroup> GetGroups () {
			List<SettingsGroup> groupsList = new List<SettingsGroup> ();

			groupsList.AddRange (groups);
			groupsList.AddRange (externalGroups);

			return groupsList.AsReadOnly ();
		}

		/// <summary>
		/// Recursively gets all child Groups.
		/// </summary>
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
				if (index < groups.IndexOf (group)) {
					index++;
				}
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


		/// <summary>
		/// Get the child Group at the given index. Null if the index is invalid.
		/// </summary>
		public SettingBase GetSettingAt (int index) {
			if (index < 0 || index >= SettingCount) {
				return null;
			}
			if (index < InternalSettingCount) {
				return settings[index];
			}
			return externalSettings[index - InternalSettingCount];
		}

		/// <summary>
		/// Gets all direct child Settings.
		/// </summary>
		public List<SettingBase> GetSettings (bool sorted = false) {
			List<SettingBase> settingsList = new List<SettingBase> (settings?.Count ?? 0);

			if (settings != null) {
				settingsList.AddRange (settings);
			}
			settingsList.AddRange (externalSettings);

			if (sorted) {
				settingsList.Sort ();
			}

			return settingsList;
		}

		/// <summary>
		/// Recursively gets all child Settings.
		/// </summary>
		public virtual List<SettingBase> GetAllSettings (bool sorted = false) {
			List<SettingBase> settings = new List<SettingBase> ();
			GetSettingsRecursively (this, settings);

			if (sorted) {
				settings.Sort ();
			}
			return settings;
		}

		/// <summary>
		/// Gets all direct child Settings that are deemed valid.
		/// </summary>
		public virtual List<SettingBase> GetFilteredSettings (SettingBaseFilter isValid, bool sorted = false) {
			List<SettingBase> settingsList = new List<SettingBase> (settings?.Count ?? 0);

			if (settings != null) {
				for (int i = 0; i < settings.Count; i++) {
					if (isValid.Invoke (settings[i])) {
						settingsList.Add (settings[i]);
					}
				}
			}
			for (int i = 0; i < externalSettings.Count; i++) {
				if (isValid.Invoke (externalSettings[i])) {
					settingsList.Add (externalSettings[i]);
				}
			}

			if (sorted) {
				settingsList.Sort ();
			}

			return settingsList;
		}

		public virtual List<SettingBase> GetAllFilteredSettings (SettingBaseFilter isValid, bool sorted = false) {
			List<SettingBase> settingsList = new List<SettingBase> (settings.Count);
			GetSettingsRecursively (this, settingsList);

			if (sorted) {
				settingsList.Sort ();
			}
			return settingsList;
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
			if (setting == null || !setting.External) {
				return;
			}
			setting.group = this;
			externalSettings.Add (setting);
			OnIntegratedSetting (setting);
		}

		private protected virtual void OnIntegratedSetting (SettingBase setting) { }

		internal void RemoveSetting (SettingBase setting) {
			if (setting == null || setting.group != this) {
				return;
			}
			settings.Remove (setting);
		}

		private void GetSettingsRecursively (SettingsGroup group, List<SettingBase> settingsList, SettingBaseFilter isValid = null) {
			if (group == null) {
				return;
			}

			if (isValid == null) {
				if (group.settings != null) {
					settingsList.AddRange (group.settings);
				}
				if (group.externalSettings != null) {
					settingsList.AddRange (group.externalSettings);
				}
			} else {
				if (settings != null) {
					for (int i = 0; i < settings.Count; i++) {
						if (isValid.Invoke (settings[i])) {
							settingsList.Add (settings[i]);
						}
					}
				}
				if (externalSettings != null) {
					for (int i = 0; i < externalSettings.Count; i++) {
						if (isValid.Invoke (externalSettings[i])) {
							settingsList.Add (externalSettings[i]);
						}
					}
				}
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


		/// <inheritdoc/>
		public sealed override SettingVisibility GetVisibilityInHierarchy () {
			SettingVisibility vis = Visibility;
			SettingsGroup obj = this;
			while (obj != null && obj.Parent != null) {
				if ((int)obj.Visibility < (int)obj.Parent.Visibility) {
					vis = obj.Parent.Visibility;
				}
				if (vis == SettingVisibility.Hidden) {
					return vis;
				}
				obj = obj.Parent;
			}
			return vis;
		}

		private protected sealed override void OnSetVisibilityInternal (SettingVisibility visibility) {
			PropagateVisiblityEvent (this);
		}

		private static void PropagateVisiblityEvent (SettingsGroup group) {
			if (group == null) {
				return;
			}
			group.SetVisibility (group.Visibility, false);

			foreach (var g in group.Groups) {
				PropagateVisiblityEvent (g);
			}
			if (group.ExternalGroups != null) {
				foreach (var g in group.ExternalGroups) {
					PropagateVisiblityEvent (g);
				}
			}

			foreach (var s in group.Settings) {
				s.SetVisibility (s.Visibility, false);
			}
			if (group.ExternalSettings != null) {
				foreach (var s in group.ExternalSettings) {
					s.SetVisibility (s.Visibility, false);
				}
			}
		}


		public override string ToString () {
			return $"Group '{Name}' ('{GUID}')";
		}

	}
}