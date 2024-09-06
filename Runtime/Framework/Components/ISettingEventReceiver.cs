namespace Zenvin.Settings.Framework.Components {
	/// <summary>
	/// Interface for extending <see cref="FrameworkComponent"/>s that attach to <see cref="SettingBase"/>s, 
	/// in order to allow reacting to changes in the Settings' values.
	/// </summary>
	public interface ISettingEventReceiver {
		/// <summary>
		/// Called while the <paramref name="setting"/>'s value is being changed, before the change is actually applied.
		/// </summary>
		/// <remarks>
		/// This method will <b>not</b> be called, if another <see cref="ISettingEventReceiver{TValue}.OnValueChanging(ValueChangingArgs{TValue})"/> 
		/// implementation on the same <see cref="SettingBase"/> prevented the change.
		/// </remarks>
		/// <param name="setting"> The Setting whose value is changing. </param>
		/// <param name="mode"> The way the Setting is changing. </param>
		void OnValueChanging(SettingBase setting, SettingBase.ValueChangeMode mode);
		/// <summary>
		/// Called after the <paramref name="setting"/>'s value was changed.
		/// </summary>
		/// <remarks>
		/// This method will be called <b>after</b> the <paramref name="setting"/>'s own 
		/// <see cref="SettingBase{T}.OnValueChanged(SettingBase.ValueChangeMode)"/> and <see cref="SettingBase{T}.ValueChanged"/>.
		/// </remarks>
		/// <param name="setting"> The Setting whose value was changed. </param>
		/// <param name="mode"> The way the Setting was changed. </param>
		void OnValueChanged(SettingBase setting, SettingBase.ValueChangeMode mode);
	}

	/// <inheritdoc cref="ISettingEventReceiver"/>
	public interface ISettingEventReceiver<TValue> {
		/// <summary>
		/// Called while the container <see cref="SettingBase{T}"/>'s value is changing (before the change actually affects the Setting). <br></br>
		/// Allows preventing the change through <see cref="ValueChangingArgs{TValue}.PreventChange"/>.
		/// </summary>
		/// <remarks>
		/// Changes <b>can not be prevented</b> during <c>Initialize</c>, <c>Deserialize</c> or <c>Notify</c> operations! <br></br>
		/// The method will not be invoked at all during <c>Prevent</c> operations.
		/// </remarks>
		/// <param name="args"> Information about the <see cref="SettingBase{T}"/> and value being changed. </param>
		void OnValueChanging (ValueChangingArgs<TValue> args);
	}
}
