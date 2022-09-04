namespace Zenvin.Settings.Framework {
	/// <summary>
	/// Visiblity state. Intended to be implemented by the user interface.
	/// </summary>
	public enum SettingVisibility {
		/// <summary> The object is visible. </summary>
		Visible = 0,
		/// <summary> The object is visible, but disabled. </summary>
		Disabled = 1,
		/// <summary> The object is not visible. </summary>
		Hidden = 2,
	}
}