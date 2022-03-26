using Zenvin.Settings.Framework;
using UnityEngine;

namespace Zenvin.Settings.Samples {
	public class DropdownSetting : IntSetting {

		[SerializeField] private string[] options;

		public string[] Options => options;

		protected override void ProcessValue (ref int value) {
			value = Mathf.Clamp (value, 0, options.Length - 1);
		}

	}
}