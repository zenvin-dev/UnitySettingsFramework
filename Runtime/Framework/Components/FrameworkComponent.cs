using UnityEngine;

namespace Zenvin.Settings.Framework.Components {
	public abstract class FrameworkComponent : FrameworkObject {
		[SerializeField, HideInInspector] private ComposedFrameworkObject baseContainer;

		public ComposedFrameworkObject BaseContainer { get => baseContainer; internal set => baseContainer = value; }


		public virtual void OnInitialize () { }

		public virtual void OnCreateWithValues (StringValuePair[] values) { }


		internal virtual bool IsValidContainer (ComposedFrameworkObject container) {
			return container != null;
		}
	}
}
