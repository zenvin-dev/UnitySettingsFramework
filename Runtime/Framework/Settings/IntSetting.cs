using System;

namespace Zenvin.Settings.Framework {
	public class IntSetting : SettingBase<int> {

		protected internal override int OnDeserialize (byte[] data) {
			return BitConverter.ToInt32 (data, 0);
		}

		protected internal override byte[] OnSerialize () {
			return BitConverter.GetBytes (CurrentValue);
		}

	}
}