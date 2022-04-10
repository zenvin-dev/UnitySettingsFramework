using UnityEngine;
using System;

namespace Zenvin.Settings.Framework {
	/// <summary>
	/// Base class for all Settings objects.
	/// </summary>
	public abstract class SettingBase : FrameworkObject, IComparable<SettingBase> {

		public enum ValueChangeMode {
			Set,
			Reset,
			Revert,
			Apply
		}

		[SerializeField, HideInInspector] internal SettingsAsset asset;
		[SerializeField, HideInInspector] internal SettingsGroup group;


		protected internal abstract object DefaultValueRaw { get; }
		protected internal abstract object CurrentValueRaw { get; }
		protected internal abstract object CachedValueRaw { get; }
		protected internal abstract Type ValueType { get; }
		public abstract bool IsDirty { get; private protected set; }
		public abstract int OrderInGroup { get; internal set; }

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


		public override string ToString () {
			return $"Setting '{Name}' ('{GUID}')";
		}

		int IComparable<SettingBase>.CompareTo (SettingBase other) {
			int def = OrderInGroup.CompareTo (other.OrderInGroup);
			if (def != 0) {
				return def;
			}
			return External.CompareTo (other.External);
		}
	}

	/// <summary>
	/// Base class for all typed Settings objects.<br></br>
	/// Inherit to create Settings with any type, but note that Unity won't be able to serialize every type you may conveive Settings for, which could break the implementation.
	/// </summary>
	public abstract class SettingBase<T> : SettingBase {

		public delegate void OnValueChangedEvt (ValueChangeMode mode);
		public event OnValueChangedEvt OnValueChanged;

		[NonSerialized] private T cachedValue;
		[NonSerialized] private T currentValue;
		[NonSerialized] private bool isDirty;

		[NonSerialized] private int orderInGroup;

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

		/// <summary>
		/// Value by which Settings can be ordered in their group hierarchy.<br></br>
		/// Settings created in the editor will automatically be assigned a value.
		/// </summary>
		public sealed override int OrderInGroup {
			get => orderInGroup;
			internal set => orderInGroup = value;
		}

		protected internal sealed override object DefaultValueRaw => defaultValue;
		protected internal sealed override object CurrentValueRaw => currentValue;
		protected internal sealed override object CachedValueRaw => cachedValue;
		protected internal sealed override Type ValueType => typeof (T);


		/// <summary>
		/// Creates a new, external instance of a given <see cref="SettingBase{T}"/> and initializes with the given values.
		/// </summary>
		public static U CreateInstanceWithValues<U> (T defaultValue, StringValuePair[] values = null) where U : SettingBase<T> {
			if (!Application.isPlaying) {
				return null;
			}

			U setting = CreateInstance<U> ();
			setting.External = true;

			setting.OnCreateWithValues (values);

			setting.ProcessValue (ref defaultValue);
			setting.defaultValue = defaultValue;
			setting.currentValue = defaultValue;
			setting.cachedValue = defaultValue;

			return setting;
		}


		/// <summary>
		/// Sets the setting's next value. Will be applied when <see cref="ApplyValue"/> is called.
		/// </summary>
		/// <param name="value"> The value to set. </param>
		public void SetValue (T value) {
			ProcessValue (ref value);

			if (!cachedValue.Equals (value)) {
				cachedValue = value;
				IsDirty = true;
				OnValueChanged?.Invoke (ValueChangeMode.Set);
			}
		}

		/// <summary>
		/// Called during <see cref="CreateInstanceWithValues{U}(T, StringKeyValuePair[])"/>, <b>before</b> the default value is set.
		/// </summary>
		protected virtual void OnCreateWithValues (StringValuePair[] values) { }

		/// <summary>
		/// Called during <see cref="SetValue(T)"/> to process the change before it is cached.
		/// </summary>
		/// <param name="value"> The next value of the setting. </param>
		protected virtual void ProcessValue (ref T value) { }

		/// <summary>
		/// Called during <see cref="ApplyValue"/>, before the <see cref="OnValueApplied"/> is invoked.
		/// </summary>
		protected virtual void OnAfterApplyValue () { }

		/// <summary>
		/// Called when the setting should be saved.<br></br>
		/// Should return <see cref="CurrentValue"/> as <c>byte[]</c>.
		/// </summary>
		protected abstract byte[] OnSerialize ();

		/// <summary>
		/// Called when the setting should is loaded.<br></br>
		/// Should convert the given <c>byte[]</c> to <see cref="T"/>, so it can be applied to the setting.
		/// </summary>
		/// <param name="data"> The loaded data. </param>
		protected abstract T OnDeserialize (byte[] data);

		/// <summary>
		/// Called after the setting has been registered in the Settings Asset.
		/// Use this to set up necessary values.
		/// </summary>
		protected virtual void OnInitialize () { }


		internal override void Initialize () {
			OnInitialize ();

			T value = defaultValue;
			ProcessValue (ref value);

			currentValue = value;
			cachedValue = value;
		}


		private protected sealed override bool OnApply () {
			if (!IsDirty) {
				return false;
			}
			currentValue = cachedValue;
			IsDirty = false;
			OnAfterApplyValue ();
			OnValueChanged?.Invoke (ValueChangeMode.Apply);
			return true;
		}

		private protected sealed override bool OnRevert () {
			if (!IsDirty) {
				return false;
			}
			T curr = currentValue;
			cachedValue = currentValue;
			IsDirty = false;
			OnValueChanged?.Invoke (ValueChangeMode.Revert);
			return true;
		}

		private protected sealed override bool OnReset (bool apply) {
			SetValue (defaultValue);
			if (apply) {
				ApplyValue ();
			}
			OnValueChanged?.Invoke (ValueChangeMode.Reset);
			return true;
		}

		private protected sealed override byte[] SerializeInternal () {
			return OnSerialize ();
		}

		private protected sealed override void DeserializeInternal (byte[] data) {
			T value = OnDeserialize (data);
			SetValue (value);
			ApplyValue ();
		}

	}

	[Serializable]
	internal class SettingData {
		public string GUID { get; set; }
		public byte[] Data { get; set; }
	}
}