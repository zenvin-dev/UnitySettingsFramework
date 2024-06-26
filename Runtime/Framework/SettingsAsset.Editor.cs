#if UNITY_EDITOR
using UnityEditor;

namespace Zenvin.Settings.Framework {
	public partial class SettingsAsset {
		partial void OnInitialized () {
			EditorApplication.playModeStateChanged -= PlayModeStateChangedHandler;
			EditorApplication.playModeStateChanged += PlayModeStateChangedHandler;
		}

		private void PlayModeStateChangedHandler (PlayModeStateChange change) {
			if (change != PlayModeStateChange.ExitingPlayMode)
				return;

			settingsDict.Clear ();
			groupsDict.Clear ();
			dirtySettings.Clear ();
			initialized = false;

			Log ("[Editor] Reset initialization state.");
		}
	}
}
#endif
