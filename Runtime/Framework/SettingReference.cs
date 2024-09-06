using UnityEngine;
using System;

namespace Zenvin.Settings.Framework {
	/// <summary>
	/// Simple class, used to provide a serialized reference to a <see cref="SettingBase{T}"/> and a fallback value in case the reference is <see langword="null"/>.
	/// </summary>
	[Serializable]
	public class SettingReference<T> {
		private SettingBase<T>.ValueChangedEvent valueChanged;
		
		[SerializeField] private SettingBase<T> settingObj = null;
		[SerializeField] private protected T fallbackValue = default;


		/// <summary>
		/// Relays the wrapped Setting's <see cref="SettingBase{T}.ValueChanged"/> event.<br></br>
		/// </summary>
		/// <remarks>
		/// Events cannot be added when the game is not running.
		/// </remarks>
		public event SettingBase<T>.ValueChangedEvent ValueChanged {
			add {
				if (!Application.isPlaying)
					return;

				if (settingObj != null) {
					settingObj.ValueChanged -= ReferenceValueChangedHandler;
					settingObj.ValueChanged += ReferenceValueChangedHandler;
				}
				valueChanged += value;
			}
			remove {
				valueChanged -= value;
			}
		}

		/// <summary> Whether a Setting is referenced. </summary>
		public bool HasSetting => settingObj != null;
		/// <summary> The referenced Setting object. </summary>
		public virtual SettingBase<T> Setting {
			get => settingObj;
			set => SetReference (value);
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


		private protected void SetReference (SettingBase<T> reference) {
			if (reference == settingObj)
				return;

			if (settingObj != null)
				settingObj.ValueChanged -= ReferenceValueChangedHandler;

			settingObj = reference;

			if (settingObj != null)
				settingObj.ValueChanged += ReferenceValueChangedHandler;
		}

		private void ReferenceValueChangedHandler (SettingBase.ValueChangeMode mode) => valueChanged?.Invoke (mode);
	}
}
