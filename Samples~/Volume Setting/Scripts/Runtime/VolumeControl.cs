using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenvin.Settings.Framework;
using Zenvin.Settings.UI;

namespace Zenvin.Settings.Samples {
	[AddComponentMenu("Zenvin/Settings/UI/Volume Control")]
	public class VolumeControl : SettingControl<VolumeSetting, float> {

		[SerializeField] private TextMeshProUGUI label;
		[SerializeField] private Slider slider;
		[SerializeField] private TextMeshProUGUI valueLabel;


		protected override void OnSetup () {
			if (slider != null) {
				slider.minValue = 0f;
				slider.maxValue = 1f;
				slider.wholeNumbers = false;
				UpdateSlider (Setting.TargetValue);
			}

			if (label != null) {
				label.SetText (Setting.Name);
			}
			UpdateValueLabel (Setting.CachedValue * 100f);
		}

		protected override void OnSettingValueChanged (SettingBase.ValueChangeMode mode) {
			if (mode != SettingBase.ValueChangeMode.Set) {
				UpdateSlider (Setting.TargetValue);
			}
			UpdateValueLabel (Setting.CachedValue * 100f);
		}


		private void UpdateSlider (float sliderValue) {
			if (slider != null) {
				slider.SetValueWithoutNotify (sliderValue);
			}
		}

		private void UpdateValueLabel (float sliderValue) {
			if (valueLabel != null) {
				valueLabel.SetText (sliderValue.ToString ("000"));
			}
		}
	}
}
