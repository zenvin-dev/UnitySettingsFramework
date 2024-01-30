using System;

namespace Zenvin.Settings {
	/// <summary>
	/// A class representing a single key-value-pair of two strings in a way that can be serialized by Unity.
	/// </summary>
	[Serializable]
	public class StringValuePair {
		/// <summary> And arbitrary string key. </summary>
		public string Key;
		/// <summary> And arbitrary string value. </summary>
		public string Value;
	}
}