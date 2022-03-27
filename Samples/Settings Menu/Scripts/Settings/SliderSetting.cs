using Zenvin.Settings.Framework;
using UnityEngine;
using Zenvin.Settings.Utility;

namespace Zenvin.Settings.Samples {
	public class SliderSetting : FloatSetting {

		[SerializeField] private float minValue = 0f;
		[SerializeField] private float maxValue = 1f;
		[SerializeField, Min (0f)] private float increment = 0.5f;

		public float MinValue => minValue;
		public float MaxValue => maxValue;
		public float Increment => increment;


		protected override void ProcessValue (ref float value) {
			value = Mathf.Clamp (value, minValue, maxValue);
			if (increment > 0f) {
				value = MathUtility.SnapValueToIncrement (value, increment);
			}
		}

	}
}