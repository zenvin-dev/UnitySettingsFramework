using System;

namespace Zenvin.Settings.Framework {
	public class FloatSetting : SettingBase<float> {

		protected internal override float OnDeserialize (byte[] data) {
			return BitConverter.ToSingle (data, 0);
		}

		protected override byte[] OnSerialize () {
			return BitConverter.GetBytes (CurrentValue);
		}

	}
}