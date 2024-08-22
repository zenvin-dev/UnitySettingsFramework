using UnityEngine;

namespace Zenvin.Settings.Samples {
	[DisallowMultipleComponent]
	public abstract class KeybindPopupBase : MonoBehaviour {

		private static KeybindPopupBase popup;

		protected KeybindControlBase Control { get; private set; } = null;

		public bool IsOpen => popup != null && popup.Control != null;


		private void Awake () {
			if (popup != null && popup != this) {
				Destroy (gameObject);
				return;
			}
			popup = this;
			OnAwake ();
		}


		public static bool Open (KeybindControlBase control) {
			if (control == null)
				return false;
			if (popup == null)
				return false;

			popup.Control = control;
			popup.OnOpen ();
			return true;
		}

		public static void Close () {
			if (popup == null)
				return;
			if (popup.Control == null)
				return;

			popup.OnClose ();
			popup.Control = null;
		}


		protected virtual void OnAwake () { }

		protected virtual void OnOpen () { }

		protected virtual void OnClose () { }
	}
}
