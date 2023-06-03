using Object = UnityEngine.Object;

using UnityEngine;
using UnityEditor;
using System;

namespace Zenvin.Settings.Framework {
	internal static class AssetUtility {

		public static T CreateAsPartOf<T> (Object asset, Action<T> onBeforeAdd = null, bool noUndo = false) where T : ScriptableObject {
			if (asset == null || typeof (T).IsAbstract) {
				return null;
			}

			T instance = ScriptableObject.CreateInstance<T> ();

			onBeforeAdd?.Invoke (instance);

			if (!noUndo) {
				Undo.RecordObject (asset, "Add sub-asset");
			}
			AssetDatabase.AddObjectToAsset (instance, asset);
			EditorUtility.SetDirty (asset);
			AssetDatabase.SaveAssets ();
			AssetDatabase.Refresh ();

			return instance;
		}

		public static T CreateAsPartOf<T> (Object asset, Type assetType, Action<T> onBeforeAdd = null, bool noUndo = false) where T : ScriptableObject {
			if (asset == null || assetType.IsAbstract) {
				return null;
			}

			T instance = ScriptableObject.CreateInstance (assetType) as T;
			if (instance == null) {
				return null;
			}

			onBeforeAdd?.Invoke (instance);

			if (!noUndo) {
				Undo.RecordObject (asset, "Add sub-asset");
			}
			AssetDatabase.AddObjectToAsset (instance, asset);
			EditorUtility.SetDirty (asset);
			AssetDatabase.SaveAssets ();
			AssetDatabase.Refresh ();

			return instance;
		}

	}
}