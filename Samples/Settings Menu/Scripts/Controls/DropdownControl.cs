using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Zenvin.Settings.Samples;
using Zenvin.Settings.UI;

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

	}
}