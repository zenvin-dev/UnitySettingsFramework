using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;

namespace Zenvin.Settings.Framework {
	public abstract class SettingBase : IdentifiableScriptableObject {

		[SerializeField, HideInInspector] private string settingName;
		[SerializeField, HideInInspector] private string settingNameLocKey;

		[SerializeField, HideInInspector] internal SettingsAsset asset;
		[SerializeField, HideInInspector] internal SettingsGroup group;


		public string Name {
			get => settingName;
			internal set {
				settingName = value;
				name = value;
			}
		}
		public string NameLocalizationKey {
			get => settingNameLocKey;
			internal set => settingNameLocKey = value;
		}


		protected internal abstract object DefaultValueRaw { get; }
		protected internal abstract Type ValueType { get; }


		internal void Setup (SettingsAsset asset) {
			this.asset = asset;
			OnSetup ();
		}

		private protected virtual void OnSetup () { }

		/// <summary>
		/// Applies the cached value to the setting.<br></br>
		/// Will return <c>false</c> if the values were equal already.
		/// </summary>
		public bool ApplyValue () => OnApply ();

		private protected abstract bool OnApply ();


		/// <summary>
		/// Reverts the cached value back to the current value.<br></br>
		/// Will return <c>false</c> if they were equal already.
		/// </summary>
		public bool RevertValue () => OnRevert ();

		private protected abstract bool OnRevert ();


		internal SettingData Serialize () => new SettingData () { GUID = GUID, Data = SerializeInternal () };

		private protected abstract byte[] SerializeInternal ();


		internal void Deserialize (byte[] data) => DeserializeInternal (data);

		private protected abstract void DeserializeInternal (byte[] data);

	}

	public abstract class SettingBase<T> : SettingBase where T : struct {

		public delegate void OnApplyValue (T previous, T current);

		public event OnApplyValue OnApplyValueHandler;

		private T cachedValue;
		private T currentValue;
		private bool isDirty;

		[SerializeField] private T defaultValue;


		/// <summary> The default value of this setting. </summary>
		public T DefaultValue => defaultValue;
		/// <summary> The currently applied value of this setting. </summary>
		public T CurrentValue => currentValue;
		/// <summary> The value this setting will have after the next call to <see cref="SettingBase.ApplyValue"/>. </summary>
		public T CachedValue => cachedValue;

		/// <summary> Whether the value of the setting has been changed but not applied yet. </summary>
		public bool IsDirty {
			get => isDirty;
			private set {
				if (isDirty == value) {
					return;
				}
				isDirty = value;
				asset.SetDirty (this, isDirty);
			}
		}

		protected internal sealed override object DefaultValueRaw => defaultValue;
		protected internal sealed override Type ValueType => typeof (T);


		/// <summary>
		/// Sets the setting's next value. Will be applied when <see cref="ApplyValue"/> is called.
		/// </summary>
		/// <param name="value"> The value to set. </param>
		public void SetValue (T value) {
			ProcessValue (ref value);

			if (!cachedValue.Equals (value)) {
				cachedValue = value;
				IsDirty = true;
			}
		}

		/// <summary>
		/// Called during <see cref="SetValue(T)"/> to process the change before it is cached.
		/// </summary>
		/// <param name="value"> The next value of the setting. </param>
		protected virtual void ProcessValue (ref T value) { }

		
		private protected sealed override bool OnApply () {
			if (!IsDirty) {
				return false;
			}
			T curr = currentValue;
			currentValue = cachedValue;
			OnAfterApplyValue ();
			OnApplyValueHandler?.Invoke (curr, currentValue);
			IsDirty = false;
			return true;
		}

		/// <summary>
		/// Called during <see cref="ApplyValue"/>, before the <see cref="OnApplyValueHandler"/> is invoked.
		/// </summary>
		protected virtual void OnAfterApplyValue () { }

		private protected sealed override bool OnRevert () {
			if (!IsDirty) {
				return false;
			}
			cachedValue = currentValue;
			IsDirty = false;
			return true;
		}


		private protected sealed override byte[] SerializeInternal () {
			return OnSerialize ();
		}

		protected internal abstract byte[] OnSerialize ();

		private protected sealed override void DeserializeInternal (byte[] data) {
			T value = OnDeserialize (data);
			SetValue (value);
			ApplyValue ();
		}

		protected internal abstract T OnDeserialize (byte[] data);

	}

	[Serializable]
	internal class SettingData {
		public string GUID { get; set; }
		public byte[] Data { get; set; }
	}
}