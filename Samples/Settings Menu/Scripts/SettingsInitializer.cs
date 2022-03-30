using UnityEngine;

using Zenvin.Settings.Framework;
using Zenvin.Settings.Loading;

namespace Zenvin.Settings.Samples {
	[DisallowMultipleComponent]
	public class SettingsInitializer : MonoBehaviour {

		public enum InitMode {
			Awake,
			Start
		}

		[SerializeField] private InitMode mode = InitMode.Start;
		[SerializeField] private SettingsAsset settings;
		[Space, SerializeField] private bool loadDynamicSettings = true;
		[SerializeField] private SettingsImportData dynamicSettings;
		[SerializeField, TextArea (25, 35)] private string dynamicSettingsJson = "";

		private void Awake () {
			if (mode == InitMode.Awake) {
				Init ();
			}
		}

		private void Start () {
			if (mode == InitMode.Start) {
				Init ();
			}
		}


		private void Init () {
			if (loadDynamicSettings) {
				SettingsAsset.OnInitialize += LoadSettings;
			}
			if (settings != null) {
				settings.Initialize ();
			}
		}

		private void LoadSettings (SettingsAsset asset) {
			if (dynamicSettingsJson == null || dynamicSettingsJson.Length == 0) {
				return;
			}
			RuntimeSettingLoader.LoadSettingsIntoAsset (
				asset, dynamicSettingsJson, null,
				new BoolSettingFactory (), new IntSettingFactory (), new FloatSettingFactory (), new DropdownSettingFactory (), new SliderSettingFactory ()
			);
		}

		private void OnValidate () {
			dynamicSettingsJson = JsonUtility.ToJson (dynamicSettings, true);
		}

	}
}