using UnityEngine;
using Zenvin.Settings.Framework;

namespace Zenvin.Settings.Samples {
	public class VolumeSetting : FloatSetting {

		public const float LogMinValue = -80f;
		public const float LogMaxValue = 20f;
		public const float LinMinValue = 0.0001f;
		public const float LinMaxValue = 1f;


		[SerializeField] private MixerVariable target = new MixerVariable ();
		[SerializeField, Tooltip ("Whether to apply the Setting's value to the AudioMixer any time the value is changed, or wait for the Setting's value to be applied.")]
		private bool changeOnSet = true;


		public float TargetValue => changeOnSet ? CachedValue : CurrentValue;


		/// <inheritdoc/>
		protected override void ProcessValue (ref float value) {
			value = Mathf.Clamp01 (value);
		}

		/// <inheritdoc/>
		protected override void OnValueChanged (ValueChangeMode mode) {
			if (changeOnSet || mode == ValueChangeMode.Apply || mode == ValueChangeMode.Initialize || mode == ValueChangeMode.Deserialize) {
				var value = ValueToVolume(TargetValue);
				target.SetValue (value);
			}
		}


		private static float VolumeToValue (float dB) {
			// a = 10 ^ (b / 20)
			// where { -80 <= b <= 20 }
			dB = Mathf.Clamp (dB, LogMinValue, LogMaxValue);
			return Mathf.Pow (10f, dB / 20f);
		}

		private static float ValueToVolume (float value) {
			var rangeValue = Mathf.Lerp (LinMinValue, LinMaxValue, value);
			// b = log10(a) * 20
			// where { 0.0001 <= a <= 1 }
			var volume = Mathf.Log10 (rangeValue) * 20f;
			return volume;
		}
	}
}