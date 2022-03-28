using System;

namespace Zenvin.Settings.Framework {
	public class IntSetting : SettingBase<int> {

		protected override int OnDeserialize (byte[] data) {
			return BitConverter.ToInt32 (data, 0);
		}

		protected override byte[] OnSerialize () {
			return BitConverter.GetBytes (CurrentValue);
		}

	}
}