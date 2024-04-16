using System;
using System.Collections.Generic;
using UnityEngine;
using Zenvin.Settings.Framework.Serialization;

namespace Zenvin.Settings.Framework {
	/// <summary>
	/// <see cref="ScriptableObject"/> that contains all Settings and Settings Groups.
	/// </summary>
	[CreateAssetMenu (menuName = "Scriptable Objects/Zenvin/Settings Asset", fileName = "New Settings")]
	public sealed class SettingsAsset : SettingsGroup {

		/// <summary>
		/// Delegate type for the <see cref="OnInitialize"/> event.
		/// </summary>
		/// <param name="asset"> The <see cref="SettingsAsset"/> that was initialized. </param>
		public delegate void InitializedEvent (SettingsAsset asset);
		/// <summary>
		/// Delegate type for the <see cref="OnRuntimeSettingsLoaded"/> event.
		/// </summary>
		/// <param name="asset"> The <see cref="SettingsAsset"/> that the Settings (and Groups) were loaded into. </param>
		public delegate void RuntimeSettingsLoadedEvent (SettingsAsset asset);


		/// <summary> Invoked for every <see cref="SettingsAsset"/> during the initialization process. Preferably hook into this to load runtime settings. </summary>
		public static event InitializedEvent OnInitialize;
		/// <summary> Invoked for every <see cref="SettingsAsset"/> after runtime settings has been loaded, but <b>only if the asset was already initialized</b>. </summary>
		public static event RuntimeSettingsLoadedEvent OnRuntimeSettingsLoaded;

		[NonSerialized] private readonly Dictionary<string, SettingBase> settingsDict = new Dictionary<string, SettingBase> ();
		[NonSerialized] private readonly Dictionary<string, SettingsGroup> groupsDict = new Dictionary<string, SettingsGroup> ();

		[NonSerialized] private readonly HashSet<SettingBase> dirtySettings = new HashSet<SettingBase> ();
		[NonSerialized] private bool initialized = false;

		[SerializeField, Min (0)] private int orderStep = 100;
		[SerializeField] private bool enableDebugLogging = false;

		/// <summary> Whether the asset has been initialized. </summary>
		public bool Initialized => initialized;
		/// <summary> The number of Settings registered in the asset. Returns 0 if the asset is not initialized. </summary>
		public int RegisteredSettingsCount => settingsDict.Count;
		/// <summary> The number of Groups registered in the asset. Returns 0 if the asset is not initialized. </summary>
		public int RegisteredGroupsCount => groupsDict.Count;
		/// <summary> The number of Settings registered in the asset, which have been changed but not applied yet. </summary>
		public int DirtySettingsCount => dirtySettings.Count;


		// Initialization
		/// <summary>
		/// Initializes the asset.
		/// </summary>
		public void Initialize () {
			if (!Application.isPlaying) {
				Debug.LogWarning ("Cannot initialize SettingsAsset during edit-time.", this);
				return;
			}
			if (Initialized) {
				Debug.LogWarning ($"SettingsAsset has already been initialized.", this);
				return;
			}

			initialized = true;
			OnBeforeInitialize ();

			RegisterGroupsAndSettingsRecursively (this, groupsDict, settingsDict, false);
			OnInitialize?.Invoke (this);
			RegisterGroupsAndSettingsRecursively (this, groupsDict, settingsDict, true);

			OnAfterInitialize ();
		}

		private void RegisterGroupsAndSettingsRecursively (SettingsGroup group, Dictionary<string, SettingsGroup> groupDict, Dictionary<string, SettingBase> settingDict, bool external) {
			if (group == null) {
				return;
			}

			// keep track of whether this group should be initialized
			var initGroup = false;

			// if group is not root
			if (group != this) {
				// register group
				if (!group.External || !groupDict.ContainsKey (group.GUID)) {
					groupDict[group.GUID] = group;
					initGroup = true;
					group.OnBeforeInitialize ();
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

			// should only run if the group was actually registered
			if (initGroup) {
				// run post-initialization actions on group
				group.OnAfterInitialize ();
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
		/// Gets the group with the associated GUID.
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
		/// Creates a <see cref="DynamicSettingReference{T}"/>, which will automatically update 
		/// when a Setting with the expected GUID and type is added to this <see cref="SettingsAsset"/>.
		/// </summary>
		/// <typeparam name="T">The type of Setting that the created reference should wrap.</typeparam>
		/// <param name="guid">
		/// The GUID that the created dynamic reference will look out for.<br></br>
		/// This value must not be <see langword="null"/> or white space.
		/// </param>
		/// <param name="fallback">The fallback value returned by <see cref="SettingReference{T}.Fallback"/> as long as no <see cref="SettingBase{T}"/> is referenced.</param>
		public DynamicSettingReference<T> GetSettingReference<T> (string guid, T fallback = default) {
			if (string.IsNullOrWhiteSpace (guid)) {
				return null;
			}
			var dyn = new DynamicSettingReference<T> (this, guid);
			dyn.Fallback = fallback;
			return dyn;
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
			Log ($"Applying all dirty Settings (Initialized: {Initialized}, Registered: {settingsDict.Count}, Dirty: {dirtySettings.Count})");
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
			Log ($"Reverting all dirty Settings (Initialized: {Initialized}, Registered: {settingsDict.Count}, Dirty: {dirtySettings.Count})");
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
			Log ($"Resetting all Settings (Initialized: {Initialized}, Registered: {settingsDict.Count}, Apply reset values: {apply})");
			foreach (var s in settingsDict.Values) {
				s.ResetValue (apply);
			}
		}


		// Saving & Loading
		/// <summary>
		/// Serializes all Settings to the provided <see cref="ISerializer{T}"/>.<br></br>
		/// Returns <see langword="true"/> if serialization was successful.<br></br>
		/// The <see cref="SettingsAsset"/> needs to be initialized before serialization can take place.
		/// </summary>
		/// <typeparam name="T">The type of object used to manage serialized data.</typeparam>
		/// <param name="serializer">The serializer used to serialize data.</param>
		/// <param name="filter">Filter for deciding which settings should be considered for serialization. Leave at <see langword="null"/> to not filter any.</param>
		public bool SerializeSettings<T> (ISerializer<T> serializer, SettingBaseFilter filter = null) where T : class, new() {
			if (!Initialized || serializer == null) {
				return false;
			}

			Log ($"Serializing Settings using '{typeof (ISerializer<T>)}' (Target type '{typeof (T)}', Filtered: {filter != null})");

			var advSer = serializer as ISerializerCallbackReceiver;
			advSer?.InitializeSerialization ();

			foreach (SettingBase setting in settingsDict.Values) {
				if (filter != null && !filter (setting)) {
					continue;
				}

				if (setting is ISerializable<T> serializable) {
					T obj = new T ();
					serializable.OnSerialize (obj);
					serializer.Serialize (setting.GUID, obj);
				}
			}

			advSer?.FinalizeSerialization ();
			return true;
		}

		/// <summary>
		/// Deserializes Setting values from the provided <see cref="ISerializer{T}"/>.<br></br>
		/// Returns <see langword="true"/> if deserialization was successful.<br></br>
		/// The <see cref="SettingsAsset"/> needs to be initialized before deserialization can take place.
		/// <see cref="SettingBase.ApplyValue"/> will automatically be called on any successfully deserialized Setting.<br></br>
		/// </summary>
		/// <typeparam name="T">The type of object used to manage deserialize data.</typeparam>
		/// <param name="serializer">The serializer used to deserialize data.</param>
		/// <param name="filter">Filter for deciding which settings should be considered for deserialization. Leave at <see langword="null"/> to not filter any.</param>
		public bool DeserializeSettings<T> (ISerializer<T> serializer, SettingBaseFilter filter = null) where T : class, new() {
			if (!Initialized || serializer == null) {
				return false;
			}

			Log ($"Deserializing Settings using '{typeof (ISerializer<T>)}' (Target type '{typeof (T)}', Filtered: {filter != null})");

			var advSer = serializer as ISerializerCallbackReceiver;
			advSer?.InitializeDeserialization ();

			var enumerator = serializer.GetSerializedData ();
			if (enumerator != null) {
				while (enumerator.MoveNext ()) {
					var data = enumerator.Current;

					if (settingsDict.TryGetValue (data.Key, out SettingBase setting) && (filter == null || filter (setting)) && setting is ISerializable<T> serializable) {
						serializable.OnDeserialize (data.Value);
						setting.ApplyValue ();
						setting.OnAfterDeserialize ();
					}
				}
			}


			advSer?.FinalizeDeserialization ();
			return true;
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

		internal void ProcessRuntimeSettingsIntegration () {
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

		internal void Log (string message) {
			if (enableDebugLogging) {
				Debug.Log ("[Settings Framework] " + message);
			}
		}

	}
}