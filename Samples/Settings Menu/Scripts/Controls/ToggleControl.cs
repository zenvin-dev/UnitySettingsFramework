using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Zenvin.Settings.Framework;
using Zenvin.Settings.UI;

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

		protected override void OnVisibilityChanged () {
			SettingVisibility vis = Setting.GetVisibilityInHierarchy ();
			toggle.interactable = vis == SettingVisibility.Visible;
			gameObject.SetActive (vis != SettingVisibility.Hidden);
		}

	}
}