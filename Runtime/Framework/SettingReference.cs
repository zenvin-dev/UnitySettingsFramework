using UnityEngine;
using System;

namespace Zenvin.Settings.Framework {
	/// <summary>
	/// Simple class, used to provide a reference to a <see cref="SettingBase{T}"/> and a fallback value in case the reference is <c>null</c>.
	/// </summary>
	[Serializable]
	public class SettingReference<T> {

		[SerializeField] private SettingBase<T> settingObj = null;
		[SerializeField] private T fallbackValue = default;


		public bool HasSetting => settingObj != null;
		public SettingBase<T> Setting {
			get => settingObj;
			set => settingObj = value;
		}

		public T Fallback => fallbackValue;
		public T CurrentValue => settingObj == null ? fallbackValue : settingObj.CurrentValue;
		public T CachedValue => settingObj == null ? fallbackValue : settingObj.CachedValue;

	}
}