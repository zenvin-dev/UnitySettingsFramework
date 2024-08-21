using System.Collections.Generic;
using UnityEngine;
using TMPro;

using Zenvin.Settings.Framework;
using Zenvin.Settings.UI;

namespace Zenvin.Settings.Samples {
	[AddComponentMenu("Zenvin/Settings/UI/Dropdown Control")]
	public class DropdownControl : SettingControl<DropdownSetting, int> {

		[SerializeField] private TextMeshProUGUI label;
		[SerializeField] private TMP_Dropdown dropdown;


		protected override void OnSetup () {
			dropdown.ClearOptions ();
			dropdown.AddOptions (new List<string> (Setting.Options));
			dropdown.SetValueWithoutNotify (Setting.CurrentValue);
			label?.SetText (Setting.Name);
		}

		protected override void OnSettingValueChanged (SettingBase.ValueChangeMode mode) {
			dropdown?.SetValueWithoutNotify (Setting.CachedValue);
		}

		protected override void OnVisibilityChanged () {
			SettingVisibility vis = Setting.GetVisibilityInHierarchy ();
			dropdown.interactable = vis == SettingVisibility.Visible;
			gameObject.SetActive (vis != SettingVisibility.Hidden);
		}

	}
}