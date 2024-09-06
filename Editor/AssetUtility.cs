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
			AddObjectToAssetAndRefresh (asset, instance);

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

			if (!noUndo)
				Undo.RecordObject (asset, "Add sub-asset");

			AddObjectToAssetAndRefresh (asset, instance);

			return instance;
		}

		public static T ConditionalCreateAsPartOf<T> (Object asset, Type assetType, Func<T, bool> condition, Action<T> onBeforeAdd = null, string undoText = null)
			where T : ScriptableObject {

			if (asset == null || assetType.IsAbstract || condition == null) {
				return null;
			}

			var instance = ScriptableObject.CreateInstance (assetType) as T;
			if (instance == null) {
				return null;
			}

			if (!condition.Invoke (instance)) {
				DestroyReliable (instance);
				return null;
			}

			if (undoText != null)
				Undo.RecordObject (asset, undoText);

			onBeforeAdd?.Invoke (instance);
			AddObjectToAssetAndRefresh (asset, instance);
			return instance;
		}

		public static void DestroyReliable (Object instance) {
			if (instance == null)
				return;

			if (Application.isPlaying) {
				Object.Destroy (instance);
			} else {
				Object.DestroyImmediate (instance);
			}
		}

		private static void AddObjectToAssetAndRefresh<T> (Object asset, T instance) where T : ScriptableObject {
			AssetDatabase.AddObjectToAsset (instance, asset);
			EditorUtility.SetDirty (asset);
			AssetDatabase.SaveAssets ();
			AssetDatabase.Refresh ();
		}
	}
}