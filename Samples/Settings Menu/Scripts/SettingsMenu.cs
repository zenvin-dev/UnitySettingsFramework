using System.Collections.Generic;
using Zenvin.Settings.Framework;
using Zenvin.Settings.UI;
using UnityEngine;
using System;

namespace Zenvin.Settings.Samples {
	public class SettingsMenu : MonoBehaviour {

		private readonly Dictionary<Type, int> controlPrefabLookup = new Dictionary<Type, int> ();

		[SerializeField] private SettingsAsset asset;
		[SerializeField] private RectTransform settingHeaderPrefab;
		[SerializeField] private SettingControl[] controlPrefabs;
		[SerializeField] private TabView tabView;


		private void Start () {
			if (asset == null) {
				return;
			}
			InitLookup ();
			SpawnMenu ();
		}
		private void InitLookup () {
			for (int i = 0; i < controlPrefabs.Length; i++) {
				if (controlPrefabs[i] != null) {
					controlPrefabLookup[controlPrefabs[i].ControlType] = i;
				}
			}
		}

		private void SpawnMenu () {

			// get settings groups for tabs
			var tabGroups = asset.GetGroups ();

			foreach (var tg in tabGroups) {

				// spawn tab
				RectTransform tabTransform = tabView.AddTab (tg);

				// populate tab
				var tabSettings = tg.GetSettings ();
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
					var settings = cg.GetAllSettings ();
					foreach (var setting in settings) {
						SpawnPrefab (tabTransform, setting);
					}

				}

			}

		}

		private void SpawnPrefab (RectTransform parent, SettingBase setting) {
			if (controlPrefabLookup.TryGetValue (setting.GetType (), out int index) && controlPrefabs[index].TryInstantiateWith (setting, out SettingControl control)) {
				control.transform.SetParent (parent);
				control.transform.localScale = Vector3.one;
			} else {
				Debug.Log ($"Can't spawn prefab for {setting.Name} ({setting.GetType ()})");
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

	}
}