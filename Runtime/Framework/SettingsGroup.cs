using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Zenvin.Settings.Framework {
	/// <summary>
	/// A collection of <see cref="SettingBase"/> and other <see cref="SettingsGroup"/> objects.
	/// </summary>
	public class SettingsGroup : ComposedFrameworkObject {

		/// <summary>
		/// Delegate type for filtering <see cref="SettingBase"/> objects.
		/// </summary>
		/// <param name="setting"> The Setting to filter. </param>
		/// <returns> Whether the given <see cref="SettingBase"/> is valid for the filter. </returns>
		public delegate bool SettingBaseFilter (SettingBase setting);
		/// <summary>
		/// Delegate type for filtering <see cref="SettingsGroup"/> objects.
		/// </summary>
		/// <param name="group"> The Group to filter. </param>
		/// <returns> Whether the given <see cref="SettingsGroup"/> is valid for the filter. </returns>
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
		/// <summary> The Group's parent Group. </summary>
		public SettingsGroup Parent {
			get => parent;
			internal set => parent = value;
		}
		/// <summary> The total count of direct child Groups, including both internal and external Groups. </summary>
		public int ChildGroupCount => InternalChildGroupCount + externalGroups.Count;
		/// <summary> The total count of child Settings, including both internal and external Settings. </summary>
		public int SettingCount => InternalSettingCount + externalSettings.Count;

		internal List<SettingsGroup> Groups => groups;
		internal List<SettingsGroup> ExternalGroups => externalGroups;
		internal List<SettingBase> Settings => settings;
		internal List<SettingBase> ExternalSettings => externalSettings;

		private int InternalChildGroupCount => groups?.Count ?? 0;
		private int InternalSettingCount => settings?.Count ?? 0;


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
		/// Gets a read-only collection of all direct child Groups.
		/// </summary>
		public ReadOnlyCollection<SettingsGroup> GetGroups () {
			List<SettingsGroup> groupsList = new List<SettingsGroup> ();

			groupsList.AddRange (groups);
			groupsList.AddRange (externalGroups);

			return groupsList.AsReadOnly ();
		}

		/// <summary>
		/// Iterates direct child groups, without allocating an entire new collection.
		/// </summary>
		public IEnumerable<SettingsGroup> IterateGroups (bool includeExternal) {
			for (int i = 0; i < groups.Count; i++) {
				yield return groups[i];
			}
			if (includeExternal) {
				for (int i = 0; i < externalGroups.Count; i++) {
					yield return externalGroups[i];
				}
			}
		}

		/// <summary>
		/// Recursively gets a read-only collection of all child Groups.
		/// </summary>
		public ReadOnlyCollection<SettingsGroup> GetAllGroups () {
			List<SettingsGroup> groups = new List<SettingsGroup> ();
			GetGroupsRecursively (this, groups);
			return groups.AsReadOnly ();
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
		/// Iterates all direct child settings, without allocating an entire new collection.
		/// </summary>
		public IEnumerable<SettingBase> IterateSettings (bool includeExternal) {
			for (int i = 0; i < settings.Count; i++) {
				yield return settings[i];
			}
			if (includeExternal) {
				for (int i = 0; i < externalSettings.Count; i++) {
					yield return externalSettings[i];
				}
			}
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
		/// Gets all direct child Settings that are deemed valid by the given filter.
		/// </summary>
		public virtual ReadOnlyCollection<SettingBase> GetFilteredSettings (SettingBaseFilter isValid, bool sorted = false) {
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

			return settingsList.AsReadOnly ();
		}

		/// <summary>
		/// Gets all child Settings that are deemed valid by the given filter.
		/// </summary>
		public virtual ReadOnlyCollection<SettingBase> GetAllFilteredSettings (SettingBaseFilter isValid, bool sorted = false) {
			List<SettingBase> settingsList = new List<SettingBase> (settings.Count);
			GetSettingsRecursively (this, settingsList);

			if (sorted) {
				settingsList.Sort ();
			}
			return settingsList.AsReadOnly ();
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
		/// <param name="applyAfterReset"> Value to pass to <see cref="SettingBase.ResetValue(bool)"/>, to determine whether each Setting's value should be applied after resetting. </param>
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


		/// <summary>
		/// Called during <see cref="CreateInstanceWithValues{T}(StringValuePair[])"/>.
		/// </summary>
		protected virtual void OnCreateWithValues (StringValuePair[] values) { }


		/// <summary>
		/// Called when the Group's <see cref="SettingsAsset"/> is initialized, before any of the Group's children are initialized.
		/// </summary>
		/// <remarks>
		/// Initialization for external Groups happens separately and always after that for internal ones.
		/// </remarks>
		internal protected virtual void OnBeforeInitialize () { }

		/// <summary>
		/// Called when the Group's <see cref="SettingsAsset"/> is initialized, after all the Group's children have been initialized.
		/// </summary>
		/// <remarks>
		/// Initialization for external Groups happens separately and always after that for internal ones.
		/// </remarks>
		internal protected virtual void OnAfterInitialize () { }


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
		}

		internal void RemoveSetting (SettingBase setting) {
			if (setting == null || setting.group != this) {
				return;
			}
			settings.Remove (setting);
		}

		internal void InitializeGroup (bool before = true, bool after = true) {
			if (before)
				OnBeforeInitialize ();

			if (after)
				OnAfterInitialize ();
		}


		private protected void RegisterAndInitializeRecursively (SettingsAsset root, Dictionary<string, SettingsGroup> groupDict, Dictionary<string, SettingBase> settingDict, bool external) {
			if (root == null || groupDict == null || settingDict == null)
				return;

			if (!groupDict.ContainsKey (GUID)) {
				groupDict[GUID] = this;
			}

			// self-initialization only needs to happen when the current group is NOT the SettingsAsset
			var initSelf = this != root && External == external;
			if (initSelf) {
				InitializeGroup (before: true, after: false);
			}

			// iterate different collections, depending on internal or external requirement
			var settings = external ? externalSettings : this.settings;

			// always iterate normal child groups, because they or their children may have received external Settings/Groups
			InitializeGroups (groups);
			// only iterate external groups if "external" is set
			if (external) {
				InitializeGroups (externalGroups);
			}

			if (settings != null) {
				for (int i = 0; i < settings.Count; i++) {
					var setting = settings[i];
					if (setting == null)
						continue;

					setting.RegisterAndInitialize (root, settingDict, settings.Count - i);
				}
			}

			if (initSelf) {
				InitializeGroup (before: false, after: true);
			}


			void InitializeGroups (List<SettingsGroup> targetGroups) {
				if (targetGroups != null) {
					for (int i = 0; i < targetGroups.Count; i++) {
						var group = targetGroups[i];
						if (group == null)
							continue;

						// initialize child group
						group.RegisterAndInitializeRecursively (root, groupDict, settingDict, external);
					}
				}
			}
		}

		private protected sealed override void OnSetVisibilityInternal (SettingVisibility visibility) {
			PropagateVisiblityEvent (this);
		}

		private protected void Uninitialize () {
			Initialized = false;
			if (Parent != null) {
				Parent.Uninitialize ();
			}
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


		/// <inheritdoc/>
		public override string ToString () {
			return $"Group '{Name}' ('{GUID}')";
		}
	}
}