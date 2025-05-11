using System;
using System.Collections.Generic;
using Zenvin.Settings.Framework;

namespace Zenvin.Settings.Loading {
	[Serializable]
	public class SettingsImportData {
		public List<GroupData> Groups;
		public List<SettingData> Settings;
		public List<OverrideData> DefaultOverrides;
	}

	[Serializable]
	public abstract class ObjectDataBase {
		public string GUID;
		public string Type;

		public string Name;
		public string NameLocalizationKey;

		public string Description;
		public string DescriptionLocalizationKey;

		public string ParentGroupGUID;

		public SettingVisibility InitialVisibility;

		public List<StringValuePair> Values;
		public List<ComponentData> Components;
	}

	[Serializable]
	public class GroupData : ObjectDataBase {
		public string IconResource;
	}

	[Serializable]
	public class SettingData : ObjectDataBase {
		public int OrderInGroup;
		public string DefaultValue;
	}

	[Serializable]
	public class OverrideData {
		public string GUID;
		public UpdateValueMode Update;
		public List<StringValuePair> Values;
	}

	[Serializable]
	public class ComponentData {
		public string Type;
		public List<StringValuePair> Values;
	}
}
