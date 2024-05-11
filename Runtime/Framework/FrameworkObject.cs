using UnityEngine;

namespace Zenvin.Settings.Framework {
	/// <summary>
	/// Base class for various <see cref="ScriptableObject"/>s inside the Settings Framework.
	/// </summary>
	public abstract class FrameworkObject : ScriptableObject {

		[SerializeField, HideInInspector] private string guid = null;
		[SerializeField, HideInInspector] private bool external = false;


		/// <summary> Unique ID of the object. </summary>
		public string GUID {
			get => guid;
			internal set => guid = value;
		}

		/// <summary> Whether the object was imported during runtime. </summary>
		public bool External {
			get => external;
			internal set => external = value;
		}
	}

	/// <summary>
	/// An class extending the <see cref="FrameworkObject"/> to include human-readable labels.
	/// </summary>
	public abstract class NamedFrameworkObject : FrameworkObject {

		[SerializeField, HideInInspector] private string label = string.Empty;
		[SerializeField, HideInInspector] private string labelLocalizationKey = string.Empty;

		[SerializeField, HideInInspector] private string description = string.Empty;
		[SerializeField, HideInInspector] private string descriptionLocalizationKey = string.Empty;


		/// <summary> The name assigned to the object. </summary>
		public string Name {
			get => label;
			internal set => label = value;
		}

		/// <summary> And arbitrary value that can be used to localize the object's <see cref="Name"/>. </summary>
		public string NameLocalizationKey {
			get => labelLocalizationKey;
			internal set => labelLocalizationKey = value;
		}

		/// <summary> A description of the object. </summary>
		public string Description {
			get => description;
			internal set => description = value;
		}

		/// <summary> And arbitrary value that can be used to localize the object's <see cref="Description"/>. </summary>
		public string DescriptionLocalizationKey {
			get => descriptionLocalizationKey;
			internal set => descriptionLocalizationKey = value;
		}
	}

	/// <summary>
	/// A class extending <see cref="NamedFrameworkObject"/> to contain visibility information that can be used when displaying the object(s) in a hierarchy.
	/// </summary>
	public abstract class VisualHierarchyObject : NamedFrameworkObject {
		/// <summary>
		/// Delegate type for the <see cref="VisibilityChanged"/> event.
		/// </summary>
		public delegate void VisibilityChangedEvt ();

		/// <summary> Invoked whenever <see cref="Visibility"/> changes. </summary>
		public event VisibilityChangedEvt VisibilityChanged;

		private SettingVisibility currentVisibility = SettingVisibility.Visible;


		/// <summary> The current visible state of the object. </summary>
		public SettingVisibility Visibility {
			get => currentVisibility;
			set => SetVisibility (value);
		}


		/// <summary>
		/// Sets the object's visible state.
		/// </summary>
		public void SetVisibility (SettingVisibility visibility) {
			SetVisibility (visibility, true);
		}

		/// <summary>
		/// Sets the object's visible state without raising any events. Should only be used for initialization.
		/// </summary>
		public void SetVisibilityWithoutNotify (SettingVisibility visibility) {
			if (visibility == currentVisibility) {
				return;
			}
			currentVisibility = visibility;
		}

		/// <summary>
		/// If implemented in an inheriting type, should return the object's visibility.
		/// </summary>
		/// <returns> The object's visibility, taking all parent objects into consideration. </returns>
		public abstract SettingVisibility GetVisibilityInHierarchy ();


		internal void SetVisibility (SettingVisibility visibility, bool propagateMethod) {
			if (visibility == currentVisibility) {
				return;
			}
			currentVisibility = visibility;
			if (propagateMethod) {
				OnSetVisibilityInternal (visibility);
			}
			VisibilityChanged?.Invoke ();
		}

		private protected virtual void OnSetVisibilityInternal (SettingVisibility visibility) { }
	}
}