using Zenvin.Settings.Framework;

namespace Zenvin.Settings.Loading {
	public interface ISettingFactory {
		string GetValidType ();
		SettingBase CreateSettingFromType (string defaultValue, StringValuePair[] values);
	}
}