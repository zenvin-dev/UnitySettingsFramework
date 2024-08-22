using TMPro;
using UnityEngine;

namespace Zenvin.Settings.Samples {
	[AddComponentMenu ("Zenvin/Settings/UI/Keybind Popup")]
	public class KeybindPopup : KeybindPopupBase {

		[SerializeField] private TextMeshProUGUI label;


		private void Start () {
			gameObject.SetActive (false);
		}

		protected override void OnOpen () {
			gameObject.SetActive (true);
			if (label != null) {
				label.SetText ($"Rebinding <b>{Control.SettingRaw.Name}</b>.\nWaiting for input...");
			}
		}

		protected override void OnClose () {
			gameObject.SetActive (false);
		}
	}
}
