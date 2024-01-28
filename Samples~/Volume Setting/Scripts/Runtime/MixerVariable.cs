using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Zenvin.Settings.Samples {
	/// <summary>
	/// Wrapper for an exposed parameter of an <see cref="AudioMixer"/>.
	/// </summary>
	[Serializable]
	public sealed class MixerVariable {
		[SerializeField] private AudioMixer mixer;
		[SerializeField] private string variable;


		public bool TryGetValue (out float value) {
			if (mixer == null || string.IsNullOrWhiteSpace (variable)) {
				value = 0f;
				return false;
			}
			return mixer.GetFloat (variable, out value);
		}

		public bool SetValue (float value) {
			if (mixer == null || string.IsNullOrWhiteSpace (variable)) {
				return false;
			}
			return mixer.SetFloat (variable, value);
		}


		public override string ToString () {
			return $"AudioMixer Target '{(mixer == null ? "<null>" : mixer.name)}' -> '{variable}'";
		}
	}
}
