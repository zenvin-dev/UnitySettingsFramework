using System.Collections.Generic;
using System.IO;
using System;

namespace Zenvin.Settings.Framework {
	public sealed class SettingsAsset : SettingsGroup {

		[NonSerialized] private readonly Dictionary<string, SettingBase> settingsDict = new Dictionary<string, SettingBase> ();
		[NonSerialized] private readonly HashSet<SettingBase> dirtySettings = new HashSet<SettingBase> ();


		// Initialization

		private void Awake () {
			InitializeSettings ();
		}

		private void InitializeSettings () {
			
		}


		// Runtime Loading


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

	}
}