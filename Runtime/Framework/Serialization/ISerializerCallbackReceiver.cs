namespace Zenvin.Settings.Framework.Serialization {
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
}