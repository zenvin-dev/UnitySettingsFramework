using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Object = UnityEngine.Object;

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

			All = ~0b0000
		}

		private const float indentSize = 15f;
		private const float margin = 1f;

		private static readonly Color hierarchyColorA = /*new Color (0.3f, 0.3f, 0.3f)*/new Color (0f, 0f, 0f, 0f);
		private static readonly Color hierarchyColorB = new Color (0.1f, 0.1f, 0.1f, 0f);
		private static readonly Color hierarchyColorSelected = new Color (0f, 0.5f, 1.0f, 0.4f);
		private static readonly Color hierarchyColorDragged = new Color (1f, 0.5f, 1f, 0.4f);
		private static readonly Color hierarchyColorHover = new Color (0.3f, 0.3f, 0.3f);

		private static GUIStyle foldoutStyleInternal;
		private static GUIStyle foldoutStyleExternal;
		private static GUIStyle labelStyleInternal;
		private static GUIStyle labelStyleExternal;

		private static GUIContent dropdownArrowContent;


		[SerializeField] private Texture windowIcon;

		[SerializeField, HideInInspector] private List<SettingsGroup> hierarchyState;

		[SerializeField] private SettingsAsset asset;
		private SerializedObject assetObj;

		private SettingsAsset[] allAssets = null;

		private float hierarchyWidth = 300f;
		private bool returnToList = false;

		[NonSerialized] private string searchString = string.Empty;
		private List<SettingBase> searchResults = null;
		private HierarchyFilter searchFilter = HierarchyFilter.All;

		private ScriptableObject selected = null;
		[NonSerialized] private ScriptableObject dragged = null;
		[NonSerialized] private ScriptableObject hovered = null;
		[NonSerialized] private Rect? dragPreview;

		private readonly Dictionary<SettingsGroup, bool> expansionState = new Dictionary<SettingsGroup, bool> ();
		private Vector2 hierarchyScroll;
		private Vector2 editorScroll;
		private Vector2 assetSelectScroll;
		private Editor editor = null;

		private Type[] viableSettingTypes = null;
		private Type[] viableGroupTypes = null;


		private HierarchyFilter CurrentFilter => Application.isPlaying ? searchFilter : HierarchyFilter.All;
		private bool AllowDrag => searchResults == null;
		private SettingsAsset Asset {
			get {
				return asset;
			}
			set {
				if (value == asset) {
					return;
				}
				asset = value;
				if (asset != null) {
					assetObj = new SerializedObject (asset);
				} else {
					assetObj = null;
				}
			}
		}
		private SerializedObject AssetObject => assetObj ?? (asset == null ? null : new SerializedObject (asset));
		private GUIContent DropdownArrowContent => dropdownArrowContent == null ? (dropdownArrowContent = EditorGUIUtility.IconContent ("icon dropdown")) : dropdownArrowContent;


		// Menus

		[MenuItem ("Window/Zenvin/Settings Asset Editor")]
		private static void Init () {
			InitWindow ();
		}

		private static void Init (SettingsAsset edit) {
			InitWindow ().Asset = edit;
		}

		internal static void Open (SettingBase setting) {
			if (setting == null || setting.asset == null) {
				return;
			}
			var win = InitWindow ();
			win.Asset = setting.Asset;
			win.Select (setting);
		}

		private static SettingsEditorWindow InitWindow () {
			SettingsEditorWindow win = GetWindow<SettingsEditorWindow> ();
			win.titleContent = new GUIContent ("Settings Editor", win.windowIcon);
			win.minSize = new Vector2 (500f, 200f);
			win.Show ();
			return win;
		}

		[OnOpenAsset]
		private static bool HandleOpenAsset (int instanceID, int line) {
			Object obj = EditorUtility.InstanceIDToObject (instanceID);
			if (obj is SettingsAsset asset) {
				Init (asset);
				return true;
			}
			return false;
		}


		// Initialization

		private void OnEnable () {
			hierarchyWidth = EditorPrefs.GetFloat ($"{GetType ().FullName}.{nameof (hierarchyWidth)}", 200f);
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
			// prompt setting asset selection if there is none
			if (Asset == null) {
				assetObj = null;
				DrawAssetMenu ();
				Repaint ();
				return;
			}

			// make sure styles are set up
			if (foldoutStyleInternal == null || foldoutStyleExternal == null || labelStyleInternal == null || labelStyleExternal == null) {
				SetupStyles ();
			}

			if (Asset.ChildGroupCount == 0 && Asset.SettingCount == 0) {
				Select (Asset);
			}

			// create rects for windows partitions
			Rect topBarRect = new Rect (
				margin, margin, position.width - margin * 2f, EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + 1
			);
			Rect hierarchyRect = new Rect (
				margin, margin + topBarRect.height + margin, hierarchyWidth - margin * 2f, position.height - margin * 2f - topBarRect.height
			);
			Rect editorRect = new Rect (
				hierarchyWidth + 2f + margin * 2f, margin + topBarRect.height + margin, position.width - hierarchyWidth - 2f - margin * 2f, position.height - margin * 3f - topBarRect.height
			);

			Rect separatorRect = new Rect (hierarchyWidth, topBarRect.height, 1f, position.height - topBarRect.height);
			EditorGUI.DrawRect (separatorRect, new Color (0.1f, 0.1f, 0.1f));

			// draw window partition contents
			DrawTopBar (topBarRect);
			DrawHierarchy (hierarchyRect);
			DrawEditor (editorRect);

			// update window frequently if a hierarchy item is being dragged
			if (dragged != null && dragPreview.HasValue) {
				EditorGUI.DrawRect (dragPreview.Value, Color.blue);
				Repaint ();
			}

			// handle drag escaping
			Event e = Event.current;
			if (dragged != null) {
				if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape) {
					Select (dragged);
					HandleEndDrag ();
					e.Use ();
				}
			}

			if (Asset != null) {
				AssetObject?.ApplyModifiedProperties ();
				EditorUtility.SetDirty (Asset);
			}
			if (returnToList) {
				returnToList = false;
				Asset = null;
				Select (null);
			}
		}


		private void DrawAssetMenu () {
			const float menuWidth = 450f;
			const float verticalMargin = 20f;
			Rect menuRect = new Rect ((position.width - menuWidth) * 0.5f, verticalMargin, menuWidth, position.height - verticalMargin * 2f);

			GUILayout.BeginArea (menuRect);

			GUILayout.Label ("Select Settings Asset", EditorStyles.boldLabel);
			assetSelectScroll = GUILayout.BeginScrollView (assetSelectScroll, false, false);

			if (allAssets == null || allAssets.Length == 0) {
				LoadSettingsAssets ();
			}

			for (int i = 0; i < allAssets.Length; i++) {
				// create the rect for this hierarchy item
				Rect rect = EditorGUILayout.GetControlRect ();
				//rect.x -= 2f;
				//rect.width += 4;
				//rect.y -= 2f;
				//rect.height += 2f;

				// draw coloured background
				Color col = rect.Contains (Event.current.mousePosition) ? hierarchyColorHover : hierarchyColorA;
				EditorGUI.DrawRect (rect, col);

				// draw select button
				if (GUI.Button (rect, allAssets[i].name, EditorStyles.label)) {
					Asset = allAssets[i];
					GUILayout.EndScrollView ();
					GUILayout.EndArea ();
					return;
				}
			}

			GUILayout.FlexibleSpace ();

			// draw refresh button
			if (GUILayout.Button ("Refresh")) {
				LoadSettingsAssets ();
			}

			GUILayout.Space (10);

			// draw create asset button
			if (GUILayout.Button ("Create new Asset")) {
				string path = EditorUtility.SaveFilePanelInProject ("Create Settings Asset", "New Settings", "asset", "");
				if (string.IsNullOrEmpty (path)) {
					GUILayout.EndArea ();
					return;
				}
				Asset = CreateInstance<SettingsAsset> ();
				AssetDatabase.CreateAsset (Asset, path);

#if UNITY_2021_1_OR_NEWER
				AssetDatabase.SaveAssetIfDirty (Asset);
#else
				AssetDatabase.SaveAssets ();
#endif

				AssetDatabase.Refresh ();
				return;
			}


			GUILayout.EndScrollView ();
			GUILayout.EndArea ();
		}

		private void DrawTopBar (Rect rect) {
			Rect bar = new Rect (rect);
			bar.height -= 1f;

			Rect line = new Rect (rect);
			line.height = 1f;
			line.y += bar.height;

			EditorGUI.DrawRect (line, new Color (0.1f, 0.1f, 0.1f));

			GUILayout.BeginArea (bar);

			GUILayout.BeginHorizontal ();

			DrawSearchBar (hierarchyWidth);

			SettingsGroup selGroup = selected as SettingsGroup;
			bool canAdd = selGroup != null;

			GUI.enabled = canAdd;
			Rect addGroupBtnRect = EditorGUILayout.GetControlRect (false, GUILayout.Width (150));
			if (GUI.Button (addGroupBtnRect, "Add Group") && !Application.isPlaying) {
				GenericMenu gm = new GenericMenu ();
				PopulateGroupsTypeMenu (gm, selGroup, false);
				gm.DropDown (addGroupBtnRect);
			}

			Rect addSettingBtnRect = EditorGUILayout.GetControlRect (false, GUILayout.Width (150));
			if (GUI.Button (addSettingBtnRect, "Add Setting") && !Application.isPlaying) {
				GenericMenu gm = new GenericMenu ();
				PopulateSettingTypeMenu (gm, selGroup, false);
				gm.DropDown (addSettingBtnRect);
			}
			GUI.enabled = true;

			if (GUILayout.Button ("Select Asset", GUILayout.Width (150), GUILayout.Height (EditorGUIUtility.singleLineHeight))) {
				Selection.activeObject = Asset;
			}

			GUI.enabled = !Application.isPlaying;
			if (GUILayout.Button ("Restore (Experimental)", GUILayout.Width (150), GUILayout.Height (EditorGUIUtility.singleLineHeight))) {
				const string notice = "This function will attempt to detect and restore lost references inside the SettingAsset.\n" +
									  "But it may just as well break the asset. It is recommended to make a backup before proceeding.";
				if (EditorUtility.DisplayDialog ("Notice", notice, "Proceed", "Cancel")) {
					Restore (Asset);
				}
			}
			GUI.enabled = true;

			GUILayout.FlexibleSpace ();

			if (GUILayout.Button (new GUIContent ("Asset List"), GUILayout.Height (EditorGUIUtility.singleLineHeight), GUILayout.Width (150))) {
				returnToList = true;
			}

			GUILayout.EndHorizontal ();

			GUILayout.EndArea ();
		}

		private void DrawHierarchy (Rect rect) {
			GUILayout.BeginArea (rect);
			hierarchyScroll = EditorGUILayout.BeginScrollView (hierarchyScroll, false, false);

			// reset hovered object and drag preview rect
			hovered = null;
			dragPreview = null;

			// display normal hierarchy
			if (searchResults == null) {
				int index = 0;
				DrawSettingsGroup (Asset, 0, ref index);

				// display hierarchy based on search results
			} else {
				for (int i = 0; i < searchResults.Count; i++) {
					DrawSetting (searchResults[i], 0, i, i);
				}
			}

			EditorGUILayout.EndScrollView ();


			// draw preview rect if necessary
			if (dragPreview != null) {
				EditorGUI.DrawRect (dragPreview.Value, Color.blue);
			}

			GUILayout.EndArea ();
		}

		private void DrawEditor (Rect rect) {
			// return if current editor is invalid
			if (editor == null || editor.target == null || editor.serializedObject == null) {
				return;
			}


			GUILayout.BeginArea (rect);
			// make read-only while in play mode
			EditorGUI.BeginDisabledGroup (Application.isPlaying);

			bool isGroupAsset = editor.target is SettingsGroup;

			if (editor.target == Asset) {
				EditorGUILayout.LabelField ("GUID", "None (Root)");
			} else {
				DrawGuidField (editor.serializedObject, isGroupAsset);
			}

			GUILayout.Space (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
			DrawNameField (editor.serializedObject.FindProperty ("label"), editor.serializedObject.FindProperty ("guid"), editor.target == Asset, isGroupAsset);
			DrawPropertyField (editor.serializedObject.FindProperty ("labelLocalizationKey"), new GUIContent ("Name Loc. Key"));

			GUILayout.Space (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
			DrawTextArea ("Description", editor.serializedObject.FindProperty ("description"), rect);
			EditorGUILayout.PropertyField (editor.serializedObject.FindProperty ("descriptionLocalizationKey"), new GUIContent ("Description Loc. Key"));

			GUILayout.Space (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);

			// draw editor based on type of selected object
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

				// draw read-only runtime information
				if (editor.target is SettingBase s) {
					EditorGUILayout.LabelField ("Default Value", s.DefaultValueRaw?.ToString () ?? "", EditorStyles.textField);
					EditorGUILayout.LabelField ("Current Value", s.CurrentValueRaw?.ToString () ?? "", EditorStyles.textField);
					EditorGUILayout.LabelField ("Cached Value", s.CachedValueRaw?.ToString () ?? "", EditorStyles.textField);
					EditorGUILayout.LabelField ("Is Dirty", s.IsDirty.ToString (), EditorStyles.textField);

					GUILayout.Space (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
					EditorGUILayout.LabelField ("Order in Group", s.OrderInGroup.ToString (), EditorStyles.textField);
				}

			} else {

				// draw default inspector to allow for custom properties in settings
				editor.DrawDefaultInspector ();
				editor.serializedObject.ApplyModifiedProperties ();
				EditorUtility.SetDirty (editor.target);

			}
			EditorGUILayout.EndScrollView ();

			EditorGUI.EndDisabledGroup ();
			GUILayout.EndArea ();
		}

		private void DrawDefaultEditor (SettingsGroup group) {

			// as long as the selected object is not the root asset
			//if (group != asset) {
			//	// draw GUID field
			//	DrawGuidField (editor.serializedObject, true);
			//} else {
			//	EditorGUILayout.LabelField ("GUID", "None (Root)");
			//}

			//GUILayout.Space (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);

			// draw base properties separate from inspector
			//EditorGUILayout.PropertyField (editor.serializedObject.FindProperty (/*nameof (SettingsGroup.groupName)*/"label"), new GUIContent ("Name"));
			//EditorGUILayout.PropertyField (editor.serializedObject.FindProperty (/*nameof (SettingsGroup.groupNameLocKey)*/"labelLocalizationKey"), new GUIContent ("Loc. Key"));
			EditorGUILayout.PropertyField (editor.serializedObject.FindProperty (nameof (SettingsGroup.groupIcon)), new GUIContent ("Icon"));

			// display runtime information on root asset
			if (group == Asset && Application.isPlaying) {
				GUILayout.Space (10);
				EditorGUILayout.LabelField ("Registered Groups", Asset.RegisteredGroupsCount.ToString (), EditorStyles.textField);
				EditorGUILayout.LabelField ("Registered Settings", Asset.RegisteredSettingsCount.ToString (), EditorStyles.textField);
				EditorGUILayout.LabelField ("Dirty Settings", Asset.DirtySettingsCount.ToString (), EditorStyles.textField);
			}

			if (!Application.isPlaying) {
				editor.serializedObject.ApplyModifiedProperties ();
			}
		}

		private void DrawDefaultEditor (SettingBase setting) {
			//DrawGuidField (editor.serializedObject, false);

			//GUILayout.Space (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);

			//EditorGUILayout.PropertyField (editor.serializedObject.FindProperty (/*"settingName"*/"label"), new GUIContent ("Name"));
			//EditorGUILayout.PropertyField (editor.serializedObject.FindProperty (/*"settingNameLocKey"*/"labelLocalizationKey"), new GUIContent ("Loc. Key"));

			//if (!Application.isPlaying) {
			//	editor.serializedObject.ApplyModifiedProperties ();
			//}
		}


		// Special field drawers

		private void DrawSearchBar (float? width = null) {

			string search = searchString;

			// draw search field
			searchString = EditorGUILayout.DelayedTextField (searchString, EditorStyles.toolbarSearchField, GUILayout.Width (width ?? 100f));

			// if application is playing, enable setting filtering
			// during edit-time there are not enough relevant, differentiating properties to warrant filtering.
			if (Application.isPlaying) {
				searchFilter = (HierarchyFilter)EditorGUILayout.EnumFlagsField (searchFilter, GUILayout.Width (150));
			}

			// reset search results if search string is empty
			if (string.IsNullOrEmpty (searchString)) {
				searchResults = null;
				Repaint ();
				return;
			}

			// update search results if search string has changed
			if (search != searchString) {
				Select (null);
				var settings = Asset.GetAllSettings ();
				searchResults = new List<SettingBase> ();
				foreach (var s in settings) {
					if (s.Name.ToUpperInvariant ().Contains (searchString.ToUpperInvariant ())) {
						searchResults.Add (s);
					}
				}
			}
		}

		private void DrawGuidField (SerializedObject obj, bool group) {
			if (obj == null) {
				return;
			}

			// find the GUID property of the selected object
			SerializedProperty guidProp = obj.FindProperty ("guid");
			if (guidProp == null) {
				return;
			}

			// draw GUID input field
			string val = guidProp.stringValue;
			string newVal = EditorGUILayout.DelayedTextField ("GUID", val);
			if (newVal == val) {
				return;
			}

			TrySetGuid (guidProp, newVal, group);
		}

		private void DrawNameField (SerializedProperty nameProp, SerializedProperty guidProp, bool dontRename, bool isGroup) {
			if (nameProp == null) {
				return;
			}

			if (guidProp == null) {
				DrawPropertyField (editor.serializedObject.FindProperty ("label"), new GUIContent ("Name"));
				return;
			}

			GUILayout.BeginHorizontal ();
			string nameCache = nameProp.stringValue;
			Rect nameFieldRect = EditorGUILayout.GetControlRect (false);
			EditorGUI.DelayedTextField (nameFieldRect, nameProp, new GUIContent ("Name"));
			nameProp.serializedObject.ApplyModifiedProperties ();


			GUI.enabled = !string.IsNullOrEmpty (nameProp.stringValue) && !dontRename;
			if (GUILayout.Button ("Copy to GUID", EditorStyles.miniButtonLeft, GUILayout.Width (150))) {
				TrySetGuid (guidProp, nameProp.stringValue, isGroup);
			}

			Rect copyMenuRect = GUILayoutUtility.GetLastRect ();
			FrameworkObject frObj = nameProp.serializedObject.targetObject as FrameworkObject;

			if (GUILayout.Button (DropdownArrowContent, EditorStyles.miniButtonRight, GUILayout.Width (EditorGUIUtility.singleLineHeight))) {
				GenericMenu copyToGuidMenu = new GenericMenu ();
				copyToGuidMenu.AddItem (
					new GUIContent ("Copy/Name only"),
					false,
					() => TrySetGuid (guidProp, GetGuidFormattedName (frObj, false, false, false), isGroup)
				);
				copyToGuidMenu.AddItem (
					new GUIContent ("Copy/With parent name"),
					false,
					() => TrySetGuid (guidProp, GetGuidFormattedName (frObj, false, true, false), isGroup)
				);
				copyToGuidMenu.AddItem (
					new GUIContent ("Copy/With parent GUID"),
					false,
					() => TrySetGuid (guidProp, GetGuidFormattedName (frObj, true, false, false), isGroup)
				);

				copyToGuidMenu.AddItem (
					new GUIContent ("Copy Formatted/Name only"),
					false,
					() => TrySetGuid (guidProp, GetGuidFormattedName (frObj, false, false, true), isGroup)
				);
				copyToGuidMenu.AddItem (
					new GUIContent ("Copy Formatted/With parent name"),
					false,
					() => TrySetGuid (guidProp, GetGuidFormattedName (frObj, false, true, true), isGroup)
				);
				copyToGuidMenu.AddItem (
					new GUIContent ("Copy Formatted/With parent GUID"),
					false,
					() => TrySetGuid (guidProp, GetGuidFormattedName (frObj, true, false, true), isGroup)
				);

				copyToGuidMenu.AddSeparator ("");

				copyToGuidMenu.AddItem (
					new GUIContent ("Generate new GUID"),
					false,
					() => TrySetGuid (guidProp, Guid.NewGuid ().ToString (), isGroup)
				);
				copyToGuidMenu.DropDown (copyMenuRect);
			}
			GUI.enabled = true;

			GUILayout.EndHorizontal ();

			if (!dontRename) {
				if (nameProp.stringValue != nameCache) {
					var obj = nameProp.serializedObject.targetObject;
					obj.name = nameProp.stringValue;
					AssetDatabase.SaveAssets ();
					AssetDatabase.ImportAsset (AssetDatabase.GetAssetPath (Asset), ImportAssetOptions.ForceUpdate);
				}
			}
		}

		private void DrawPropertyField (SerializedProperty property, GUIContent label) {
			EditorGUILayout.PropertyField (property, label);
			property.serializedObject.ApplyModifiedProperties ();
		}

		private void DrawTextArea (string label, SerializedProperty property, Rect rect) {
			if (property.propertyType != SerializedPropertyType.String) {
				return;
			}
			EditorGUILayout.LabelField (label);

			string val = property.stringValue;
			float height = EditorStyles.textArea.CalcHeight (new GUIContent (val), rect.width);

			val = EditorGUILayout.TextArea (val, GUILayout.Height (Mathf.Max (height, EditorGUIUtility.singleLineHeight * 3f)));

			if (property.stringValue != val) {
				property.stringValue = val;
				property.serializedObject.ApplyModifiedProperties ();
			}
		}


		// Hierarchy drawers

		private void DrawSettingsGroup (SettingsGroup group, int groupIndex, ref int index, int indent = 0) {
			if (group == null) {
				return;
			}

			index++;

			// create the rect for this hierarchy item
			Rect rect = EditorGUILayout.GetControlRect ();
			rect.x -= 2f;
			rect.width += 4;
			rect.y -= 2f;
			rect.height += 2f;

			// draw coloured background
			Color col = GetHierarchyColor (group, index, rect.Contains (Event.current.mousePosition));
			EditorGUI.DrawRect (rect, col);

			// calculate indented rect
			Rect lblRect = new Rect (rect);
			lblRect.x += indentSize * (indent + 1);
			lblRect.width -= indentSize * (indent + 1);

			// toggle expansion state
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

			// input handling
			Event e = Event.current;
			if (lblRect.Contains (e.mousePosition)) {
				hovered = group;
				//int? q = GetCursorQuadrant (rect, e.mousePosition, 0.3f);
				bool below = e.mousePosition.y > (rect.y + rect.height * 0.8f);

				switch (e.type) {
					//case EventType.ContextClick:
					//	e.Use ();
					//	Select (group);
					//	ShowGroupMenu (group);
					//	break;
					case EventType.MouseDown:
						if (e.button == 1) {
							Select (group);
							ShowGroupMenu (group);
						} else if (dragged == null) {
							e.Use ();
							Select (group);
						}
						break;
					case EventType.MouseDrag:
						if (AllowDrag && e.button == 0) {
							Select (null);
							if (dragged == null && group != Asset) {
								e.Use ();
								dragged = group;
							}
						}
						break;
					case EventType.MouseUp:
						if (AllowDrag) {
							//HandleEndDrag (group, groupIndex, q);
							HandleEndDrag (group, groupIndex, below);
							e.Use ();
						}
						break;
				}

				if (dragged != null && dragged != group/* && q.HasValue*/ && below) {

					Rect preview = new Rect (rect);
					preview.y += rect.height * 0.8f;
					preview.height = rect.height * 0.2f;
					EditorGUI.DrawRect (preview, Color.blue);

					//	int _q = q.Value;

					//	float prevHeight = 2f;
					//	float prevOffset = prevHeight * 0.5f;
					//	Rect preview = new Rect (rect);
					//	switch (_q) {
					//		case -1:
					//			preview.y -= prevOffset;
					//			preview.height = prevHeight;
					//			dragPreview = preview;
					//			break;
					//		case 1:
					//			preview.y += preview.height - prevOffset;
					//			preview.height = prevHeight;
					//			dragPreview = preview;
					//			break;
					//		default:
					//			dragPreview = null;
					//			break;
					//	}
				}
			}

			// don't draw children if item is not expanded in the hierarchy
			if (!expanded) {
				return;
			}

			// draw sub-groups
			for (int i = 0; i < group.ChildGroupCount; i++) {
				DrawSettingsGroup (group.GetGroupAt (i), i, ref index, indent + 1);
			}

			// draw settings from current group
			DrawGroupSettings (group, ref index, indent + 1);

		}

		private void DrawGroupSettings (SettingsGroup group, ref int index, int indent) {

			for (int i = 0; i < group.SettingCount; i++) {

				SettingBase setting = group.GetSettingAt (i);
				if (setting == null) {
					continue;
				}

				// only show setting if current filter matches setting properties.
				// during edit-time, CurrentFilter will always be HierarchyFilter.All
				var filter = CurrentFilter;
				if (((filter & HierarchyFilter.Clean) != 0 && !setting.IsDirty) ||
					((filter & HierarchyFilter.Dirty) != 0 && setting.IsDirty) ||
					((filter & HierarchyFilter.Normal) != 0 && !setting.External) ||
					((filter & HierarchyFilter.External) != 0 && setting.External)) {

					index++;
					DrawSetting (setting, indent, index, i);

				}
			}

		}

		private void DrawSetting (SettingBase setting, int indent, int index, int settingIndex) {

			// calculate rect for this hierarchy item
			Rect rect = EditorGUILayout.GetControlRect ();
			rect.x -= 2f;
			rect.width += 4;
			rect.y -= 2f;
			rect.height += 2f;

			// draw coloured background
			Color col = GetHierarchyColor (setting, index, rect.Contains (Event.current.mousePosition));
			EditorGUI.DrawRect (rect, col);

			// calculate indented rect & display label
			float indentValue = (indent + 1) * indentSize + 10f;
			Rect foldRect = new Rect (rect.x + indentValue, rect.y, rect.width - indentValue, rect.height);
			EditorGUI.LabelField (foldRect, $"{setting.Name} ({setting.ValueType.Name})", setting.External ? labelStyleExternal : labelStyleInternal);

			// input handling
			Event e = Event.current;
			if (foldRect.Contains (e.mousePosition)) {
				hovered = setting;

				switch (e.type) {
					case EventType.MouseDown:
						e.Use ();
						Select (setting);
						if (e.button == 1) {
							ShowSettingMenu (setting);
						}
						break;
					//case EventType.ContextClick:
					//	e.Use ();
					//	Select (setting);
					//	ShowSettingMenu (setting);
					//	break;
					case EventType.MouseDrag:
						if (AllowDrag && e.button == 0) {
							Select (null);
							if (dragged == null) {
								e.Use ();
								dragged = setting;
							}
						}
						break;
					case EventType.MouseUp:
						if (AllowDrag) {
							HandleEndDrag (setting, settingIndex);
							e.Use ();
						}
						break;
				}
			}
		}


		// Hierarchy manipulation

		private void MoveGroup (SettingsGroup dragged, SettingsGroup settingsGroup, int groupIndex) {
			settingsGroup?.AddChildGroup (dragged, groupIndex);
		}

		private void MoveSetting (SettingBase dragged, SettingsGroup settingsGroup, int index) {
			settingsGroup.AddSetting (dragged, index);
		}

		private void Select (ScriptableObject sel) {
			if (sel == selected) {
				return;
			}

			// reset GUI focus
			GUI.FocusControl ("");

			// update editor
			if (sel == null) {
				editor = null;
			} else {
				editor = Editor.CreateEditor (sel);
			}

			selected = sel;
			Repaint ();
		}

		private void HandleEndDrag () {
			dragged = null;
		}

		private void HandleEndDrag (ScriptableObject hover, int index, bool below = false) {
			ScriptableObject _dragged = dragged;
			dragged = null;

			if (_dragged == null || hovered == null || Application.isPlaying) {
				return;
			}

			if (_dragged == hover) {
				return;
			}

			SettingsGroup dGroup = _dragged as SettingsGroup;
			SettingsGroup hGroup = hovered as SettingsGroup;
			SettingBase dSetting = _dragged as SettingBase;
			SettingBase hSetting = hovered as SettingBase;

			// dragging group onto group
			if (dGroup != null && hGroup != null) {
				if (below) {
					MoveGroup (dGroup, hGroup.Parent ?? Asset, index);
				} else {
					MoveGroup (dGroup, hGroup, 0);
				}

				// dragging setting onto group
			} else if (dSetting != null && hGroup != null) {
				MoveSetting (dSetting, hGroup, 0);

				// dragging setting onto setting
			} else if (dSetting != null && hSetting != null) {
				MoveSetting (dSetting, hSetting.group, index);

				// dragging group onto setting
			} else if (dGroup != null && dSetting != null) {
				MoveGroup (dGroup, dSetting.group, dSetting.group.ChildGroupCount);

			}
		}


		// Context Menu creation

		private void ShowGroupMenu (SettingsGroup group) {
			GenericMenu gm = new GenericMenu ();

			// only allow changing menus to be shown during edit-time
			if (!Application.isPlaying) {

				PopulateSettingTypeMenu (gm, group);
				//gm.AddItem (new GUIContent ("Add Group"), false, CreateGroupAsChildOfGroup, group);
				PopulateGroupsTypeMenu (gm, group);

				if (group != Asset) {
					gm.AddSeparator ("");
					gm.AddItem (new GUIContent ("Delete Group"), false, DeleteGroup, group);
				}

				gm.AddSeparator ("");
			}

			gm.AddItem (new GUIContent ("Collapse/All"), false, () => {
				Select (Asset);
				expansionState.Clear ();
				expansionState[Asset] = true;
			});
			gm.AddItem (new GUIContent ("Collapse/To This"), false, () => { ExpandToSelection (true); });

			gm.ShowAsContext ();
		}

		private void PopulateSettingTypeMenu (GenericMenu gm, SettingsGroup group, bool prefixItem = true) {
			if (gm == null || group == null) {
				return;
			}

			// load all types inheriting SettingBase<T>
			// result will be cached automatically
			LoadViableTypes ();

			// there are no viable types to create settings from
			if (viableSettingTypes.Length == 0) {
				gm.AddDisabledItem (prefixItem ? new GUIContent ("Add Setting") : new GUIContent ("No Setting types found."));

				// generate sub-menus for each viable type
			} else {
				string prefix = prefixItem ? "Add Setting/" : "";
				foreach (Type t in viableSettingTypes) {
					Type generic = null;
					Type[] generics = t.BaseType.GetGenericArguments ();
					if (generics.Length > 0) {
						generic = generics[0];
					}
					string ns = string.IsNullOrEmpty (t.Namespace) ? "<Global>" : t.Namespace;
					gm.AddItem (
						new GUIContent ($"{prefix}{ns}/{ToEditorSpelling (t.Name)}"),
						false, CreateSettingAsChildOfGroup, new NewSettingData (t, group)
					);
				}
			}
		}

		private void PopulateGroupsTypeMenu (GenericMenu gm, SettingsGroup group, bool prefixItem = true) {
			if (gm == null || group == null) {
				return;
			}

			// load all types inheriting SettingsGroup
			// result will be cached automatically
			LoadViableTypes ();

			string prefix = prefixItem ? "Add Group/" : "";

			gm.AddItem (new GUIContent (prefixItem ? "Add Default Group" : "Default Group"), false, CreateGroupAsChildOfGroup, new NewSettingData (typeof (SettingsGroup), group));

			if (viableGroupTypes.Length > 0) {
				if (!prefixItem) {
					gm.AddSeparator ("");
				}
				foreach (Type t in viableGroupTypes) {
					Type generic = null;
					Type[] generics = t.BaseType.GetGenericArguments ();
					if (generics.Length > 0) {
						generic = generics[0];
					}
					string ns = string.IsNullOrEmpty (t.Namespace) ? "<Global>" : t.Namespace;
					gm.AddItem (
						new GUIContent ($"{prefix}{ns}/{ToEditorSpelling (t.Name)}"),
						false, CreateGroupAsChildOfGroup, new NewSettingData (t, group)
					);
				}
			}
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
				Select (Asset);
				expansionState.Clear ();
				expansionState[Asset] = true;
			});
			gm.AddItem (new GUIContent ("Collapse/To This"), false, () => { ExpandToSelection (true); });

			gm.ShowAsContext ();
		}


		// Creating and deleting Settings & Groups

		private void CreateSettingAsChildOfGroup (object data) {
			if (!(data is NewSettingData d)) {
				return;
			}
			if (d.Group == null) {
				return;
			}

			SettingBase setting = AssetUtility.CreateAsPartOf<SettingBase> (Asset, d.SettingType, s => {
				s.name = "New Setting";
				s.Name = "New Setting";
				s.GUID = Guid.NewGuid ().ToString ();
				s.asset = Asset;
				d.Group.AddSetting (s);
			});

			Select (setting);
			ExpandToSelection (false);
		}

		private void CreateGroupAsChildOfGroup (object group) {
			SettingsGroup parentGroup;
			SettingsGroup newGroup;

			switch (group) {
				case SettingsGroup g:
					parentGroup = g;
					newGroup = AssetUtility.CreateAsPartOf<SettingsGroup> (Asset, g => {
						g.name = "New Group";
						g.Name = "New Group";
						g.GUID = Guid.NewGuid ().ToString ();
						parentGroup.AddChildGroup (g);
					});

					g.AddChildGroup (newGroup);
					Select (newGroup);
					ExpandToSelection (false);
					break;
				case NewSettingData nsd:
					parentGroup = nsd.Group;
					newGroup = AssetUtility.CreateAsPartOf<SettingsGroup> (Asset, nsd.SettingType, g => {
						g.name = "New Group";
						g.Name = "New Group";
						g.GUID = Guid.NewGuid ().ToString ();
						parentGroup.AddChildGroup (g);
					});
					break;
				default:
					return;
			}

			Select (newGroup);
			ExpandToSelection (false);
		}

		private void DuplicateSetting (object setting) {
			if (!(setting is SettingBase set)) {
				return;
			}

			SettingBase newSetting = Instantiate (set);
			newSetting.GUID = Guid.NewGuid ().ToString ();
			newSetting.group = null;
			set.group.AddSetting (newSetting);

			AssetDatabase.AddObjectToAsset (newSetting, Asset);
			DirtyAsset ();
			AssetDatabase.SaveAssets ();
			AssetDatabase.Refresh ();

			Select (newSetting);
		}

		private void DeleteGroup (object group) {
			if (group is SettingsGroup g) {
				if (selected == g) {
					Select (g.Parent);
				}

				for (int i = g.ChildGroupCount - 1; i >= 0; i--) {
					Asset.AddChildGroup (g.GetGroupAt (i));
				}
				for (int i = g.SettingCount - 1; i >= 0; i--) {
					Asset.AddSetting (g.GetSettingAt (i));
				}
				expansionState.Remove (g);
				if (g.Parent != null) {
					g.Parent.RemoveChildGroup (g);
				}
				DestroyImmediate (g, true);
				DirtyAsset ();
				AssetDatabase.Refresh ();
				AssetDatabase.SaveAssets ();
				Repaint ();
			}
		}

		private void DeleteSetting (object setting) {
			if (setting is SettingBase s) {
				s.group.RemoveSetting (s);

				DestroyImmediate (s, true);
				DirtyAsset ();
				AssetDatabase.Refresh ();
				AssetDatabase.SaveAssets ();
				Repaint ();
			}
		}


		// Helper Methods

		public static bool MakeHorizontalDragRect (ref Rect rect, float min, float max, float size = 5f, Color? color = null) {
			Color col = color.HasValue ? color.Value : new Color (0.1f, 0.1f, 0.1f);

			EditorGUI.DrawRect (rect, col);
			Event e = Event.current;

			if (e.type == EventType.Used) {
				return false;
			}

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
					rect.x = Mathf.Clamp (rect.x + e.delta.x, min, max);
					e.Use ();
					return true;
				}
			}

			return false;
		}

		public static float MakeHorizontalDragRect (Rect rect, float min, float max, float size = 5f, Color? color = null) {
			Color col = color.HasValue ? color.Value : new Color (0.1f, 0.1f, 0.1f);

			EditorGUI.DrawRect (rect, col);
			Event e = Event.current;

			if (e.type == EventType.Used) {
				return rect.x;
			}

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
					rect.x = Mathf.Clamp (rect.x + e.delta.x, min, max);
					e.Use ();
				}
			}

			return rect.x;
		}

		private static SettingsAsset FindAsset () {
			string[] guids = AssetDatabase.FindAssets ($"t:{typeof (SettingsAsset).FullName}");
			if (guids.Length > 0) {
				return AssetDatabase.LoadAssetAtPath<SettingsAsset> (AssetDatabase.GUIDToAssetPath (guids[0]));
			}
			return null;
		}

		private void LoadViableTypes () {
			if (viableSettingTypes != null && viableGroupTypes != null) {
				return;
			}

			List<Type> settingTypes = new List<Type> ();
			Type settingBaseType = typeof (SettingBase);

			List<Type> groupTypes = new List<Type> ();
			Type groupBaseType = typeof (SettingsGroup);

			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies ();
			foreach (Assembly asm in assemblies) {

				Type[] asmTypes = asm.GetTypes ();
				foreach (Type t in asmTypes) {

					if (settingBaseType.IsAssignableFrom (t) && !t.IsAbstract) {
						settingTypes.Add (t);
					}
					if (groupBaseType.IsAssignableFrom (t) && !t.IsAbstract && t != groupBaseType && t != typeof (SettingsAsset)) {
						groupTypes.Add (t);
					}

				}
			}

			if (viableSettingTypes == null) {
				settingTypes.Sort (Compare);
				viableSettingTypes = settingTypes.ToArray ();
			}
			if (viableGroupTypes == null) {
				groupTypes.Sort (Compare);
				viableGroupTypes = groupTypes.ToArray ();
			}
		}

		private int Compare (Type a, Type b) {
			return a.FullName.CompareTo (b.FullName);
		}

		private Color GetHierarchyColor (ScriptableObject obj, int index, bool hovered) {
			if (obj == dragged) {
				return hierarchyColorDragged;
			} else if (obj == selected) {
				return hierarchyColorSelected;
			} else if (hovered) {
				return hierarchyColorHover;
			} else {
				return index % 2 == 0 ? hierarchyColorA : hierarchyColorB;
			}
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

		private string ToEditorSpelling (string value) {
			if (value.StartsWith ("<")) {
				value = value.Substring (1);
				value = value.Replace (">k__BackingField", "");
			}

			if (value.Length > 2 && value[1] == '_') {
				value = value.Substring (2);
			}

			value = value.Replace ("_", " ");

			string val = "";
			for (int i = 0; i < value.Length; i++) {
				if (i == 0) {
					val = char.ToUpper (value[0]).ToString ();
					continue;
				}

				val = val + value[i];

				if (i < value.Length - 1) {
					char next = value[i + 1];
					if ((char.IsUpper (next) || char.IsNumber (next)) && !char.IsUpper (value[i]) && value[i] != '.') {
						val = val + " ";
					}
				}
			}

			return val.TrimEnd ('_');
		}

		private void LoadSettingsAssets () {
			List<SettingsAsset> assets = new List<SettingsAsset> (10);
			Type assetType = typeof (SettingsAsset);

			foreach (var guid in AssetDatabase.FindAssets ($"t:{assetType.FullName}")) {
				if (AssetDatabase.LoadAssetAtPath (AssetDatabase.GUIDToAssetPath (guid), assetType) is SettingsAsset a) {
					assets.Add (a);
				}
			}

			allAssets = assets.ToArray ();
		}

		private bool TrySetGuid (SerializedProperty target, string guid, bool isGroup, bool verbose = true) {
			if (Application.isPlaying) {
				if (verbose) {
					Debug.LogError ("Cannot set GUID while game is running.");
				}
				return false;
			}

			if (!Asset.Editor_IsValidGUID (guid, isGroup)) {
				if (verbose) {
					Debug.LogError ("Could not copy Name to GUID. Either this would have resulted in a duplicate GUID, or the value was invalid.");
				}
				return false;
			}

			target.stringValue = guid;
			target.serializedObject.ApplyModifiedPropertiesWithoutUndo ();
			return true;
		}

		private void DirtyAsset () {
			if (asset != null) {
				EditorUtility.SetDirty (asset);
			}
		}

		private string GetGuidFormattedName (FrameworkObject obj, bool includeParentGuid, bool includeParentName, bool format) {
			string name = obj.Name;
			if (includeParentGuid ^ includeParentName) {
				switch (obj) {
					case SettingsGroup group:
						if (group.Parent != null) {
							name = $"{(includeParentName ? group.Parent.Name : group.Parent.GUID)}/{name}";
						}
						break;
					case SettingBase setting:
						name = $"{(includeParentName ? setting.Group.Name : setting.Group.GUID)}/{name}";
						break;
				}
			}

			if (format) {
				name = name.ToLower ().Replace (' ', '-');
			}

			return name;
		}

		private void Restore (SettingsAsset asset) {
			if (asset == null) {
				Debug.LogError ("[Settings Framework] No asset to restore.");
				return;
			}

			Reserialize ();

			if (AllValid (asset)) {
				Debug.Log ("[Settings Framework] (Experimental) Restoration did not find any missing references.");
				return;
			}

			var path = AssetDatabase.GetAssetPath (asset);
			var assets = AssetDatabase.LoadAllAssetsAtPath (path);
			foreach (var obj in assets) {
				if (obj is SettingsGroup group) {
					Debug.Log ($"Trying to restore {group}");
					RestoreGroup (group);
					continue;
				}
				if (obj is SettingBase setting) {
					Debug.Log ($"Trying to restore {setting}");
					RestoreSetting (setting);
					continue;
				}
			}

			AssetDatabase.SaveAssetIfDirty (asset);
			AssetDatabase.Refresh ();
			Debug.Log ($"[Settings Framework] (Experimental) Restoration modified '{path}'.");


			void Reserialize() {
				Debug.Log ("[Settings Framework] (Experimental) Attempting to reserialize asset. (This sometimes is enough to restore references)");

				EditorUtility.SetDirty (asset);
				AssetDatabase.SaveAssetIfDirty (asset);

				asset.groups.Add (null);

				EditorUtility.SetDirty (asset);
				AssetDatabase.SaveAssetIfDirty (asset);

				asset.groups.RemoveAt (asset.groups.Count - 1);

				EditorUtility.SetDirty (asset);
				AssetDatabase.SaveAssetIfDirty (asset);

				AssetDatabase.Refresh ();
			}

			void RestoreGroup (SettingsGroup group) {
				if (group == null) {
					return;
				}

				var ms = MonoScript.FromScriptableObject (group);
				if (ms == null) {
					AssetDatabase.RemoveObjectFromAsset (group);
					EditorUtility.SetDirty (asset);
					DestroyImmediate (group);
					Debug.Log ($"{group} could not be restored and was deleted.");
					return;
				}

				for (int i = group.settings.Count - 1; i >= 0; i--) {
					if (!IsValid(group.settings[i])) {
						group.settings.RemoveAt (i);
					}
				}
			}

			void RestoreSetting (SettingBase setting) {
				if (setting == null) {
					return;
				}

				var ms = MonoScript.FromScriptableObject (setting);
				if (setting.group == null || ms == null) {
					AssetDatabase.RemoveObjectFromAsset (setting);
					DestroyImmediate (setting);
					Debug.Log ($"{setting} could not be restored and was deleted.");
					return;
				}

				if (setting.group.settings.Contains (setting)) {
					return;
				}

				setting.group.settings.Add (setting);
			}

			static bool AllValid (SettingsGroup group) {
				if (group == null) {
					return false;
				}
				for (int i = 0; i < group.SettingCount; i++) {
					if (IsValid(group.settings[i])) {
						return false;
					}
				}
				for (int i = 0; i < (group.groups?.Count ?? 0); i++) {
					if (!AllValid (group.groups[i])) {
						return false;
					}
				}
				return true;
			}

			static bool IsValid (ScriptableObject so) {
				return so != null && MonoScript.FromScriptableObject (so) != null;
			}
		}


		// Hierarchy Serialization

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