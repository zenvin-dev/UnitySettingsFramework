using System;

namespace Zenvin.Settings.Framework {
	public class BoolSetting : SettingBase<bool> {

		protected override bool OnDeserialize (byte[] data) {
			return BitConverter.ToBoolean (data, 0);
		}

		protected override byte[] OnSerialize () {
			return BitConverter.GetBytes (CurrentValue);
		}

	}
}