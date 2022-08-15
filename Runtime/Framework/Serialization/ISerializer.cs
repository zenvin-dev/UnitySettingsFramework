using System.Collections.Generic;

namespace Zenvin.Settings.Framework.Serialization {
	/// <summary>
	/// Interface for serializing a <see cref="SettingBase{T}"/>'s value in a generic object.
	/// </summary>
	/// <typeparam name="T">The type of object that handles storing a single <see cref="SettingBase{T}"/>'s value.</typeparam>
	public interface ISerializer<T> where T : class, new() {
		/// <summary>
		/// Called once for every <see cref="SettingBase{T}"/> that should be serialized.
		/// </summary>
		/// <param name="guid">The GUID of the <see cref="SettingBase"/> that needs saving with the current call.</param>
		/// <param name="value">The value returned by the <see cref="SettingBase"/>'s implementation of <see cref="ISerializable{T}.OnSerialize(T)"/>.</param>
		void Serialize (string guid, T value);
		/// <summary>
		/// Called during deserialization. Should return all saved GUIDs, along with the associated data.
		/// </summary>
		IEnumerable<KeyValuePair<string, T>> GetSerializedData ();
	}
}