using Zenvin.Settings.Framework;
using UnityEngine;

namespace Zenvin.Settings.UI {
	public abstract class SettingControl : MonoBehaviour {

		public SettingBase SettingRaw { get; internal set; }


		internal void Setup (SettingBase setting) {
			SettingRaw = setting;
			OnSetupInternal ();
		}

		private protected abstract void OnSetupInternal ();

	}

	public abstract class SettingControl<T, U> : SettingControl where T : SettingBase<U> where U : struct {

		public T Setting { get; internal set; }


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