using System;

namespace Zenvin.Settings.Loading {
	[Serializable]
	public class SettingsImportData {
		public GroupData[] Groups;
		public SettingData[] Settings;
	}

	[Serializable]
	public abstract class ObjectDataBase {
		public string GUID;
		public string Name;
		public string LocalizationKey;
		public string ParentGroupGUID;
		public int OrderInGroup;
	}

	[Serializable]
	public class GroupData : ObjectDataBase {
		//public string GUID;
		//public string ParentGroupGUID;
		//public int OrderInGroup;

		//public string Name;
		//public string LocalizationKey;

		public string IconResource;
	}

	[Serializable]
	public class SettingData : ObjectDataBase {
		//public string GUID;
		public string Type;

		//public string Name;
		//public string LocalizationKey;

		//public string ParentGroupGUID;
		//public int OrderInGroup;

		public string DefaultValue;
		public StringValuePair[] Values;
	}
}