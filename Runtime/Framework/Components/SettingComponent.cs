namespace Zenvin.Settings.Framework.Components {
	public abstract class SettingComponent<TSettingType, TValueType> : TypedFrameworkComponent<TSettingType>, ISettingEventReceiver<TValueType> where TSettingType : SettingBase<TValueType> {
		internal override bool IsValidContainer (ComposedFrameworkObject container) {
			return container != null && container is TSettingType;
		}

		/// <inheritdoc/>
		public virtual void OnValueChanging (ValueChangingArgs<TValueType> args) { }

		/// <inheritdoc/>
		public virtual void OnValueChanged (SettingBase.ValueChangeMode mode) { }
	}
}
