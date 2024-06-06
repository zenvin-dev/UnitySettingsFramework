#if UNITY_2023_1_OR_NEWER
using System.Collections.Generic;
using UnityEngine;

namespace Zenvin.Settings.Framework.Serialization {
	/// <summary>
	/// Interface for asynchronously serializing a <see cref="SettingBase{T}"/>'s value in a generic object.
	/// </summary>
	/// <typeparam name="T">The type of object that handles storing a single <see cref="SettingBase{T}"/>'s value.</typeparam>
	public interface IAsyncSerializer<T> where T : class, new() {
		/// <inheritdoc cref="ISerializer{T}.Serialize(string, T)"/>
		Awaitable SerializeAsync (string guid, T value);
		/// <inheritdoc cref="ISerializer{T}.GetSerializedData()" />
		Awaitable<IEnumerator<KeyValuePair<string, T>>> GetSerializedDataAsync ();
	}
}
#endif
