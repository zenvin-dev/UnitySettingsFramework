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


		public SettingReference () { }

		public SettingReference (T fallbackValue) {
			this.fallbackValue = fallbackValue;
		}


		/// <summary> Whether a Setting is referenced. </summary>
		public bool HasSetting => settingObj != null;
		/// <summary> The referenced Setting object. </summary>
		public SettingBase<T> Setting {
			get => settingObj;
			set => settingObj = value;
		}

		/// <summary> The Fallback value, that is used when no valid <see cref="SettingBase{T}"/> is referenced. </summary>
		public T Fallback { get => fallbackValue; set => fallbackValue = value; }
		/// <summary> The <see cref="SettingBase{T}.CurrentValue"/> of the referenced Setting. <see cref="Fallback"/> if none. </summary>
		public T CurrentValue => settingObj == null ? fallbackValue : settingObj.CurrentValue;
		/// <summary> The <see cref="SettingBase{T}.CachedValue"/> of the referenced Setting. <see cref="Fallback"/> if none. </summary>
		public T CachedValue => settingObj == null ? fallbackValue : settingObj.CachedValue;

	}
}