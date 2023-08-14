using Zenvin.Settings.UI;
using UnityEngine;

using Zenvin.Settings.Framework;
using Zenvin.Settings.Framework.Serialization;
using System.IO;

namespace Zenvin.Settings.Samples {
	public class SettingsMenu : MonoBehaviour {

		[SerializeField] private SettingsAsset asset;
		[SerializeField] private bool sortSettings = true;
		[SerializeField] private RectTransform settingHeaderPrefab;
		[SerializeField] private SettingControlCollection controlPrefabs;
		[SerializeField] private TabView tabView;

		private JsonSerializer serializer;


		private void Start () {
			if (asset == null) {
				return;
			}
			SpawnMenu ();

			serializer = new JsonFileSerializer (Path.Combine (Application.dataPath, "settings_test.json"));
		}
		
		private void SpawnMenu () {

			// get settings groups for tabs
			var tabGroups = asset.GetGroups ();

			foreach (var tg in tabGroups) {

				// spawn tab
				RectTransform tabTransform = tabView.AddTab (tg);

				// populate tab
				var tabSettings = tg.GetSettings (sortSettings);
				foreach (var setting in tabSettings) {
					SpawnPrefab (tabTransform, setting);
				}

				var contentGroups = tg.GetAllGroups ();
				foreach (var cg in contentGroups) {

					// spawn group header, if possible
					if (settingHeaderPrefab != null) {
						RectTransform header = Instantiate (settingHeaderPrefab);
						header.SetParent (tabTransform);
						header.localScale = Vector3.one;
					}

					// spawn controls
					var settings = cg.GetAllSettings (sortSettings);
					foreach (var setting in settings) {
						SpawnPrefab (tabTransform, setting);
					}

				}

			}

		}

		private void SpawnPrefab (RectTransform parent, SettingBase setting) {
			// get prefab fitting the setting type.
			if (controlPrefabs.TryGetControl (setting.GetType (), out SettingControl ctrl)) {
				// spawn prefab, if it is spawnable for the current setting
				if (ctrl.TryInstantiateWith (setting, out SettingControl control)) {
					// setup instance
					control.transform.SetParent (parent);
					control.transform.localScale = Vector3.one;
					control.transform.localPosition = Vector3.zero;
				} else {
					Debug.Log ($"Can't spawn prefab for {setting.Name} ({setting.GetType ()})");
				}
			} else {
				Debug.Log ($"No prefab found for {setting.Name} ({setting.GetType ()}).");
			}
		}


		// Button methods

		public void ApplyDirtySettings () {
			asset.ApplyDirtySettings ();
		}

		public void RevertDirtySettings () {
			asset.RevertDirtySettings ();
		}

		public void ResetAllSettings () {
			asset.ResetAllSettings (true);
		}

		public void Save () {
			asset.SerializeSettings (serializer);
		}

	}
}