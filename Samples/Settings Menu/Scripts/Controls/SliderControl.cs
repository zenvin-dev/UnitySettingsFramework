using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Zenvin.Settings.Framework;
using Zenvin.Settings.UI;

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
		
		protected override void OnSettingValueChanged (SettingBase.ValueChangeMode mode) {
			UpdateSlider ();
		}

		protected override void OnVisibilityChanged () {
			SettingVisibility vis = Setting.GetVisibilityInHierarchy ();
			slider.interactable = vis == SettingVisibility.Visible;
			gameObject.SetActive (vis != SettingVisibility.Hidden);
		}


		// TODO: Redo so increment snapping actually works RELIABLY.
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