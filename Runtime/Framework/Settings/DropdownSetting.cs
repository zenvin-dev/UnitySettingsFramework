using UnityEngine;

namespace Zenvin.Settings.Framework {
	public class DropdownSetting : IntSetting {

		[SerializeField] private string[] options;


		protected override void ProcessValue (ref int value) {
			value = Mathf.Clamp (value, 0, options.Length - 1);
		}

	}
}