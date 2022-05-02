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
		[SerializeField, TextArea (25, 35), Space (20)] private string dynamicSettingsJsonOutput = "";


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
			if (dynamicSettingsJsonOutput == null || dynamicSettingsJsonOutput.Length == 0) {
				return;
			}
			RuntimeSettingLoader.LoadSettingsIntoAsset (
				asset, dynamicSettingsJsonOutput, null,
				("bool", new BoolSettingFactory ()),
				("int", new IntSettingFactory ()),
				("float", new FloatSettingFactory ()),
				("dropdown", new DropdownSettingFactory ()),
				("slider", new SliderSettingFactory ())
			);
		}

		private void OnValidate () {
			dynamicSettingsJsonOutput = JsonUtility.ToJson (dynamicSettings, true);
		}

	}
}