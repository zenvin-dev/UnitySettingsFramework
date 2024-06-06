#if UNITY_2023_1_OR_NEWER
using UnityEngine;
using Zenvin.Settings.Framework.Serialization;

namespace Zenvin.Settings.Framework {
	public partial class SettingsAsset {
		/// <summary>
		/// Asynchronously serializes all Settings to the provided <see cref="IAsyncSerializer{T}"/>.<br></br>
		/// Returns <see langword="true"/> if serialization was successful.<br></br>
		/// The <see cref="SettingsAsset"/> needs to be initialized before serialization can take place.
		/// </summary>
		/// <remarks>
		/// Support for Awaitables was introduced in <see href="https://docs.unity3d.com/2023.2/Documentation/Manual/AwaitSupport.html">Unity 2023.1</see>.<br></br>
		/// This method will not be available in previous versions.
		/// </remarks>
		/// <typeparam name="T">The type of object used to manage serialized data.</typeparam>
		/// <param name="serializer">The serializer used to serialize data.</param>
		/// <param name="filter">Filter for deciding which settings should be considered for serialization. Leave at <see langword="null"/> to not filter any.</param>
		public async Awaitable<bool> SerializeSettingsAsync<T> (IAsyncSerializer<T> serializer, SettingBaseFilter filter = null) where T : class, new() {
			if (!Initialized || serializer == null)
				return false;

			Log ($"Serializing Settings using '{serializer.GetType ()}' (Target type '{typeof (T)}', Filtered: {filter != null})");

			var cbRec = serializer as ISerializerCallbackReceiver;
			cbRec?.InitializeSerialization ();

			var asyncCbRec = serializer as IAsyncSerializerCallbackReceiver;
			await asyncCbRec.InitializeSerializationAsync ();

			foreach (SettingBase setting in settingsDict.Values) {
				if (filter != null && !filter (setting)) {
					continue;
				}

				if (setting is ISerializable<T> serializable) {
					T obj = new T ();
					serializable.OnSerialize (obj);
					await serializer.SerializeAsync (setting.GUID, obj);
				}
			}

			cbRec?.FinalizeSerialization ();
			asyncCbRec?.FinalizeSerializationAsync ();
			return true;
		}

		/// <summary>
		/// Asynchronously deserializes Setting values from the provided <see cref="IAsyncSerializer{T}"/>.<br></br>
		/// Returns <see langword="true"/> if deserialization was successful.<br></br>
		/// The <see cref="SettingsAsset"/> needs to be initialized before deserialization can take place.
		/// <see cref="SettingBase.ApplyValue"/> will automatically be called on any successfully deserialized Setting.<br></br>
		/// </summary>
		/// <remarks>
		/// Support for Awaitables was introduced in <see href="https://docs.unity3d.com/2023.2/Documentation/Manual/AwaitSupport.html">Unity 2023.1</see>.<br></br>
		/// This method will not be available in previous versions.
		/// </remarks>
		/// <typeparam name="T">The type of object used to manage deserialize data.</typeparam>
		/// <param name="serializer">The serializer used to deserialize data.</param>
		/// <param name="filter">Filter for deciding which settings should be considered for deserialization. Leave at <see langword="null"/> to not filter any.</param>
		public async Awaitable<bool> DeserializeSettingsAsync<T> (IAsyncSerializer<T> serializer, SettingBaseFilter filter = null) where T : class, new() {
			if (!Initialized || serializer == null) {
				return false;
			}

			Log ($"Deserializing Settings using '{serializer.GetType ()}' (Target type '{typeof (T)}', Filtered: {filter != null})");

			var cbRec = serializer as ISerializerCallbackReceiver;
			cbRec?.InitializeDeserialization ();

			var asyncCbRec = serializer as IAsyncSerializerCallbackReceiver;
			await asyncCbRec.InitializeDeserializationAsync ();

			var enumerator = await serializer.GetSerializedDataAsync ();
			if (enumerator != null) {
				while (enumerator.MoveNext ()) {
					var data = enumerator.Current;

					if (settingsDict.TryGetValue (data.Key, out SettingBase setting) && (filter == null || filter (setting)) && setting is ISerializable<T> serializable) {
						serializable.OnDeserialize (data.Value);
						setting.ApplyValue ();
						setting.OnAfterDeserialize ();
					}
				}
			}


			cbRec?.FinalizeDeserialization ();
			asyncCbRec?.FinalizeDeserializationAsync ();
			return true;
		}
	}
}
#endif
