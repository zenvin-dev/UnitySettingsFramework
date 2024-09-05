using System.Collections.Generic;
using UnityEngine;
using Zenvin.Settings.Framework.Components;

namespace Zenvin.Settings.Framework {
	public abstract class ComposedFrameworkObject : VisualHierarchyObject {

		[SerializeField, HideInInspector] private protected ComponentCollection components = new ComponentCollection ();


		public bool TryGetComponent<T> (out T component) {
			component = default;

			if (components == null)
				return false;

			for (int i = 0; i < components.Count; i++) {
				if (components[i] is T typedComponent) {
					component = typedComponent;
					return true;
				}
			}
			return false;
		}

		public T GetComponent<T> () {
			return TryGetComponent (out T component) ? component : default;
		}

		public IEnumerable<T> GetComponents<T> () {
			for (int i = 0; i < components.Count; i++) {
				if (components[i] is T typedComponent) {
					yield return typedComponent;
				}
			}
		}


		internal bool TryAddComponentNoContainerCheck (FrameworkComponent component) {
			return components.Add (this, component, false);
		}

		internal bool TryAddComponent (FrameworkComponent component) {
			return components.Add (this, component, true);
		}

		internal bool RemoveComponent (FrameworkComponent component) {
			return components.Remove (this, component);
		}

		internal void InitializeComponents () {
			for (int i = 0; i < components.Count; i++) {
				if (components[i] != null) {
					components[i].OnInitialize ();
				}
			}
		}
	}
}
