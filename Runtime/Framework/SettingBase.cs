using System;
using System.ComponentModel;
using UnityEngine;

namespace Zenvin.Settings.Framework {
	/// <summary>
	/// Base class for all Settings objects.
	/// </summary>
	public abstract class SettingBase : FrameworkObject, IComparable<SettingBase> {

		/// <summary>
		/// Enum used in <see cref="SettingBase{T}.ValueChanged"/>, to convey how the Setting's value was changed.
		/// </summary>
		public enum ValueChangeMode {
			/// <summary> The Setting was initialized by <see cref="SettingsAsset.Initialize"/>. </summary>
			Initialize,
			/// <summary> The Setting's value was set by <see cref="SettingBase{T}.SetValue(T)"/>. </summary>
			Set,
			/// <summary>
			/// The Setting's value was reset to its default by <see cref="ResetValue(bool)"/>.<br></br>
			/// <see cref="IsDirty"/> will be <see langword="true"/>.
			/// </summary>
			Reset,
			/// <summary> The Setting's cached value was reverted to its previously applied state by <see cref="RevertValue"/> </summary>
			Revert,
			/// <summary>
			/// The Setting's cached value was applied by <see cref="ApplyValue"/>.<br></br>
			/// <see cref="SettingBase{T}.CachedValue"/> and <see cref="SettingBase{T}.CurrentValue"/> will be the same.
			/// </summary>
			Apply,
			/// <summary>
			/// The Setting's value was changed due to deserialization by <see cref="SettingsAsset.DeserializeSettings{T}(Serialization.ISerializer{T}, SettingsGroup.SettingBaseFilter)"/>.
			/// </summary>
			Deserialize,
			/// <summary>
			/// The Setting's value implements <see cref="INotifyPropertyChanged"/> and triggered its changed event.
			/// </summary>
			Notify,
		}

		[SerializeField, HideInInspector] internal SettingsAsset asset;
		[SerializeField, HideInInspector] internal SettingsGroup group;


		internal abstract object DefaultValueRaw { get; }
		internal abstract object CurrentValueRaw { get; }
		internal abstract object CachedValueRaw { get; }
		internal abstract Type ValueType { get; }

		/// <summary> Whether the Setting's value has been changed but not applied yet. </summary>
		public abstract bool IsDirty { get; private protected set; }
		/// <summary> The sorting order this Setting will have, relative to its group. Assigned automatically on internal groups, can be assigned manually on external ones. </summary>
		public abstract int OrderInGroup { get; internal set; }
		/// <summary> The <see cref="SettingsAsset"/> at the root of this Setting's hierarchy. </summary>
		public SettingsAsset Asset => asset;
		/// <summary> The <see cref="SettingsGroup"/> which this Setting is a child of. May be the same as <see cref="Asset"/>. </summary>
		public SettingsGroup Group => group;


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


		/// <summary>
		/// Attempts to replace the Setting's current <see cref="SettingBase{T}.DefaultValue"/> 
		/// with a value parsed from the given <see cref="StringValuePair"/> collection.<br></br>
		/// Should only be used for manipulation while loading new Settings.
		/// </summary>
		/// <param name="values"> The value collection to parse the new default from. If the collection is null or empty, the method will abort. </param>
		/// <param name="updateMode"> Determines if and how the Setting's current value is updated after a successful parsing. </param>
		/// <returns> Whether parsing was successful. </returns>
		public bool OverrideDefaultValue (StringValuePair[] values, UpdateValueMode updateMode) {
			if (values == null || values.Length == 0)
				return false;

			if (!OnOverrideDefault (values, out var wasDefault))
				return false;

			if (updateMode == UpdateValueMode.DontUpdate)
				return true;

			if (wasDefault) {
				OnReset (updateMode == UpdateValueMode.ApplyIfDefault);
			}
			return true;
		}

		private protected abstract bool OnOverrideDefault (StringValuePair[] values, out bool wasDefault);


		internal abstract void OnAfterDeserialize ();

		/// <summary>
		/// Returns the name and GUID of the Setting.
		/// </summary>
		public override string ToString () {
			return $"Setting '{Name}' (GUID: '{GUID}')";
		}

		/// <inheritdoc/>
		public sealed override SettingVisibility GetVisibilityInHierarchy () {
			SettingVisibility vis = Group.GetVisibilityInHierarchy ();
			if ((int)vis > (int)Visibility) {
				return vis;
			}
			return Visibility;
		}

		int IComparable<SettingBase>.CompareTo (SettingBase other) {
			int def = OrderInGroup.CompareTo (other.OrderInGroup);
			if (def != 0) {
				return def;
			}
			return External.CompareTo (other.External);
		}

		internal void Log (string message) {
			if (Asset != null) {
				Asset.Log (message);
			}
		}


		[ContextMenu ("Force Delete Setting")]
		private void ForceDelete () {
			Debug.Log ($"Deleting {ToString ()}");

			if (Group != null) {
				Group.RemoveSetting (this);
			}

			DestroyImmediate (this, true);
		}
	}

	/// <summary>
	/// Base class for all typed Settings objects. Inherit to create Settings with any type.
	/// </summary>
	/// <remarks>
	/// <para>
	/// When implementing Settings for new types, it is worth bearing in mind that Unity will not be able to serialize any arbitrary type.<br></br>
	/// As such, implementing Settings for types Unity cannot serialize may cause unexpected behaviour in the editor,
	/// as it will become impossible to define a default value for the Setting.<br></br>
	/// The same goes for value types that Unity does not have a built-in property drawer for.
	/// </para>
	/// <para>
	/// Custom default value handling can be achieved by decorating a given Setting class with the <see cref="Utility.HasDeviatingDefaultValueAttribute"/>
	/// and overriding <see cref="SettingBase{T}.OnSetupInitialDefaultValue"/>.<br></br>
	/// An example for this can be found in <see cref="ValueArraySetting"/> and <see cref="ValueArraySetting{T}"/>, respectively.
	/// </para>
	/// </remarks>
	public abstract class SettingBase<T> : SettingBase {

		/// <summary>
		/// Delegate type for the <see cref="ValueChanged"/> event.
		/// </summary>
		/// <param name="mode"> The way in which the Setting's value was modified. </param>
		public delegate void ValueChangedEvent (ValueChangeMode mode);
		/// <summary> Invoked every time the Setting's value changes. </summary>
		/// <remarks> The event invocation will always happen <b>after</b> <see cref="OnValueChanged(ValueChangeMode)"/> was called for the same change. </remarks>
		public event ValueChangedEvent ValueChanged;

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

		internal sealed override object DefaultValueRaw => defaultValue;
		internal sealed override object CurrentValueRaw => currentValue;
		internal sealed override object CachedValueRaw => cachedValue;
		internal sealed override Type ValueType => typeof (T);


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
		/// Sets the setting's next value. Will be applied when <see cref="SettingBase.ApplyValue"/> is called.
		/// </summary>
		/// <param name="value"> The value to set. </param>
		public void SetValue (T value) {
			if (!Application.isPlaying) {
				Log ("Cannot set Setting value during edit-time.");
				return;
			}

			Log ($"Setting Value of {ToString ()} to '{value}'");
			ProcessValue (ref value);

			if (CompareEquality (cachedValue, value)) {
				return;
			}

			UnSubscribeNotification (cachedValue);
			cachedValue = value;
			IsDirty = true;
			SubscribeNotification (cachedValue);

			OnValueChanged (ValueChangeMode.Set);
			ValueChanged?.Invoke (ValueChangeMode.Set);
		}

		/// <summary>
		/// Called during <see cref="SettingBase{T}.CreateInstanceWithValues{U}(T, StringValuePair[])"/>, <b>before</b> the default value is set.
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
		/// Called after the setting has been registered in the Settings Asset. <br></br>
		/// Use this to set up necessary values. <br></br>
		/// Assignment of the default value will happen immediately after.
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

		/// <summary>
		/// Used to compare Setting values. Can be overridden to define custom behaviours during operations that set values.
		/// </summary>
		/// <remarks>
		/// If not overridden, will equate <see langword="null"/> and use the default <see cref="object.Equals(object)"/> implementation for non-null objects.
		/// </remarks>
		protected virtual bool CompareEquality (T a, T b) {
			if (a == null && b == null) {
				return true;
			}
			if (a != null && a.Equals (b)) {
				return true;
			}
			if (b != null && b.Equals (a)) {
				return true;
			}
			return false;
		}

		/// <summary>
		/// If implemented, attempts to parse a collection of string/value pairs into the Setting's value type <typeparamref name="T"/>.
		/// </summary>
		/// <param name="values"> The values to be parsed. Will always have at least once entry. </param>
		/// <param name="value"> The result of the parsing operation, or <see langword="default"/>. </param>
		/// <returns> Whether parsing was successful. </returns>
		protected virtual bool TryGetOverrideValue (StringValuePair[] values, out T value) {
			value = default;
			return false;
		}


		internal sealed override void Initialize () {
			Log ($"Initializing {ToString ()}");
			OnInitialize ();

			T value = OnSetupInitialDefaultValue ();

			currentValue = value;
			cachedValue = value;
			isDirty = false;

			SubscribeNotification (value);
			OnValueChanged (ValueChangeMode.Initialize);
			ValueChanged?.Invoke (ValueChangeMode.Initialize);

			Log ($"Initialized {ToString ()}. Current Value: '{currentValue}', Cached Value: '{cachedValue}', Default Value: '{defaultValue}'");
		}

		internal sealed override void OnAfterDeserialize () {
			OnValueChanged (ValueChangeMode.Deserialize);
			ValueChanged?.Invoke (ValueChangeMode.Deserialize);
			Log ($"Deserialized {ToString ()}");
		}


		private protected sealed override bool OnApply () {
			if (!IsDirty) {
				Log ($"Did not apply value on {ToString ()} because it was not dirty.");
				return false;
			}
			Log ($"Applying value '{cachedValue}' on {ToString ()}.");
			currentValue = cachedValue;
			IsDirty = false;
			OnValueChanged (ValueChangeMode.Apply);
			ValueChanged?.Invoke (ValueChangeMode.Apply);
			return true;
		}

		private protected sealed override bool OnRevert () {
			if (!IsDirty) {
				Log ($"Did not revert value on {ToString ()} because it was not dirty.");
				return false;
			}
			Log ($"Applying value of {ToString ()}.");
			T curr = currentValue;
			cachedValue = currentValue;
			IsDirty = false;
			OnValueChanged (ValueChangeMode.Revert);
			ValueChanged?.Invoke (ValueChangeMode.Revert);
			return true;
		}

		private protected sealed override bool OnReset (bool apply) {
			Log ($"Resetting {ToString ()} [apply: {apply}]");
			SetValue (defaultValue);
			if (apply) {
				ApplyValue ();
			}
			OnValueChanged (ValueChangeMode.Reset);
			ValueChanged?.Invoke (ValueChangeMode.Reset);
			return true;
		}

		private protected sealed override bool OnOverrideDefault (StringValuePair[] values, out bool wasDefault) {
			wasDefault = false;
			if (!TryGetOverrideValue (values, out var newDefault))
				return false;

			var oldDefault = defaultValue;
			ProcessValue (ref newDefault);
			defaultValue = newDefault;

			wasDefault = CompareEquality (newDefault, oldDefault);
			return true;
		}


		private void SubscribeNotification (T value) {
			if (value != null && value is INotifyPropertyChanged notify) {
				notify.PropertyChanged += ValuePropertyChangedHandler;
			}
		}

		private void UnSubscribeNotification (T value) {
			if (value != null && value is INotifyPropertyChanged notify) {
				notify.PropertyChanged -= ValuePropertyChangedHandler;
			}
		}


		private void ValuePropertyChangedHandler (object sender, PropertyChangedEventArgs args) {
			IsDirty = true;
			OnValueChanged (ValueChangeMode.Notify);
		}
	}
}