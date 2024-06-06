#if UNITY_2023_1_OR_NEWER
using UnityEngine;

namespace Zenvin.Settings.Framework.Serialization {
	/// <summary>
	/// Interface for hooking into the Setting serialization process.<br></br>
	/// Meant to be attached to types that already implement <see cref="IAsyncSerializer{T}"/>.
	/// </summary>
	public interface IAsyncSerializerCallbackReceiver {
		/// <inheritdoc cref="ISerializerCallbackReceiver.InitializeSerialization"/>
		Awaitable InitializeSerializationAsync ();
		/// <inheritdoc cref="ISerializerCallbackReceiver.FinalizeSerialization"/>
		Awaitable FinalizeSerializationAsync ();
		/// <inheritdoc cref="ISerializerCallbackReceiver.InitializeDeserialization"/>
		Awaitable InitializeDeserializationAsync ();
		/// <inheritdoc cref="ISerializerCallbackReceiver.FinalizeDeserialization"/>
		Awaitable FinalizeDeserializationAsync ();
	}
}
#endif
