using System.Collections.Generic;
using UnityEngine;
using TMPro;

using Zenvin.Settings.Samples;
using Zenvin.Settings.UI;
using Zenvin.Settings.Framework;

namespace Zenvin {
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

	}
}