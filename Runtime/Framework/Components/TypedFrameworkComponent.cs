namespace Zenvin.Settings.Framework.Components {
	public abstract class TypedFrameworkComponent<T> : FrameworkComponent where T : ComposedFrameworkObject {
		private T container;

		public T Container {
			get {
				if (container != null)
					return container;

				container = BaseContainer as T;
				return container;
			}
		}
	}
}
