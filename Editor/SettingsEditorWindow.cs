using Object = UnityEngine.Object;

using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using System;

namespace Zenvin.Settings.Framework {
	internal class SettingsEditorWindow : EditorWindow, ISerializationCallbackReceiver {

		[Flags]
		internal enum HierarchyFilter : int {
			/// <summary> Settings created during edit time. </summary>
			Normal = 0b0001,
			/// <summary> Settings loaded during runtime. </summary>
			External = 0b0010,
			/// <summary> Dirty settings. </summary>
			Dirty = 0b0100,
			/// <summary> Non-dirty settings. </summary>
			Clean = 0b1000,

			All = ~0b0
		}

		private const float indentSize = 15f;
		private const float margin = 1f;

		private static readonly Color hierarchyColorA = /*new Color (0.3f, 0.3f, 0.3f)*/new Color (0f, 0f, 0f, 0f);
		private static readonly Color hierarchyColorB = new Color (0.1f, 0.1f, 0.1f, 0f);
		private static readonly Color hierarchyColorSelected = new Color (0f, 0.5f, 1.0f, 0.4f);
		private static readonly Color hierarchyColordragged = new Color (1f, 0.5f, 1f, 0.4f);

		private static GUIStyle foldoutStyleInternal;
		private static GUIStyle foldoutStyleExternal;
		private static GUIStyle labelStyleInternal;
		private static GUIStyle labelStyleExternal;


		[SerializeField, HideInInspector] private List<SettingsGroup> hierarchyState;

		private static SettingsAsset asset;

		private ScriptableObject selected = null;
		private float hierarchyWidth = 200f;

		[NonSerialized] private string searchString = string.Empty;
		private List<SettingBase> searchResults = null;
		private HierarchyFilter searchFilter = HierarchyFilter.All;

		[NonSerialized] private ScriptableObject dragged = null;
		[NonSerialized] private Rect? dragPreview;

		private readonly Dictionary<SettingsGroup, bool> expansionState = new Dictionary<SettingsGroup, bool> ();
		private Vector2 hierarchyScroll;
		private Vector2 editorScroll;
		private Editor editor = null;

		private Type[] viableTypes = null;

		private HierarchyFilter CurrentFilter => Application.isPlaying ? searchFilter : HierarchyFilter.All;


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
			win.minSize = new Vector2 (500f, 200f);
			win.Show ();
		}


		// Initialization

		private void OnEnable () {
			ExpandToSelection (false);
		}

		private void SetupStyles () {
			foldoutStyleInternal = EditorStyles.foldout;
			foldoutStyleExternal = new GUIStyle (EditorStyles.foldout) { fontStyle = FontStyle.Italic };
			labelStyleInternal = EditorStyles.label;
			labelStyleExternal = new GUIStyle (EditorStyles.label) { fontStyle = FontStyle.Italic };
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

			if (foldoutStyleInternal == null || foldoutStyleExternal == null || labelStyleInternal == null || labelStyleExternal == null) {
				SetupStyles ();
			}

			Rect topBarRect = new Rect (
				margin, margin, position.width - margin * 2f, EditorGUIUtility.singleLineHeight + 1
			);
			Rect hierarchyRect = new Rect (
				margin, margin + topBarRect.height + margin, hierarchyWidth - margin * 2f, position.height - margin * 2f - topBarRect.height
			);
			Rect editorRect = new Rect (
				hierarchyWidth + 2f + margin * 2f, margin + topBarRect.height + margin, position.width - hierarchyWidth - 2f - margin * 2f, position.height - margin * 3f - topBarRect.height
			);

			Rect rect = new Rect (hierarchyWidth, topBarRect.height, 2f, position.height - topBarRect.height);
			if (MakeHorizontalDragRect (ref rect)) {
				hierarchyWidth = rect.x;
				hierarchyWidth = Mathf.Clamp (hierarchyWidth, 200f, position.width - 300f);
				Repaint ();
			}

			DrawTopBar (topBarRect);
			DrawHierarchy (hierarchyRect);
			DrawEditor (editorRect);

			if (dragged != null && dragPreview.HasValue) {
				EditorGUI.DrawRect (dragPreview.Value, Color.blue);
				Repaint ();
			}

			Event e = Event.current;
			if (e.type == EventType.MouseUp && dragged != null) {
				dragged = null;
				e.Use ();
			}

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

		private void DrawTopBar (Rect rect) {
			Rect bar = new Rect (rect);
			bar.height -= 1f;

			Rect line = new Rect (rect);
			line.height = 1f;
			line.y += bar.height;

			EditorGUI.DrawRect (line, new Color (0.1f, 0.1f, 0.1f));

			GUILayout.BeginArea (bar);

			DrawSearchBar (hierarchyWidth);

			GUILayout.EndArea ();
		}

		private void DrawHierarchy (Rect rect) {
			GUILayout.BeginArea (rect);

			//DrawSearchBar ();

			hierarchyScroll = EditorGUILayout.BeginScrollView (hierarchyScroll, false, false);

			if (searchResults == null) {
				int index = 0;
				DrawSettingsGroup (asset, 0, ref index);
			} else {
				for (int i = 0; i < searchResults.Count; i++) {
					DrawSetting (searchResults[i], 0, i);
				}
			}

			EditorGUILayout.EndScrollView ();
			GUILayout.EndArea ();
		}

		private void DrawEditor (Rect rect) {
			if (editor == null || editor.target == null || editor.serializedObject == null) {
				return;
			}

			GUILayout.BeginArea (rect);
			EditorGUI.BeginDisabledGroup (Application.isPlaying);

			switch (editor.target) {
				case SettingsGroup g:
					DrawDefaultEditor (g);
					break;
				case SettingBase s:
					DrawDefaultEditor (s);
					break;
				default:
					return;
			}

			GUILayout.Space (10);

			editorScroll = EditorGUILayout.BeginScrollView (editorScroll, false, false);
			if (Application.isPlaying) {

				if (editor.target is SettingBase s) {
					EditorGUILayout.LabelField ("Default Value", s.DefaultValueRaw.ToString (), EditorStyles.textField);
					EditorGUILayout.LabelField ("Current Value", s.CurrentValueRaw.ToString (), EditorStyles.textField);
					EditorGUILayout.LabelField ("Cached Value", s.CachedValueRaw.ToString (), EditorStyles.textField);
					EditorGUILayout.LabelField ("Is Dirty", s.IsDirty.ToString (), EditorStyles.textField);
				}

			} else {

				editor.DrawDefaultInspector ();
				editor.serializedObject.ApplyModifiedProperties ();
				EditorUtility.SetDirty (editor.target);

			}
			EditorGUILayout.EndScrollView ();

			EditorGUI.EndDisabledGroup ();
			GUILayout.EndArea ();
		}

		private void DrawDefaultEditor (SettingsGroup group) {

			if (group != asset) {
				DrawGuidField (editor.serializedObject, true);
			}

			GUILayout.Space (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);

			EditorGUILayout.PropertyField (editor.serializedObject.FindProperty (nameof (SettingsGroup.groupName)), new GUIContent ("Name"));
			EditorGUILayout.PropertyField (editor.serializedObject.FindProperty (nameof (SettingsGroup.groupNameLocKey)), new GUIContent ("Loc. Key"));
			EditorGUILayout.PropertyField (editor.serializedObject.FindProperty (nameof (SettingsGroup.groupIcon)), new GUIContent ("Icon"));

			if (group == asset) {
				GUILayout.Space (10);
				EditorGUILayout.LabelField ("Registered Settings", asset.RegisteredSettingsCount.ToString (), EditorStyles.textField);
				EditorGUILayout.LabelField ("Dirty Settings", asset.DirtySettingsCount.ToString (), EditorStyles.textField);
			}

			if (!Application.isPlaying) {
				editor.serializedObject.ApplyModifiedProperties ();
			}
		}

		private void DrawDefaultEditor (SettingBase setting) {
			DrawGuidField (editor.serializedObject, false);

			GUILayout.Space (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);

			EditorGUILayout.PropertyField (editor.serializedObject.FindProperty ("settingName"), new GUIContent ("Name"));
			EditorGUILayout.PropertyField (editor.serializedObject.FindProperty ("settingNameLocKey"), new GUIContent ("Loc. Key"));

			if (!Application.isPlaying) {
				editor.serializedObject.ApplyModifiedProperties ();
			}
		}


		private void DrawSearchBar (float? width = null) {

			string search = searchString;

			if (width.HasValue) {
				GUILayout.BeginHorizontal (GUILayout.MaxWidth (width.Value));
				//searchString = EditorGUILayout.DelayedTextField (searchString, EditorStyles.toolbarSearchField, GUILayout.MaxWidth (width.Value));
			} else {
				GUILayout.BeginHorizontal ();
				//searchString = EditorGUILayout.DelayedTextField (searchString, EditorStyles.toolbarSearchField);
			}

			searchString = EditorGUILayout.DelayedTextField (searchString, EditorStyles.toolbarSearchField, GUILayout.ExpandWidth (true));

			if (Application.isPlaying) {
				searchFilter = (HierarchyFilter)EditorGUILayout.EnumFlagsField (searchFilter);
			}
			GUILayout.EndHorizontal ();

			if (string.IsNullOrEmpty (searchString)) {
				searchResults = null;
				Repaint ();
				return;
			}

			if (search != searchString) {
				var settings = asset.GetAllSettings ();
				searchResults = new List<SettingBase> ();
				foreach (var s in settings) {
					if (s.Name.Contains (searchString)) {
						searchResults.Add (s);
					}
				}
			}
		}

		private void DrawGuidField (SerializedObject obj, bool group) {
			if (obj == null) {
				return;
			}
			SerializedProperty guidProp = obj.FindProperty ("guid");
			if (guidProp == null) {
				return;
			}

			string val = guidProp.stringValue;
			string newVal = EditorGUILayout.DelayedTextField ("GUID", val);
			if (newVal == val) {
				return;
			}

			if (!asset.Editor_IsValidGUID (newVal, group)) {
				newVal = val;
			}

			guidProp.stringValue = newVal;
			obj.ApplyModifiedPropertiesWithoutUndo ();
		}


		private void DrawSettingsGroup (SettingsGroup group, int groupIndex, ref int index, int indent = 0) {
			if (group == null) {
				return;
			}

			index++;

			Color col = GetHierarchyColor (group, index);

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
				expanded = EditorGUI.Foldout (foldRect, expanded, group.Name, false, group.External ? foldoutStyleExternal : foldoutStyleInternal);
				expansionState[group] = expanded;
			} else {
				foldRect.x += indentSize;
				foldRect.width -= indentSize;
				EditorGUI.LabelField (foldRect, group.Name);
			}

			Event e = Event.current;
			int quad = GetCursorQuadrant (rect, e.mousePosition);

			if (lblRect.Contains (e.mousePosition)) {
				switch (e.type) {
					case EventType.MouseDown:
						if (dragged == null) {
							e.Use ();
							Select (group);
						}
						break;
					case EventType.ContextClick:
						e.Use ();
						Select (group);
						ShowGroupMenu (group);
						break;
					case EventType.MouseDrag:
						if (e.button == 0) {
							Select (null);
							if (dragged == null && group != asset) {
								e.Use ();
								dragged = group;
							}
						}
						break;
				}

				// TODO: Fix Group dragging
				SettingsGroup dragGroup = dragged as SettingsGroup;
				if (dragGroup != null && dragGroup.IsNestedChildGroup (group)) {
					dragGroup = null;
				}

				Rect topRect = new Rect (lblRect);
				topRect.height *= 0.75f;
				Rect btmRect = new Rect (lblRect);
				btmRect.height *= 0.25f;
				btmRect.y += topRect.height;

				if (e.type == EventType.MouseUp || e.type == EventType.MouseLeaveWindow) {
					if (dragGroup != null) {
						e.Use ();

						if (quad == 1 && dragGroup != group) {
							MoveGroup (dragGroup, group, groupIndex);
							Debug.Log ("Transferred");
						} else if (quad == -1) {
							MoveGroup (dragGroup, group.Parent == null ? asset : group.Parent, groupIndex);
							Debug.Log ("Moved");
						} else {
							Debug.Log ("-");
						}

						dragged = null;
						Repaint ();
					}
				}

				const float previewHeight = 2f;

				if (dragGroup == null || quad == 0) {
					dragPreview = null;
				} else if (quad == 1 && dragGroup != group) {

					Rect preview = new Rect (rect);
					preview.y += preview.height - previewHeight * 2f;
					preview.height = previewHeight * 2f;
					dragPreview = preview;

				} else if (quad == -1) {

					Rect preview = new Rect (rect);
					preview.y += preview.height - previewHeight;
					preview.height = previewHeight;
					dragPreview = preview;

				} else {
					dragPreview = null;
				}
			}

			if (!expanded) {
				return;
			}

			for (int i = 0; i < group.ChildGroupCount; i++) {
				DrawSettingsGroup (group.GetGroupAt (i), i, ref index, indent + 1);
			}

			DrawGroupSettings (group, ref index, indent + 1);

		}

		private void DrawGroupSettings (SettingsGroup group, ref int index, int indent) {

			for (int i = 0; i < group.SettingCount; i++) {

				SettingBase setting = group.GetSettingAt (i);
				if (setting == null) {
					continue;
				}

				//var filter = CurrentFilter;
				//if ((int)filter != -1) {
				//	if ((filter & HierarchyFilter.Clean) != HierarchyFilter.Clean || !setting.IsDirty &&
				//		(filter & HierarchyFilter.Dirty) != HierarchyFilter.Dirty || setting.IsDirty &&
				//		(filter & HierarchyFilter.Normal) != HierarchyFilter.Normal || !setting.External &&
				//		(filter & HierarchyFilter.External) != HierarchyFilter.External || setting.External) {

				//		continue;
				//	}
				//}

				index++;

				DrawSetting (setting, indent, index);
			}

		}

		private void DrawSetting (SettingBase setting, int indent, int index) {
			Color col = GetHierarchyColor (setting, index);

			Rect rect = EditorGUILayout.GetControlRect ();
			rect.x -= 2f;
			rect.width += 4;
			rect.y -= 2f;
			rect.height += 2f;

			EditorGUI.DrawRect (rect, col);


			float indentValue = (indent + 1) * indentSize + 10f;
			Rect foldRect = new Rect (rect.x + indentValue, rect.y, rect.width - indentValue, rect.height);
			EditorGUI.LabelField (foldRect, $"{setting.Name} ({setting.ValueType.Name})", setting.External ? labelStyleExternal : labelStyleInternal);

			Event e = Event.current;
			if (foldRect.Contains (e.mousePosition)) {
				switch (e.type) {
					case EventType.MouseDown:
						e.Use ();
						Select (setting);
						break;
					case EventType.ContextClick:
						e.Use ();
						Select (setting);
						ShowSettingMenu (setting);
						break;
				}
			}

			// TODO: Implement Setting dragging
		}

		private void MoveGroup (SettingsGroup dragged, SettingsGroup settingsGroup, int groupIndex) {
			settingsGroup.AddChildGroup (dragged, groupIndex);
		}

		private void Select (ScriptableObject sel) {
			if (sel == selected) {
				return;
			}
			GUI.FocusControl ("");
			if (sel == null) {
				editor = null;
			} else {
				editor = Editor.CreateEditor (sel);
			}
			selected = sel;
		}


		private void ShowGroupMenu (SettingsGroup group) {
			GenericMenu gm = new GenericMenu ();

			if (!Application.isPlaying) {
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
						string ns = string.IsNullOrEmpty (t.Namespace) ? "<Global>" : t.Namespace;
						string gn = generic?.Name?.Split ('`')[0];
						gm.AddItem (
							new GUIContent ($"Add Setting/{ns}/{t.Name} {(string.IsNullOrEmpty (gn) ? "" : $"({gn})")}"),
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
			}

			gm.AddItem (new GUIContent ("Collapse/All"), false, () => {
				Select (asset);
				expansionState.Clear ();
				expansionState[asset] = true;
			});
			gm.AddItem (new GUIContent ("Collapse/To This"), false, () => { ExpandToSelection (true); });

			gm.ShowAsContext ();
		}

		private void ShowSettingMenu (SettingBase setting) {


			GenericMenu gm = new GenericMenu ();

			if (!Application.isPlaying) {
				gm.AddItem (new GUIContent ("Duplicate Setting"), false, DuplicateSetting, setting);
				gm.AddSeparator ("");
				gm.AddItem (new GUIContent ("Delete Setting"), false, DeleteSetting, setting);
				gm.AddSeparator ("");
			}

			gm.AddItem (new GUIContent ("Collapse/All"), false, () => {
				Select (asset);
				expansionState.Clear ();
				expansionState[asset] = true;
			});
			gm.AddItem (new GUIContent ("Collapse/To This"), false, () => { ExpandToSelection (true); });

			gm.ShowAsContext ();
		}


		private void CreateSettingAsChildOfGroup (object data) {
			if (!(data is NewSettingData d)) {
				return;
			}
			if (d.Group == null) {
				return;
			}

			SettingBase setting = AssetUtility.CreateAsPartOf<SettingBase> (asset, d.SettingType, s => {
				s.name = "New Setting";
				s.Name = "New Setting";
				s.GUID = Guid.NewGuid ().ToString ();
				s.asset = asset;
			});

			d.Group.AddSetting (setting);
			Select (setting);
			ExpandToSelection (false);
		}

		private void CreateGroupAsChildOfGroup (object group) {
			if (!(group is SettingsGroup g)) {
				return;
			}

			SettingsGroup newGroup = AssetUtility.CreateAsPartOf<SettingsGroup> (asset, g => {
				g.name = "New Group";
				g.groupName = "New Group";
				g.GUID = Guid.NewGuid ().ToString ();
			});

			g.AddChildGroup (newGroup);
			Select (newGroup);
			ExpandToSelection (false);
		}

		private void DuplicateSetting (object setting) {
			if (!(setting is SettingBase set)) {
				return;
			}

			SettingBase newSetting = Instantiate (set);
			newSetting.GUID = Guid.NewGuid ().ToString ();

			AssetDatabase.AddObjectToAsset (newSetting, asset);
			AssetDatabase.SaveAssets ();
			AssetDatabase.Refresh ();

			newSetting.group = null;
			set.group.AddSetting (newSetting);
			Select (newSetting);
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

		private void DeleteSetting (object setting) {
			if (setting is SettingBase s) {
				s.group.RemoveSetting (s);

				DestroyImmediate (s, true);
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

			r.width += Mathf.Abs (e.delta.x);
			if (e.delta.x < 0f) {
				r.x += e.delta.x;
			}

			if (r.Contains (e.mousePosition)) {
				EditorGUIUtility.AddCursorRect (new Rect (e.mousePosition, Vector2.one * 32f), MouseCursor.ResizeHorizontal);

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

			types.Sort (Compare);
			viableTypes = types.ToArray ();
		}

		private int Compare (Type a, Type b) {
			return a.FullName.CompareTo (b.FullName);
		}

		private Color GetHierarchyColor (ScriptableObject obj, int index) {
			if (obj == dragged) {
				return hierarchyColordragged;
			} else if (obj == selected) {
				return hierarchyColorSelected;
			} else {
				return index % 2 == 0 ? hierarchyColorA : hierarchyColorB;
			}
		}

		private int GetCursorQuadrant (Rect rect, Vector2 cursor, float ratio = 0.5f) {
			ratio = Mathf.Clamp (ratio, 0.1f, 0.9f);
			if (!rect.Contains (cursor)) {
				return 0;
			}
			if (cursor.y < rect.y + rect.height * ratio) {
				return 1;
			}
			return -1;
		}

		private void ExpandToSelection (bool collapseOthers) {
			if (collapseOthers) {
				expansionState.Clear ();
			}
			SettingsGroup sel = selected is SettingsGroup g ? g : (selected is SettingBase s ? s.group : null);
			int i = 0;
			while (sel != null && i < 50) {
				expansionState[sel] = true;
				sel = sel.Parent;
				i++;
			}
		}


		// Serialization

		void ISerializationCallbackReceiver.OnBeforeSerialize () {
			hierarchyState = new List<SettingsGroup> ();
			foreach (var kvp in expansionState) {
				if (kvp.Value) {
					hierarchyState.Add (kvp.Key);
				}
			}
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize () {
			if (hierarchyState != null) {
				foreach (var o in hierarchyState) {
					if (o != null) {
						expansionState[o] = true;
					}
				}
			}
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