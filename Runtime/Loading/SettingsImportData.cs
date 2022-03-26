using System;

namespace Zenvin.Settings.Loading {
	[Serializable]
	public class SettingsImportData {
		public GroupData[] Groups;
		public SettingData[] Settings;
	}

	[Serializable]
	public class GroupData {
		public string GUID;
		public string ParentGroupGUID;
		public string Name;
		public string LocalizationKey;
		public string IconResource;
	}

	[Serializable]
	public class SettingData {
		public string GUID;
		public string Type;

		public string Name;
		public string LocalizationKey;

		public string ParentGroupGUID;

		public string DefaultValue;
		public JsonKeyValuePair[] Values;
	}

	[Serializable]
	public class JsonKeyValuePair {
		public string Key;
		public string Value;
	}
}