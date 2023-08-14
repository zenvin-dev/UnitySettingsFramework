using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using Zenvin.Settings.Framework;
using Zenvin.Settings.Framework.Serialization;

namespace Zenvin.Settings.Samples {
	public class LocalizationSetting : SettingBase<int>, ISerializable<JObject>, ISerializable<ValuePacket> {

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


		void ISerializable<JObject>.OnDeserialize (JObject value) {
			if (value.TryGetValue ("value", out JToken token)) {
				SetValue ((int)token);
				ApplyValue ();
			}
		}

		void ISerializable<ValuePacket>.OnDeserialize (ValuePacket value) {
			if (value.TryRead ("value", out int val)) {
				SetValue (val);
				ApplyValue ();
			}
		}

		void ISerializable<JObject>.OnSerialize (JObject value) {
			value.Add ("value", CurrentValue);
		}

		void ISerializable<ValuePacket>.OnSerialize (ValuePacket value) {
			value.Write ("value", CurrentValue);
		}

	}
}