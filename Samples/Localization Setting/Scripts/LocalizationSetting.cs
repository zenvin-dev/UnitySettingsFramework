using UnityEngine.Localization.Settings;
using System.Collections.Generic;
using Zenvin.Settings.Framework;
using UnityEngine.Localization;
using UnityEngine;
using System;

namespace Zenvin.Settings.Samples {
	public class LocalizationSetting : SettingBase<int> {

		private List<Locale> locales;


		public int LocaleCount => locales?.Count ?? 0;
		public Locale this[int index] => locales == null || index < 0 || index >= locales.Count ? null : locales[index];


		protected override void ProcessValue (ref int value) {
			value = Mathf.Clamp (value, 0, LocaleCount - 1);
		}

		// On initialization
		protected override void OnInitialize () {
			// update locales list
			locales = LocalizationSettings.AvailableLocales.Locales;
		}

		// After appying the Setting
		protected override void OnValueChanged (ValueChangeMode mode) {
			// if the value is not being applied, cancel
			if (mode != ValueChangeMode.Apply) {
				return;
			}

			// get locale that the setting wants to be selected
			var loc = this[CurrentValue];

			if (loc != null) {
				// update selected locale
				LocalizationSettings.SelectedLocale = loc;
			}
		}


		protected override byte[] OnSerialize () {
			return BitConverter.GetBytes (CurrentValue);
		}

		protected override int OnDeserialize (byte[] data) {
			return BitConverter.ToInt32 (data, 0);
		}

	}
}