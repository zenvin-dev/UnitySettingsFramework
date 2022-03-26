using Zenvin.Settings.UI;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

namespace Zenvin.Settings.Samples {
	public class SliderControl : SettingControl<SliderSetting, float> {

		[SerializeField] private TextMeshProUGUI label;
		[SerializeField] private Slider slider;
		[SerializeField] private TextMeshProUGUI valueLabel;


		protected override void OnSetup () {
			slider.minValue = 0f;
			slider.maxValue = 1f;
			slider.SetValueWithoutNotify (Mathf.InverseLerp (Setting.MinValue, Setting.MaxValue, Setting.CurrentValue));
			label?.SetText (Setting.Name);
		}

		public void OnValueChange (float value) {
			if (Setting == null) {
				return;
			}

			value = Mathf.Lerp (Setting.MinValue, Setting.MaxValue, value);
			value = MathUtility.SnapValueToIncrement (value, Setting.Increment);

			Setting.SetValue (value);
			slider.SetValueWithoutNotify (Mathf.InverseLerp (Setting.MinValue, Setting.MaxValue, Setting.CachedValue));
			valueLabel?.SetText (value.ToString ("0.0"));
		}

	}
}