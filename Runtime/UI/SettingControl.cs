using Zenvin.Settings.Framework;
using UnityEngine;
using System;

namespace Zenvin.Settings.UI {
	[DisallowMultipleComponent]
	public abstract class SettingControl : MonoBehaviour {

		/// <summary> The type of the <see cref="SettingBase"/> that this Control is valid for. </summary>
		public abstract Type ControlType { get; }
		/// <summary> The "raw" <see cref="SettingBase"/> object assigned to this Control. </summary>
		public SettingBase SettingRaw { get; internal set; }


		private void Setup (SettingBase setting) {
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
	/// Base class for a UI Control managing a <see cref="SettingBase"/>'s value on runtime.<br></br>
	/// <typeparamref name="T"/> represents the type of setting that can be managed by the Control (base types will work as well),
	/// while <typeparamref name="U"/> constrains the type of that setting.
	/// </summary>
	public abstract class SettingControl<T, U> : SettingControl where T : SettingBase<U> {

		/// <inheritdoc/>
		public sealed override Type ControlType => typeof (T);

		/// <summary> The <see cref="SettingBase{T}"/> assigned to this Control. </summary>
		public T Setting { get; internal set; }


		/// <summary>
		/// Calls <see cref="SettingBase{T}.SetValue(T)"/> on the current <see cref="Setting"/>, if there is one.
		/// </summary>
		public void SetValue (U value) {
			if (Setting != null) {
				Setting.SetValue (value);
			}
		}

		/// <summary>
		/// Called when the Control is first being set up with a Setting.
		/// </summary>
		protected virtual void OnSetup () { }

		/// <summary>
		/// Called when the Control's current Setting is reset.
		/// </summary>
		protected virtual void OnSettingReset () { }

		/// <summary>
		/// Called when the Control's current Setting is reverted.
		/// </summary>
		protected virtual void OnSettingReverted () { }


		private protected sealed override bool CanAssignTo (SettingBase setting) {
			return setting is T;
		}

		private protected override sealed void OnSetupInternal () {
			Setting = SettingRaw as T;
			Setting.OnValueReset += OnSettingReset;
			Setting.OnValueReverted += OnSettingReverted;
			OnSetup ();
		}

		private void OnDestroy () {
			Setting.OnValueReset -= OnSettingReset;
			Setting.OnValueReverted -= OnSettingReverted;
		}

	}
}