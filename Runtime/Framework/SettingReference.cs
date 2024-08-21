using UnityEngine;
using System;

namespace Zenvin.Settings.Framework {
	/// <summary>
	/// Simple class, used to provide a serialized reference to a <see cref="SettingBase{T}"/> and a fallback value in case the reference is <see langword="null"/>.
	/// </summary>
	[Serializable]
	public class SettingReference<T> {

		[SerializeField] private protected SettingBase<T> settingObj = null;
		[SerializeField] private protected T fallbackValue = default;


		/// <summary>
		/// Exposes the wrapped Setting's <see cref="SettingBase{T}.ValueChanged"/> event.<br></br>
		/// (Un-)Subscribing while no <see cref="SettingBase{T}"/> is referenced will not have an effect.
		/// </summary>
		public event SettingBase<T>.ValueChangedEvent ValueChanged {
			add {
				if (HasSetting) settingObj.ValueChanged += value;
			}
			remove {
				if (HasSetting) settingObj.ValueChanged -= value;
			}
		}

		/// <summary> Whether a Setting is referenced. </summary>
		public bool HasSetting => settingObj != null;
		/// <summary> The referenced Setting object. </summary>
		public virtual SettingBase<T> Setting {
			get => settingObj;
			set => settingObj = value;
		}

		/// <summary> The Fallback value, that is used when no valid <see cref="SettingBase{T}"/> is referenced. </summary>
		public T Fallback { get => fallbackValue; set => fallbackValue = value; }
		/// <summary> The <see cref="SettingBase{T}.CurrentValue"/> of the referenced Setting. <see cref="Fallback"/> if no fallback is given. </summary>
		public T CurrentValue => settingObj == null ? fallbackValue : settingObj.CurrentValue;
		/// <summary> The <see cref="SettingBase{T}.CachedValue"/> of the referenced Setting. <see cref="Fallback"/> if no fallback is given. </summary>
		public T CachedValue => settingObj == null ? fallbackValue : settingObj.CachedValue;


		public SettingReference () { }

		public SettingReference (T fallbackValue) {
			this.fallbackValue = fallbackValue;
		}
	}
}
