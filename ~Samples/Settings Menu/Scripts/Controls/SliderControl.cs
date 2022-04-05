using Zenvin.Settings.Utility;
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
			if (Setting.Increment == 0f) {
				slider.minValue = Setting.MinValue;
				slider.maxValue = Setting.MaxValue;
				slider.wholeNumbers = false;

				slider.SetValueWithoutNotify (Setting.CurrentValue);
			} else {
				slider.minValue = Setting.MinValue;
				slider.maxValue = (Setting.MaxValue - Setting.MinValue + Setting.Increment) / Setting.Increment + Setting.MinValue;
				slider.wholeNumbers = true;

				slider.SetValueWithoutNotify (Setting.CurrentValue * Setting.Increment);
			}

			label?.SetText (Setting.Name);
			UpdateValueLabel ();
		}

		protected override void OnSettingReset () {
			UpdateSlider ();
		}

		protected override void OnSettingReverted () {
			UpdateSlider ();
		}


		public void OnValueChange (float value) {
			if (Setting == null) {
				return;
			}

			//value = Mathf.Lerp (Setting.MinValue, Setting.MaxValue, value);
			//value = MathUtility.SnapValueToIncrement (value, Setting.Increment);

			//Setting.SetValue (value * Setting.Increment);

			if (Setting.Increment == 0f) {
				Setting.SetValue (value);
			} else {
				Setting.SetValue (value * Setting.Increment);
			}

			UpdateSlider ();
		}

		private void UpdateSlider () {
			if (slider != null) {
				//slider.SetValueWithoutNotify (Mathf.InverseLerp (Setting.MinValue, Setting.MaxValue, Setting.CachedValue));
				slider.SetValueWithoutNotify (Setting.CachedValue / (Setting.Increment > 0 ? Setting.Increment : 1f));
			}
			UpdateValueLabel ();
		}

		private void UpdateValueLabel () {
			if (valueLabel != null) {
				valueLabel.SetText ((Setting.CachedValue/* * Setting.Increment*/).ToString ("0.0"));
			}
		}

	}
}