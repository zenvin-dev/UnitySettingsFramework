using System.Collections.Generic;
using UnityEngine;

namespace Zenvin.Settings.Framework.Presets {
	/// <summary>
	/// Base class for a <see cref="SettingsGroup"/> that can apply preset values based on a <see cref="ScriptableObject"/> value holder to its nested Settings and Groups.
	/// </summary>
	public abstract class PresetSettingsGroup : SettingsGroup {

		/// <summary>
		/// Used to determine how preset values are applied.
		/// </summary>
		public enum PresetApplicationMode {
			/// <summary> Values will only be set (<see cref="IPresetTarget{TPreset}.OnSetPresetValue(TPreset)"/>), but not applied. </summary>
			SetOnly,
			/// <summary> Values will both be set and applied at the same time. </summary>
			SetAndApply,
			/// <summary> Values will be set first. Once all values have been set, any changed Settings will be applied. </summary>
			SetThenApply,
		}


		[SerializeField, Tooltip ("How the preset values are applied.")]
		private PresetApplicationMode applicationMode = PresetApplicationMode.SetOnly;

		[SerializeField, Tooltip ("Determines whether preset application continues on nested preset groups.")]
		private bool stopOnNestedPresetGroups;

		[SerializeField, Min (0), Tooltip ("The maximum nesting depth at which the preset should be applied. Set to 0 to traverse all children of the group.")]
		private int maxDepth = 1;


		/// <summary> How the preset values are applied. </summary>
		public PresetApplicationMode ApplicationMode => applicationMode;
		/// <summary> Determines whether preset application continues on nested preset groups. </summary>
		public bool StopOnNestedPresetGroups => stopOnNestedPresetGroups;
		/// <summary> The maximum nesting depth at which the preset should be applied. Set to 0 to traverse all children of the group. </summary>
		public int MaxDepth => maxDepth;
	}

	/// <inheritdoc/>
	/// <typeparam name="TPreset"> The type of object holding preset values. </typeparam>
	public abstract class PresetSettingsGroup<TPreset> : PresetSettingsGroup where TPreset : ScriptableObject {

		[SerializeField, Tooltip ("The preset object from which preset values can/will be sourced upon application.")]
		private TPreset[] presets;
		[SerializeField, Tooltip ("If true, the Group will attempt to load any available preset objects from the graphics settings on startup.")]
		private bool findPresets = true;


		/// <summary> If true, the Group will attempt to load any available preset objects from the graphics settings on startup. </summary>
		public bool FindPresets => findPresets;


		/// <summary>
		/// Applies the preset values to all viable child Settings of the <see cref="PresetSettingsGroup"/>.
		/// </summary>
		public bool ApplyPreset (int index) {
			if (presets == null)
				return false;
			if (index < 0 || index >= presets.Length)
				return false;

			var preset = presets[index];
			if (preset == null)
				return false;

			var targets = ApplicationMode == PresetApplicationMode.SetThenApply ? new List<IPresetTarget<TPreset>> () : null;
			var apply = ApplicationMode != PresetApplicationMode.SetOnly;

			ApplyRecursively (preset, apply, this, 0, MaxDepth, targets);

			if (targets != null) {
				for (int i = 0; i < targets.Count; i++) {
					targets[i].OnApplyPresetValue (preset);
				}
			}
			return true;
		}

		/// <summary>
		/// Attempts to figure out the best of all available presets, using <see cref="IEvaluatablePresetTarget{TPreset}"/> implementations in child Settings.
		/// </summary>
		/// <returns> The index of the best preset in this Group, or -1 if there were no Settings that could evaluate a preset. </returns>
		public int GetBestPresetIndex () {
			if (presets == null || presets.Length == 0)
				return -1;

			var bestIndex = 0;
			var bestScore = 0;
			var targetList = new List<IEvaluatablePresetTarget<TPreset>> ();

			GetEvaluatableTargetsRecursively (this, targetList, 0, MaxDepth);
			if (targetList.Count == 0)
				return -1;

			for (int i = 0; i < presets.Length; i++) {
				var score = 0;
				for (int j = 0; j < targetList.Count; j++) {
					score += targetList[j].EvaluatePresetViability (presets[i]);
				}

				if (score > bestScore) {
					bestScore = score;
					bestIndex = i;
				}
			}

			return bestIndex;
		}

		/// <summary>
		/// Checks all viable child Settings of the <see cref="PresetSettingsGroup"/> to determine which preset is active (if any).
		/// </summary>
		protected int GetActivePresetIndex () {
			if (presets == null)
				return -1;

			for (int i = 0; i < presets.Length; i++) {
				if (CheckRecursively (presets[i], this, 0, MaxDepth)) {
					return i;
				}
			}

			return -1;
		}

		/// <summary>
		/// If implemented, used to find available presets during initialization, if <see cref="FindPresets"/> is <see langword="true"/>.
		/// </summary>
		protected virtual TPreset[] GetAvailablePresets () => null;

		/// <inheritdoc/>
		protected internal override void OnBeforeInitialize () {
			if (findPresets) {
				presets = GetAvailablePresets ();
			}
		}


		private void ApplyRecursively (TPreset preset, bool apply, SettingsGroup group, int depth, int maxDepth, List<IPresetTarget<TPreset>> targets) {
			if (group == null)
				return;

			ApplyToSettings (preset, apply, group, targets);

			if (maxDepth <= 0 || depth < maxDepth) {
				ApplyToGroups (preset, apply, depth, maxDepth, targets);
			}
		}

		private void ApplyToSettings (TPreset preset, bool apply, SettingsGroup group, List<IPresetTarget<TPreset>> targets) {
			foreach (var setting in group.IterateSettings (true)) {
				if (setting is IPresetTarget<TPreset> target) {
					target.OnSetPresetValue (preset);
					if (apply) {
						if (targets == null) {
							target.OnApplyPresetValue (preset);
						} else {
							targets.Add (target);
						}
					}
				}
			}
		}

		private void ApplyToGroups (TPreset preset, bool apply, int depth, int maxDepth, List<IPresetTarget<TPreset>> targets) {
			foreach (var childGroup in IterateGroups (true)) {
				if (StopOnNestedPresetGroups && childGroup is PresetSettingsGroup) {
					continue;
				}

				ApplyRecursively (preset, apply, childGroup, depth + 1, maxDepth, targets);
			}
		}

		private bool CheckRecursively (TPreset preset, SettingsGroup group, int depth, int maxDepth) {
			if (group == null)
				return true;

			foreach (var setting in group.IterateSettings (true)) {
				if (setting is IPresetTarget<TPreset> target && !target.IsPresetActive (preset)) {
					return false;
				}
			}

			if (maxDepth <= 0 || depth < maxDepth) {
				foreach (var childGroup in group.IterateGroups (true)) {
					if (StopOnNestedPresetGroups && childGroup is PresetSettingsGroup) {
						continue;
					}
					if (!CheckRecursively (preset, childGroup, depth + 1, maxDepth)) {
						return false;
					}
				}
			}
			return true;
		}

		private void GetEvaluatableTargetsRecursively (SettingsGroup group, List<IEvaluatablePresetTarget<TPreset>> list, int depth, int maxDepth) {
			if (group == null)
				return;

			foreach (var setting in group.IterateSettings (true)) {
				if (setting is IEvaluatablePresetTarget<TPreset> target) {
					list.Add (target);
				}
			}

			if (maxDepth <= 0 || depth < maxDepth) {
				foreach (var childGroup in group.IterateGroups (true)) {
					if (StopOnNestedPresetGroups && childGroup is PresetSettingsGroup) {
						continue;
					}
					GetEvaluatableTargetsRecursively (childGroup, list, depth + 1, maxDepth);
				}
			}
		}
	}
}
