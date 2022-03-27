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
		protected internal abstract object CurrentValueRaw { get; }
		protected internal abstract object CachedValueRaw { get; }
		protected internal abstract Type ValueType { get; }
		public abstract bool IsDirty { get; private protected set; }


		internal void Setup (SettingsAsset asset) {
			this.asset = asset;
			OnSetup ();
		}

		private protected virtual void OnSetup () { }

		internal virtual void Initialize () { }

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

		/// <summary>
		/// Resets the setting to its default value.
		/// </summary>
		/// <param name="apply"> Whether to instantly apply the value. Otherwise the setting will be dirtied. </param>
		public bool ResetValue (bool apply) => OnReset (apply);

		private protected abstract bool OnReset (bool apply);


		internal bool TrySerialize (out SettingData data) {
			byte[] valueData = SerializeInternal ();
			data = valueData == null ? null : new SettingData () { GUID = GUID, Data = valueData };
			return data != null;
		}

		private protected abstract byte[] SerializeInternal ();


		internal void Deserialize (byte[] data) => DeserializeInternal (data);

		private protected abstract void DeserializeInternal (byte[] data);

	}

	public abstract class SettingBase<T> : SettingBase where T : struct {

		public delegate void OnApplyValue (T previous, T current);

		public event OnApplyValue OnApplyValueHandler;

		[NonSerialized] private T cachedValue;
		[NonSerialized] private T currentValue;
		[NonSerialized] private bool isDirty;

		[SerializeField] private T defaultValue;


		/// <summary> The default value of this setting. </summary>
		public T DefaultValue => defaultValue;
		/// <summary> The currently applied value of this setting. </summary>
		public T CurrentValue => currentValue;
		/// <summary> The value this setting will have after the next call to <see cref="SettingBase.ApplyValue"/>. </summary>
		public T CachedValue => cachedValue;

		/// <summary> Whether the value of the setting has been changed but not applied yet. </summary>
		public sealed override bool IsDirty {
			get => isDirty;
			private protected set {
				if (isDirty == value) {
					return;
				}
				isDirty = value;
				asset.SetDirty (this, isDirty);
			}
		}

		protected internal sealed override object DefaultValueRaw => defaultValue;
		protected internal sealed override object CurrentValueRaw => currentValue;
		protected internal sealed override object CachedValueRaw => cachedValue;
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

		internal override void Initialize () {
			currentValue = defaultValue;
			cachedValue = defaultValue;
			isDirty = false;
		}

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

		private protected sealed override bool OnReset (bool apply) {
			SetValue (defaultValue);
			if (apply) {
				ApplyValue ();
			}
			return true;
		}

		private protected sealed override byte[] SerializeInternal () {
			return OnSerialize ();
		}

		/// <summary>
		/// Called when the setting should be saved.<br></br>
		/// Should return <see cref="CurrentValue"/> as <c>byte[]</c>.
		/// </summary>
		protected abstract byte[] OnSerialize ();

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