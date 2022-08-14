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

	/// <summary>
	/// Interface for hooking into the Setting serialization process.<br></br>
	/// Meant to be attached to types that already implement <see cref="ISerializer{T}"/>.
	/// </summary>
	public interface ISerializerCallbackReceiver {
		/// <summary>
		/// Called before any Settings are serialized.
		/// </summary>
		void InitializeSerialization ();
		/// <summary>
		/// Called after all Settings have been serialized.
		/// </summary>
		void FinalizeSerialization ();
		/// <summary>
		/// Called before any Settings are deserialized.
		/// </summary>
		void InitializeDeserialization ();
		/// <summary>
		/// Called after all Settings have been deserialized.
		/// </summary>
		void FinalizeDeserialization ();
	}

	/// <summary>
	/// Interface to allow a <see cref="SettingBase"/> to be serialized using a <see cref="ISerializer{T}"/>.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ISerializable<T> where T : class, new() {
		void OnSerialize (T value);
		void OnDeserialize (T value);
	}
}