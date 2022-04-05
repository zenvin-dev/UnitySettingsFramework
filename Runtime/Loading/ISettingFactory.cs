using Zenvin.Settings.Framework;

namespace Zenvin.Settings.Loading {
	public interface ISettingFactory {
		string GetDefaultValidType ();
		SettingBase CreateSettingFromType (string defaultValue, StringValuePair[] values);
	}
}