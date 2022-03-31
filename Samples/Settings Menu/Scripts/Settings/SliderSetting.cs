using Zenvin.Settings.Framework;
using UnityEngine;

using Zenvin.Settings.Utility;

namespace Zenvin.Settings.Samples {
	public class SliderSetting : FloatSetting {

		// slider settings
		[SerializeField] private float minValue = 0f;
		[SerializeField] private float maxValue = 1f;
		[SerializeField, Min (0f)] private float increment = 0.5f;

		public float MinValue => minValue;
		public float MaxValue => maxValue;
		public float Increment => increment;


		// make sure the value snaps to an increment
		protected override void ProcessValue (ref float value) {
			value = Mathf.Clamp (value, minValue, maxValue);
			//if (increment > 0f) {
			//	value = MathUtility.SnapValueToIncrement (value, increment);
			//}
		}

		// try assigning slider settings
		protected override void OnCreateWithValues (StringValuePair[] values) {

			float? min = null;
			float? max = null;
			float? inc = null;

			foreach (var value in values) {

				if (min == null && value.Key.Equals ("minValue", System.StringComparison.OrdinalIgnoreCase)) {
					float.TryParse (value.Value, out float _min);
					min = _min;
					continue;
				}

				if (value.Key.Equals ("maxValue", System.StringComparison.OrdinalIgnoreCase)) {
					float.TryParse (value.Value, out float _max);
					max = _max;
					continue;
				}

				if (value.Key.Equals ("increment", System.StringComparison.OrdinalIgnoreCase)) {
					float.TryParse (value.Value, out float _inc);
					inc = Mathf.Max (0f, _inc);
					continue;
				}

			}

			min = min ?? 0f;
			max = max ?? 1f;
			inc = inc ?? 0f;

			minValue = min.Value;
			maxValue = max.Value;
			increment = inc.Value;

			MathUtility.AssignMinMax (ref minValue, ref maxValue);

		}

	}
}