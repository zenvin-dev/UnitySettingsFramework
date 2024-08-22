using System;
using UnityEngine;
using Zenvin.Settings.Framework;

namespace Zenvin.Settings.UI {
	/// <summary>
	/// Base class for a UI Control managing a <see cref="SettingBase"/>'s value during runtime.<br></br>
	/// Implement through <see cref="SettingControl{TControlType, THandledType}"/>.
	/// </summary>
	[DisallowMultipleComponent]
	public abstract class SettingControl : MonoBehaviour {

		/// <summary> The type of the <see cref="SettingBase"/> that this Control is valid for. </summary>
		public abstract Type ControlType { get; }
		/// <summary> The non-generic <see cref="SettingBase"/> object assigned to this Control. </summary>
		public SettingBase SettingRaw { get; internal set; }


		private protected void Setup (SettingBase setting) {
			SettingRaw = setting;
			OnSetupInternal ();
		}

		private protected abstract void OnSetupInternal ();


		/// <summary>
		/// Tries to create a new instance of this Control for the given Setting.<br></br>
		/// Will fail if the given Setting is not of a valid type for this Control.
		/// </summary>
		/// <param name="setting"> The Setting to spawn the control for. </param>
		/// <param name="control"> The new instance (if any). </param>
		public bool TryInstantiateWith (SettingBase setting, out SettingControl control) {
			if (!CanAssignTo (setting)) {
				control = null;
				return false;
			}
			control = Instantiate (this);
			control.Setup (setting);
			return true;
		}

		private protected abstract bool CanAssignTo (SettingBase setting);

	}

	/// <summary>
	/// Base class for a UI Control managing a <see cref="SettingBase"/>'s value during runtime.
	/// </summary>
	/// <typeparam name="TControlType"> The most basic type of <see cref="SettingBase{T}"/> that this control can handle. </typeparam>
	/// <typeparam name="THandledType"> The type used by the handled <see cref="SettingBase{T}"/>. </typeparam>
	public abstract class SettingControl<TControlType, THandledType> : SettingControl where TControlType : SettingBase<THandledType> {

		/// <inheritdoc/>
		public sealed override Type ControlType => typeof (TControlType);

		/// <summary> The <see cref="SettingBase{T}"/> assigned to this Control. </summary>
		public TControlType Setting { get; private set; }


		/// <summary>
		/// Calls <see cref="SettingBase{T}.SetValue(T)"/> on the current <see cref="Setting"/>, if there is one.
		/// </summary>
		public void SetValue (THandledType value) {
			if (Setting != null) {
				Setting.SetValue (value);
			}
		}

		/// <summary>
		/// Called when the Control is first being set up with a Setting.
		/// </summary>
		protected virtual void OnSetup () { }

		/// <summary>
		/// Called when the Setting's value was changed in any way. <br></br>
		/// Equivalent of subscribing to <see cref="SettingBase{T}.ValueChanged"/>.<br></br>
		/// <b>Do not change the Setting's value from here, unless you know what you are doing. Otherwise you might create an infinite recursion!</b>
		/// </summary>
		/// <param name="mode"> In what way the value was changed. </param>
		protected virtual void OnSettingValueChanged (SettingBase.ValueChangeMode mode) { }

		/// <summary>
		/// Called when the visible state of the <see cref="Setting"/> or one of its parent groups was changed. <br></br>
		/// Equivalent of subscribing to <see cref="FrameworkObject.VisibilityChanged"/>. <br></br>
		/// Also called while the control is being set up.
		/// </summary>
		protected virtual void OnVisibilityChanged () { }

		/// <summary>
		/// Called when the validity of the <see cref="Setting"/> associated with the Control changes.<br></br>
		/// Equivalent of subscribing to <see cref="ValidatedSettingBase{T}.ValidityChanged"/>.
		/// </summary>
		/// <remarks>
		/// Requires the <see cref="SettingBase"/> associated with the Control to inherit <see cref="ValidatedSettingBase{T}"/>.
		/// </remarks>
		/// <param name="isValid"></param>
		protected virtual void OnValidityChanged (bool isValid) { }

		protected virtual void OnDestroy () {
			if (Setting != null) {
				Setting.ValueChanged -= OnSettingValueChanged;
				Setting.VisibilityChanged -= OnVisibilityChanged;
			}
		}


		private protected sealed override bool CanAssignTo (SettingBase setting) {
			return setting is TControlType;
		}

		private protected override sealed void OnSetupInternal () {
			Setting = SettingRaw as TControlType;
			Setting.ValueChanged += OnSettingValueChanged;
			Setting.VisibilityChanged += OnVisibilityChanged;
			OnSetup ();
			OnVisibilityChanged ();
		}
	}
}