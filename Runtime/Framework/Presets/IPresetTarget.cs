using UnityEngine;

namespace Zenvin.Settings.Framework.Presets {
	/// <summary>
	/// Interface to allow a <see cref="SettingBase"/> apply values provided by a preset object. 
	/// In conjunction with <see cref="PresetSettingsGroup{T}"/>s, this can be used to change multiple Settings at once.
	/// </summary>
	/// <typeparam name="TPreset"> The type that the preset value must have. </typeparam>
	/// <remarks>
	/// Any <see cref="SettingBase"/> can implement multiple overloads of this interface.<br></br>
	/// Which one is used depends on the compatibility of the <see cref="PresetSettingsGroup{T}"/> ordering application of the preset's values.
	/// </remarks>
	public interface IPresetTarget<TPreset> where TPreset : ScriptableObject {
		void OnSetPresetValue (TPreset preset);
		void OnApplyPresetValue (TPreset preset);
		bool IsPresetActive (TPreset preset);
	}
}
