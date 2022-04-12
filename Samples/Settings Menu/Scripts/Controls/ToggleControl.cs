using Zenvin.Settings.Framework;
using Zenvin.Settings.UI;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

namespace Zenvin.Settings.Samples {
	public class ToggleControl : SettingControl<BoolSetting, bool> {

		[SerializeField] private TextMeshProUGUI label;
		[SerializeField] private Toggle toggle;


		protected override void OnSetup () {
			label?.SetText (Setting.Name);
			toggle.SetIsOnWithoutNotify (Setting.CurrentValue);
		}

		protected override void OnSettingValueChanged (SettingBase.ValueChangeMode mode) {
			toggle?.SetIsOnWithoutNotify (Setting.CachedValue);
		}

	}
}