using UnityEngine;

namespace Zenvin.Settings {
	public static class MathUtility {

		public static float Difference (float a, float b) {
			if (a == b) {
				return 0f;
			}

			if (a == 0f) {
				return Mathf.Abs (b);
			}
			if (b == 0f) {
				return Mathf.Abs (a);
			}

			if (a < 0f && b > 0f) {
				return -a + b;
			}
			if (b < 0f && a > 0f) {
				return -b + a;
			}

			return 0f;
		}

		public static float SnapValueToIncrement (float value, float increment) {
			float mod = value % increment;
			return value - Mathf.Abs (mod) + (increment < 0f && mod != 0f ? Mathf.Abs (increment) : 0f) + increment;
		}

	}
}