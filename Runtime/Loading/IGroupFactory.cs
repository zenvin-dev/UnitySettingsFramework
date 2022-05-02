using Zenvin.Settings.Framework;

namespace Zenvin.Settings.Loading {
	public interface IGroupFactory {
		string GetDefaultValidType ();
		SettingsGroup CreateGroupFromType (StringValuePair[] values);
	}
}