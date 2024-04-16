using UnityEngine;

namespace Zenvin.Settings.Framework.Presets {
	/// <summary>
	/// An extension of <see cref="IPresetTarget{TPreset}"/> that allows for evaluating how viable a given preset is. <br></br>
	/// </summary>
	/// <example>
	/// This could, for example, be useful for determining ideal graphics settings.
	/// </example>
	/// <typeparam name="TPreset"></typeparam>
	public interface IEvaluatablePresetTarget<TPreset> : IPresetTarget<TPreset> where TPreset : ScriptableObject {
		/// <summary>
		/// If implemented, assigns a score to a given preset to determine how viable the preset is (in comparison with others of its kind).
		/// </summary>
		/// <param name="preset"> The preset to evaluate. </param>
		/// <returns> The implementing Setting's score for the given preset. A preset's total score determines its ultimate viability. </returns>
		int EvaluatePresetViability (TPreset preset);
	}
}
