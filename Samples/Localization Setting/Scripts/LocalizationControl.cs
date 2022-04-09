using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using System.Collections.Generic;
using UnityEngine.Localization;
using Zenvin.Settings.UI;
using UnityEngine;
using TMPro;
using Zenvin.Settings.Framework;

namespace Zenvin.Settings.Samples {
	public class LocalizationControl : SettingControl<LocalizationSetting, int> {

		[SerializeField] private TextMeshProUGUI label;
		[SerializeField] private TMP_Dropdown dropdown;
		[SerializeField] private TableReference localizationTable;


		protected override void OnSetup () {
			LocalizationSettings.SelectedLocaleChanged += UpdateLabel;
			UpdateLabel (LocalizationSettings.SelectedLocale);

			if (dropdown != null) {
				var options = new List<TMP_Dropdown.OptionData> ();
				int current = 0;

				for (int i = 0; i < Setting.LocaleCount; i++) {
					var locale = Setting[i];
					options.Add (new TMP_Dropdown.OptionData (locale.LocaleName));
					if (locale == LocalizationSettings.SelectedLocale) {
						current = i;
					}
				}

				dropdown.ClearOptions ();
				dropdown.AddOptions (options);
				dropdown.SetValueWithoutNotify (current);
				Setting.SetValue (current);
				Setting.ApplyValue ();
			}
		}

		private void OnDestroy () {
			LocalizationSettings.SelectedLocaleChanged -= UpdateLabel;
		}

		private void UpdateLabel (Locale locale) {
			if (label != null) {
				label.text = LocalizationSettings.StringDatabase.GetLocalizedString (localizationTable, Setting.NameLocalizationKey, locale);
			}
		}

		//protected override void OnSettingReverted () {
		//	if (dropdown != null) {
		//		dropdown.SetValueWithoutNotify (Setting.CurrentValue);
		//	}
		//}

		//protected override void OnSettingReset () {
		//	if (dropdown != null) {
		//		dropdown.SetValueWithoutNotify (Setting.CurrentValue);
		//	}
		//}

		protected override void OnSettingValueChanged (SettingBase.ValueChangeMode mode) {
			if (dropdown != null) {
				dropdown.SetValueWithoutNotify (Setting.CachedValue);
			}
		}

	}
}