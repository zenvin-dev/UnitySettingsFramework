using System.Text;

namespace Zenvin.Settings.Framework {
	public class StringSetting : SettingBase<string> {

		private Encoding StringEncoding => Encoding.UTF8;


		protected override string OnDeserialize (byte[] data) {
			return StringEncoding.GetString (data);
		}

		protected override byte[] OnSerialize () {
			return StringEncoding.GetBytes (CurrentValue);
		}

	}
}