using Zenvin.Settings.Framework;
using UnityEngine;
using System;

namespace Zenvin.Settings.UI {
	[DisallowMultipleComponent]
	public abstract class SettingControl : MonoBehaviour {

		public abstract Type ControlType { get; }
		public SettingBase SettingRaw { get; internal set; }


		private void Setup (SettingBase setting) {
			SettingRaw = setting;
			OnSetupInternal ();
		}

		private protected abstract void OnSetupInternal ();


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

	public abstract class SettingControl<T, U> : SettingControl where T : SettingBase<U> where U : struct {

		public sealed override Type ControlType => typeof (T);

		public T Setting { get; internal set; }


		private protected override bool CanAssignTo (SettingBase setting) {
			return setting is T;
		}

		private protected override sealed void OnSetupInternal () {
			Setting = SettingRaw as T;
			OnSetup ();
		}

		protected virtual void OnSetup () { }

		public void SetValue (U value) {
			if (Setting != null) {
				Setting.SetValue (value);
			}
		}

	}
}