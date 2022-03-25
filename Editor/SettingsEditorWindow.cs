using Object = UnityEngine.Object;

using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

namespace Zenvin.Settings.Framework {
	internal class SettingsEditorWindow : EditorWindow {

		private const float indentSize = 12f;

		private static SettingsAsset asset;

		private ScriptableObject selected = null;
		private float hierarchyWidth = 200f;

		private readonly Dictionary<SettingsGroup, bool> expansionState = new Dictionary<SettingsGroup, bool> ();
		private Vector2 hierarchyScroll;
		private Vector2 editorScroll;
		private Editor editor = null;

		private Type[] viableTypes = null;


		// Menus

		[MenuItem ("Assets/Create/Scriptable Objects/Zenvin/Settings Asset", priority = -10000)]
		private static SettingsAsset HandleCreateSettingsAsset () {
			SettingsAsset asset = FindAsset ();

			if (asset == null) {
				asset = CreateInstance<SettingsAsset> ();
				asset.name = "Game Settings";
				asset.groupName = "Game Settings";

				AssetDatabase.CreateAsset (asset, $"Assets/Game Settings.asset");

				AssetDatabase.Refresh ();
				AssetDatabase.SaveAssets ();
			} else {
				Selection.activeObject = asset;
			}

			return asset;
		}


		[MenuItem ("Window/Zenvin/Settings Asset Editor")]
		private static void Init () {
			SettingsEditorWindow win = GetWindow<SettingsEditorWindow> ();
			win.titleContent = new GUIContent ("Settings Editor");
			win.minSize = new Vector2 (400f, 200f);
			win.Show ();
		}


		// GUI methods

		private void OnGUI () {
			if (asset == null) {
				asset = FindAsset ();
			}
			if (asset == null) {
				DrawCreateAssetButton ();
				Repaint ();
				return;
			}

			Rect rect = new Rect (hierarchyWidth, 0f, 2f, position.height);
			if (MakeHorizontalDragRect (ref rect)) {
				hierarchyWidth = rect.x;
				hierarchyWidth = Mathf.Clamp (hierarchyWidth, 200f, position.width - 300f);
				Repaint ();
			}

			Rect hierarchyRect = new Rect (0f, 0f, hierarchyWidth, position.height);
			Rect editorRect = new Rect (hierarchyWidth + 2f, 0f, position.width - hierarchyWidth - 2f, position.height);

			DrawHierarchy (hierarchyRect);
			DrawEditor (editorRect);

			EditorUtility.SetDirty (asset);
		}

		private void DrawCreateAssetButton () {
			Vector2 btnSize = new Vector2 (250f, 50f);
			float xOffset = (position.width - btnSize.x) * 0.5f;
			float yOffset = (position.height - btnSize.y) * 0.5f;

			Rect btnRect = new Rect (xOffset, yOffset, btnSize.x, btnSize.y);

			if (GUI.Button (btnRect, "Create Settings Asset")) {
				asset = HandleCreateSettingsAsset ();
			}
		}

		private void DrawHierarchy (Rect rect) {
			GUILayout.BeginArea (rect);
			hierarchyScroll = EditorGUILayout.BeginScrollView (hierarchyScroll, false, false);

			int index = 0;
			DrawSettingsGroup (asset, ref index);

			EditorGUILayout.EndScrollView ();
			GUILayout.EndArea ();
		}

		private void DrawEditor (Rect rect) {
			if (editor == null || editor.target == null) {
				return;
			}

			GUILayout.BeginArea (rect);
			editorScroll = EditorGUILayout.BeginScrollView (editorScroll, false, false);

			editor.DrawDefaultInspector ();
			EditorUtility.SetDirty (editor.target);

			EditorGUILayout.EndScrollView ();
			GUILayout.EndArea ();
		}


		private void DrawSettingsGroup (SettingsGroup group, ref int index, int indent = 0) {
			if (group == null) {
				return;
			}

			index++;

			Color col = selected == group ? new Color (0.2f, 0.6f, 1f, 0.5f) : (index % 2 == 0 ? new Color (0.3f, 0.3f, 0.3f) : new Color (0.4f, 0.4f, 0.4f));

			Rect rect = EditorGUILayout.GetControlRect ();
			rect.x -= 2f;
			rect.width += 4;
			rect.y -= 2f;
			rect.height += 2f;

			EditorGUI.DrawRect (rect, col);

			Rect btnRect = new Rect (rect);
			btnRect.x += indentSize * indent;
			btnRect.width = indentSize;

			Rect lblRect = new Rect (rect);
			lblRect.x += indentSize * (indent + 1);
			lblRect.width -= indentSize * (indent + 1);

			if (!expansionState.TryGetValue (group, out bool expanded)) {
				expanded = false;
			}

			float indentValue = indent * indentSize + 10f;
			Rect foldRect = new Rect (rect.x + indentValue, rect.y, rect.width - indentValue, rect.height);

			if ((group.ChildGroupCount + group.SettingCount) > 0) {
				expanded = EditorGUI.Foldout (foldRect, expanded, group.Name, false);
				expansionState[group] = expanded;
			} else {
				foldRect.x += 18f;
				foldRect.width -= 18f;
				EditorGUI.LabelField (foldRect, group.Name);
			}

			Event e = Event.current;
			if (lblRect.Contains (e.mousePosition)) {
				switch (e.type) {
					case EventType.MouseDown:
						e.Use ();
						Select (group);
						break;
					case EventType.ContextClick:
						e.Use ();
						Select (group);
						ShowGroupMenu (group);
						break;
				}
			}

			if (!expanded) {
				return;
			}

			for (int i = 0; i < group.ChildGroupCount; i++) {
				DrawSettingsGroup (group.GetGroupAt (i), ref index, indent + 1);
			}

			DrawGroupSettings (group, ref index, indent + 1);

		}

		private void DrawGroupSettings (SettingsGroup group, ref int index, int indent) {

		}

		private void Select (ScriptableObject sel) {
			if (sel == selected) {
				return;
			}
			if (sel == null) {
				editor = null;
			} else {
				editor = Editor.CreateEditor (sel);
			}
			selected = sel;
		}

		private void ShowGroupMenu (SettingsGroup group) {
			GenericMenu gm = new GenericMenu ();

			LoadViableTypes ();
			if (viableTypes.Length == 0) {
				gm.AddDisabledItem (new GUIContent ("Add Setting"));
			} else {
				foreach (Type t in viableTypes) {
					Type generic = null;
					Type[] generics = t.BaseType.GetGenericArguments ();
					if (generics.Length > 0) {
						generic = generics[0];
					}
					gm.AddItem (
						new GUIContent ($"Add Setting/{t.Namespace}/{t.Name} ({(generic?.Name?.Split ('`')[0] ?? "<None>")})"),
						false, CreateSettingAsChildOfGroup, new NewSettingData (t, group)
					);
				}
			}


			gm.AddItem (new GUIContent ("Add Group"), false, CreateGroupAsChildOfGroup, group);

			if (group != asset) {
				gm.AddSeparator ("");
				gm.AddItem (new GUIContent ("Delete Group"), false, DeleteGroup, group);
			}

			gm.AddSeparator ("");
			gm.AddItem (new GUIContent ("Collapse All"), false, () => {
				Select (asset);
				expansionState.Clear ();
			});

			gm.ShowAsContext ();
		}

		private void CreateSettingAsChildOfGroup (object group) {
			if (!(group is SettingsGroup g)) {
				return;
			}

		}

		private void CreateGroupAsChildOfGroup (object group) {
			if (!(group is SettingsGroup g)) {
				return;
			}

			SettingsGroup newGroup = CreateInstance<SettingsGroup> ();
			newGroup.name = "New Group";
			newGroup.groupName = "New Group";

			AssetDatabase.AddObjectToAsset (newGroup, asset);
			AssetDatabase.SaveAssets ();
			AssetDatabase.Refresh ();

			g.AddChildGroup (newGroup);
		}

		private void DeleteGroup (object group) {
			if (group is SettingsGroup g) {
				if (selected == g) {
					Select (g.Parent);
				}

				for (int i = g.ChildGroupCount - 1; i >= 0; i--) {
					asset.AddChildGroup (g.GetGroupAt (i));
				}
				for (int i = g.SettingCount - 1; i >= 0; i--) {
					asset.AddSetting (g.GetSettingAt (i));
				}
				expansionState.Remove (g);
				if (g.Parent != null) {
					g.Parent.RemoveChildGroup (g);
				}
				DestroyImmediate (g, true);
				AssetDatabase.Refresh ();
				AssetDatabase.SaveAssets ();
				Repaint ();
			}
		}

		// Helper Methods

		public static bool MakeHorizontalDragRect (ref Rect rect, float size = 5f, Color? color = null) {
			Color col = color.HasValue ? color.Value : new Color (0.1f, 0.1f, 0.1f);

			EditorGUI.DrawRect (rect, col);
			Event e = Event.current;

			Rect r = new Rect (rect);
			r.x -= size * 0.5f;
			r.width = Mathf.Abs (size);

			if (r.Contains (e.mousePosition)) {
				if (e.type == EventType.MouseDrag) {
					rect.x += e.delta.x;
					e.Use ();
					return true;
				}
			}
			return false;
		}

		private static SettingsAsset FindAsset () {
			string[] guids = AssetDatabase.FindAssets ($"t:{typeof (SettingsAsset).FullName}");
			if (guids.Length > 0) {
				return AssetDatabase.LoadAssetAtPath<SettingsAsset> (AssetDatabase.GUIDToAssetPath (guids[0]));
			}
			return null;
		}

		private void LoadViableTypes () {
			if (viableTypes != null) {
				return;
			}

			List<Type> types = new List<Type> ();
			Type baseType = typeof (SettingBase);

			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies ();
			foreach (Assembly asm in assemblies) {
				Type[] asmTypes = asm.GetTypes ();
				foreach (Type t in asmTypes) {
					if (baseType.IsAssignableFrom (t) && !t.IsAbstract) {
						types.Add (t);
					}
				}
			}

			viableTypes = types.ToArray ();
		}


		private class NewSettingData {
			public readonly Type SettingType;
			public SettingsGroup Group;

			public NewSettingData (Type settingType, SettingsGroup group) {
				SettingType = settingType;
				Group = group;
			}
		}

	}
}