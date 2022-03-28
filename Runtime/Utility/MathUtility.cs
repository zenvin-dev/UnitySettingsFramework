using UnityEngine;

namespace Zenvin.Settings.Utility {
	public static class MathUtility {

		/// <summary>
		/// Snaps a given value to an increment.
		/// </summary>
		public static float SnapValueToIncrement (float value, float increment) {
			float mod = value % increment;
			return value - Mathf.Abs (mod) + (increment < 0f && mod != 0f ? Mathf.Abs (increment) : 0f) + increment;
		}

		/// <summary>
		/// Makes sure that <paramref name="min"/> is the smaller value and <paramref name="max"/> is the larger value.
		/// </summary>
		public static void AssignMinMax (ref float min, ref float max) {
			if (min <= max) {
				return;
			}
			float cache = max;
			max = min;
			min = cache;
		}

	}
}