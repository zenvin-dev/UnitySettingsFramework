using System.Text;

namespace Zenvin.Settings.Framework {
	public class StringSetting : SettingBase<string> {

		protected override string OnDeserialize (byte[] data) {
			return Encoding.ASCII.GetString (data);
		}

		protected override byte[] OnSerialize () {
			return Encoding.ASCII.GetBytes (CurrentValue);
		}

	}
}