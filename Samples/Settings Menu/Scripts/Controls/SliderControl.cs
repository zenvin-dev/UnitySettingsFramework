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
			slider.minValue = 0f;
			slider.maxValue = 1f;
			slider.SetValueWithoutNotify (Mathf.InverseLerp (Setting.MinValue, Setting.MaxValue, Setting.CurrentValue));

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

			value = Mathf.Lerp (Setting.MinValue, Setting.MaxValue, value);
			value = MathUtility.SnapValueToIncrement (value, Setting.Increment);

			Setting.SetValue (value);
			UpdateSlider ();
		}

		private void UpdateSlider () {
			if (slider != null) {
				slider.SetValueWithoutNotify (Mathf.InverseLerp (Setting.MinValue, Setting.MaxValue, Setting.CachedValue));
			}
			UpdateValueLabel ();
		}

		private void UpdateValueLabel () {
			if (valueLabel != null) {
				valueLabel.SetText (Setting.CachedValue.ToString ("0.0"));
			}
		}

	}
}