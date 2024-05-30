using FMOD;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using Zenvin.Settings.Framework;
using Debug = UnityEngine.Debug;

namespace Zenvin.Settings.Samples {
	public class VolumeSettingFMOD : FloatSetting {

		public const float MinValue = 0f;
		public const float MaxValue = 1f;

		private Bus? targetBus;

		[SerializeField, Tooltip ("The name of the target bus. Must start with 'bus:/'"), BankRef]
		private string targetBusName = "evt:/";
		[SerializeField, Tooltip ("Whether to apply the Setting's value to the AudioMixer any time the value is changed, or wait for the Setting's value to be applied.")]
		private bool changeOnSet = true;
		[SerializeField, Tooltip ("Will log an error to the console if no bus was found for the given name.")]
		private bool missingBusError = true;
		[SerializeField, Tooltip ("Will log an error to the console if setting the volume on the target bus failed.")]
		private bool setVolumeFailedError = true;


		public float TargetValue => changeOnSet ? CachedValue : CurrentValue;
		public Bus? TargetBus => targetBus;


		/// <inheritdoc/>
		protected override void ProcessValue (ref float value) {
			value = Mathf.Clamp01 (value);
		}

		/// <inheritdoc/>
		protected override void OnValueChanged (ValueChangeMode mode) {
			if (string.IsNullOrEmpty (targetBusName))
				return;
			if (!TryGetBus ())
				return;

			if (changeOnSet || mode == ValueChangeMode.Apply || mode == ValueChangeMode.Initialize || mode == ValueChangeMode.Deserialize) {
				var result = targetBus.Value.setVolume (TargetValue);
				if(setVolumeFailedError && result != RESULT.OK)
					Debug.LogError ($"Setting volume of bus '{targetBusName}' failed: {result}");
			}
		}

		private bool TryGetBus () {
			if (targetBus.HasValue)
				return true;
			if (string.IsNullOrWhiteSpace (targetBusName))
				return false;

			try {
				targetBus = RuntimeManager.GetBus (targetBusName);
				return true;
			} catch {
				if (missingBusError)
					Debug.LogError ($"[FMOD Volume Setting] Could not find FMOD Bus with name '{targetBusName}'.", this);

				return false;
			}
		}
	}
}