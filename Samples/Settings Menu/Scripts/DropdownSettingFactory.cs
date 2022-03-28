using Zenvin.Settings.Framework;
using Zenvin.Settings.Loading;

namespace Zenvin.Settings.Samples {
	public class DropdownSettingFactory : ISettingFactory {
		string ISettingFactory.GetValidType () {
			return "dropdown";
		}

		SettingBase ISettingFactory.CreateSettingFromType (string defaultValue, StringValuePair[] values) {
			return DropdownSetting.CreateInstanceWithValues<DropdownSetting> (int.TryParse (defaultValue, out int val) ? val : 0, values);
		}
	}
}