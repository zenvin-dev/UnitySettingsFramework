namespace Zenvin.Settings.Framework.Components {
	public abstract class SettingComponent<TSettingType, TValueType> : TypedFrameworkComponent<TSettingType> where TSettingType : SettingBase<TValueType> {
		internal override bool IsValidContainer (ComposedFrameworkObject container) {
			return container != null && container is TSettingType;
		}


		protected virtual void OnValueChanging (in ValueChangingArgs<TValueType> args) { }

		protected virtual void OnValueChanged (SettingBase.ValueChangeMode mode) { }
	}
}
