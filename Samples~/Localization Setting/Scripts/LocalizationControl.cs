using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using System.Collections.Generic;
using Zenvin.Settings.Framework;
using UnityEngine.Localization;
using UnityEngine;
using TMPro;

namespace Zenvin.Settings.Samples {
	[AddComponentMenu("Zenvin/Settings/UI/Localization Control")]
	public class LocalizationControl : LocalizedControlBase<LocalizationSetting, int> {

		[SerializeField] private TextMeshProUGUI label;
		[SerializeField] private TMP_Dropdown dropdown;
		[SerializeField] private TableReference localizationTable;


		protected override void OnLocalizedSetup () {
			var options = new List<TMP_Dropdown.OptionData> ();
			int current = 0;

			// generate dropdown info and get selected locale index
			for (int i = 0; i < Setting.LocaleCount; i++) {
				var locale = Setting[i];
				options.Add (new TMP_Dropdown.OptionData (locale.LocaleName));
				if (locale == LocalizationSettings.SelectedLocale) {
					current = i;
				}
			}

			// set up dropdown
			if (dropdown != null) {
				dropdown.ClearOptions ();
				dropdown.AddOptions (options);
			}

			// update setting to be selected locale
			Setting.SetValue (current);
			Setting.ApplyValue ();
		}

		protected override void OnLocalizationChanged (Locale locale) {
			if (label != null) {
				if (string.IsNullOrWhiteSpace (Setting.NameLocalizationKey)) {
					label.text = Setting.Name;
				} else {
					label.text = LocalizationSettings.StringDatabase.GetLocalizedString (localizationTable, Setting.NameLocalizationKey, locale);
				}
			}
		}

		protected override void OnSettingValueChanged (SettingBase.ValueChangeMode mode) {
			if (dropdown != null) {
				dropdown.SetValueWithoutNotify (Setting.CachedValue);
			}
		}
	}
}
