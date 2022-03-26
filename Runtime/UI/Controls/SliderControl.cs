using Zenvin.Settings.Framework;
using UnityEngine.UI;
using UnityEngine;

namespace Zenvin.Settings.UI {
	public class SliderControl : SettingControl<SliderSetting, float> {

		[SerializeField] private Slider slider;


		protected override void OnSetup () {
			slider.minValue = 0f;
			slider.maxValue = 1f;
		}

		public void OnValueChange (float value) {
			value = Mathf.Lerp (Setting.MinValue, Setting.MaxValue, value);
			value = MathUtility.SnapValueToIncrement (value, Setting.Increment);

			Setting.SetValue (value);
			slider.SetValueWithoutNotify (Setting.CachedValue);
		}

	}
}