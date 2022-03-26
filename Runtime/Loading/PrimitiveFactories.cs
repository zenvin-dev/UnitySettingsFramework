using Zenvin.Settings.Framework;
using UnityEngine;

namespace Zenvin.Settings.Loading {
	public class BoolSettingFactory : ISettingFactory {
		string ISettingFactory.GetValidType () {
			return "bool";
		}

		SettingBase ISettingFactory.CreateSettingFromType (string defaultValue, JsonKeyValuePair[] values) {
			return ScriptableObject.CreateInstance<BoolSetting> ();
		}
	}

	public class IntSettingFactory : ISettingFactory {
		string ISettingFactory.GetValidType () {
			return "int";
		}

		SettingBase ISettingFactory.CreateSettingFromType (string defaultValue, JsonKeyValuePair[] values) {
			return ScriptableObject.CreateInstance<IntSetting> ();
		}
	}

	public class FloatSettingFactory : ISettingFactory {
		string ISettingFactory.GetValidType () {
			return "float";
		}

		SettingBase ISettingFactory.CreateSettingFromType (string defaultValue, JsonKeyValuePair[] values) {
			return ScriptableObject.CreateInstance<FloatSetting> ();
		}
	}
}