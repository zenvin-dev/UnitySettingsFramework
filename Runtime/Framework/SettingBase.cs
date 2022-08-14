using UnityEngine;
using System;
using Assets.UnitySettingsFramework.Runtime.Framework.Serialization;

namespace Zenvin.Settings.Framework {
	/// <summary>
	/// Base class for all Settings objects.
	/// </summary>
	public abstract class SettingBase : FrameworkObject, IComparable<SettingBase> {

		public enum ValueChangeMode {
			Initialize,
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

		/// <summary> Whether the Setting's value has been changed but not applied yet. </summary>
		public abstract bool IsDirty { get; private protected set; }
		/// <summary> The sorting order this Setting will have, relative to its group. Assigned automatically on internal groups, can be assigned manually on external ones. </summary>
		public abstract int OrderInGroup { get; internal set; }
		/// <summary> The <see cref="SettingsAsset"/> at the root of this Setting's hierarchy. </summary>
		public SettingsAsset Asset => asset;


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
	/// Inherit to create Settings with any type, but keep in mind that Unity won't be able to serialize every type you may conveive Settings for, which could break the implementation.
	/// </summary>
	public abstract class SettingBase<T> : SettingBase {

		public delegate void OnValueChangedEvt (ValueChangeMode mode);
		public event OnValueChangedEvt ValueChanged;

		[NonSerialized] private T cachedValue;
		[NonSerialized] private T currentValue;
		[NonSerialized] private bool isDirty;

		[NonSerialized] private int orderInGroup;

		[SerializeField, DefaultValue] private T defaultValue;


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
				if (isDirty != value) {
					isDirty = value;
					asset.SetDirty (this, isDirty);
				}
			}
		}

		/// <summary>
		/// Value by which Settings <b>can</b> be ordered in their group hierarchy.<br></br>
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
				OnValueChanged (ValueChangeMode.Set);
				ValueChanged?.Invoke (ValueChangeMode.Set);
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
		/// Called during change operations, before the <see cref="ValueChanged"/> is invoked.
		/// </summary>
		protected virtual void OnValueChanged (ValueChangeMode mode) { }

		/// <summary>
		/// Called after the setting has been registered in the Settings Asset.
		/// Use this to set up necessary values.
		/// </summary>
		protected virtual void OnInitialize () { }

		/// <summary>
		/// Called during initialization. Override to change how the initial default value is set.
		/// </summary>
		protected virtual T OnSetupInitialDefaultValue () {
			T value = defaultValue;
			ProcessValue (ref value);
			return value;
		}

		internal sealed override void Initialize () {
			OnInitialize ();

			T value = OnSetupInitialDefaultValue ();

			currentValue = value;
			cachedValue = value;

			OnValueChanged (ValueChangeMode.Initialize);
			ValueChanged?.Invoke (ValueChangeMode.Initialize);
		}


		private protected sealed override bool OnApply () {
			if (!IsDirty) {
				return false;
			}
			currentValue = cachedValue;
			IsDirty = false;
			OnValueChanged (ValueChangeMode.Apply);
			ValueChanged?.Invoke (ValueChangeMode.Apply);
			return true;
		}

		private protected sealed override bool OnRevert () {
			if (!IsDirty) {
				return false;
			}
			T curr = currentValue;
			cachedValue = currentValue;
			IsDirty = false;
			OnValueChanged (ValueChangeMode.Revert);
			ValueChanged?.Invoke (ValueChangeMode.Revert);
			return true;
		}

		private protected sealed override bool OnReset (bool apply) {
			SetValue (defaultValue);
			if (apply) {
				ApplyValue ();
			}
			OnValueChanged (ValueChangeMode.Reset);
			ValueChanged?.Invoke (ValueChangeMode.Reset);
			return true;
		}

	}
}