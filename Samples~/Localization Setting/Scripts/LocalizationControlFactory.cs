using Zenvin.Settings.Framework;
using Zenvin.Settings.Loading;

namespace Zenvin.Settings.Samples {
	public class LocalizationControlFactory : ISettingFactory {

		string ISettingFactory.GetDefaultValidType () {
			return "language";
		}

		SettingBase ISettingFactory.CreateSettingFromType (string defaultValue, StringValuePair[] values) {
			return LocalizationSetting.CreateInstanceWithValues<LocalizationSetting> (int.TryParse (defaultValue, out int val) ? val : 0, values);
		}

	}
}