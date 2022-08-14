using System;

namespace Assets.UnitySettingsFramework.Runtime.Framework.Serialization {
	[Serializable]
	internal class SettingData {
		public string GUID { get; set; }
		public byte[] Data { get; set; }
	}
}
