using TMPro;
using UnityEngine;
using Zenvin.Settings.Framework;

namespace Zenvin.Settings.Samples {
	[AddComponentMenu ("Zenvin/Settings/UI/Keybind Control")]
	public class KeybindControl : KeybindControlBase {

		[SerializeField] private TextMeshProUGUI labelText;
		[SerializeField] private TextMeshProUGUI bindingText;


		/// <inheritdoc/>
		protected override void OnSetup () {
			base.OnSetup ();

			if (labelText != null) {
				labelText.SetText (SettingRaw.Name);
			}
			UpdateBindingUI ();
		}

		/// <inheritdoc/>
		protected override void OnSettingValueChanged (SettingBase.ValueChangeMode mode) {
			UpdateBindingUI ();
		}

		/// <inheritdoc/>
		protected override void OnStartedRebind () {
			KeybindPopupBase.Open (this);
			UpdateBindingUI ();
		}

		/// <inheritdoc/>
		protected override void OnEndedRebind (bool cancelled) {
			KeybindPopupBase.Close ();
			UpdateBindingUI ();
		}


		private void UpdateBindingUI () {
			if (bindingText == null)
				return;

			bindingText.SetText (Setting.GetDisplayTextForCurrentValue ());
		}
	}
}
