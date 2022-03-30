using Zenvin.Settings.Framework;
using Zenvin.Settings.Loading;

namespace Zenvin.Settings.Samples {
	public class SliderSettingFactory : ISettingFactory {
		string ISettingFactory.GetValidType () {
			return "slider";
		}

		SettingBase ISettingFactory.CreateSettingFromType (string defaultValue, StringValuePair[] values) {
			return SliderSetting.CreateInstanceWithValues<SliderSetting> (float.TryParse (defaultValue, out float val) ? val : 0, values);
		}
	}
}