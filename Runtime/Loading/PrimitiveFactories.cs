using Zenvin.Settings.Framework;

namespace Zenvin.Settings.Loading {
	public class BoolSettingFactory : ISettingFactory {
		string ISettingFactory.GetDefaultValidType () {
			return "bool";
		}

		SettingBase ISettingFactory.CreateSettingFromType (string defaultValue, StringValuePair[] values) {
			return BoolSetting.CreateInstanceWithValues<BoolSetting> (bool.Parse (defaultValue), values);
		}
	}

	public class IntSettingFactory : ISettingFactory {
		string ISettingFactory.GetDefaultValidType () {
			return "int";
		}

		SettingBase ISettingFactory.CreateSettingFromType (string defaultValue, StringValuePair[] values) {
			return IntSetting.CreateInstanceWithValues<IntSetting> (int.TryParse (defaultValue, out int val) ? val : 0, values);
		}
	}

	public class FloatSettingFactory : ISettingFactory {
		string ISettingFactory.GetDefaultValidType () {
			return "float";
		}

		SettingBase ISettingFactory.CreateSettingFromType (string defaultValue, StringValuePair[] values) {
			return FloatSetting.CreateInstanceWithValues<FloatSetting> (float.TryParse (defaultValue, out float val) ? val : 0f, values);
		}
	}
}