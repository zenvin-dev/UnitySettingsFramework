#if UNITY_EDITOR
using UnityEditor;

namespace Zenvin.Settings.Framework {
	public partial class SettingsAsset {
		partial void OnInitialized_Editor () {
			EditorApplication.playModeStateChanged -= PlayModeStateChangedHandler;
			EditorApplication.playModeStateChanged += PlayModeStateChangedHandler;
		}

		private void PlayModeStateChangedHandler (PlayModeStateChange change) {
			if (change != PlayModeStateChange.ExitingPlayMode)
				return;

			settingsDict.Clear ();
			groupsDict.Clear ();
			dirtySettings.Clear ();
			Initialized = false;

			Log ("[Editor] Reset initialization state.");
		}
	}
}
#endif
