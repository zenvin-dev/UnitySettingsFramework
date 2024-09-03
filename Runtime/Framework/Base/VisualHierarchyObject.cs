namespace Zenvin.Settings.Framework {
	/// <summary>
	/// A class extending <see cref="NamedFrameworkObject"/> to contain visibility information that can be used when displaying the object(s) in a hierarchy.
	/// </summary>
	public abstract class VisualHierarchyObject : NamedFrameworkObject {
		/// <summary>
		/// Delegate type for the <see cref="VisibilityChanged"/> event.
		/// </summary>
		public delegate void VisibilityChangedEvt ();

		/// <summary> 
		/// Invoked whenever <see cref="Visibility"/> changes. 
		/// </summary>
		public event VisibilityChangedEvt VisibilityChanged;

		private SettingVisibility currentVisibility = SettingVisibility.Visible;


		/// <summary> 
		/// The current visible state of the object. 
		/// </summary>
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
