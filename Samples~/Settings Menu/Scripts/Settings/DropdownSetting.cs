using UnityEngine;

using Zenvin.Settings.Framework;

namespace Zenvin.Settings.Samples {
	public class DropdownSetting : IntSetting {

		[SerializeField] private string[] options;

		public string[] Options => options;


		protected override void ProcessValue (ref int value) {
			value = Mathf.Clamp (value, 0, options.Length - 1);
		}

		protected override void OnCreateWithValues (StringValuePair[] values) {
			options = new string[values.Length];
			for (int i = 0; i < values.Length; i++) {
				options[i] = values[i].Value;
			}
		}

	}
}