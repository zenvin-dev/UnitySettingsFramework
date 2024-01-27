namespace Zenvin.Settings.Framework.Serialization {
	/// <summary>
	/// Interface to allow a <see cref="SettingBase"/> to be serialized using a <see cref="ISerializer{T}"/>.
	/// </summary>
	/// <typeparam name="T"> The type as which the Setting's value will be serialized. </typeparam>
	/// <remarks>
	/// Any <see cref="SettingBase"/> can implement multiple overloads of this interface.<br></br>
	/// Which one is used depends on the compatibility of the <see cref="ISerializer{T}"/> used for serialization of the Setting.
	/// </remarks>
	public interface ISerializable<T> where T : class, new() {
		/// <summary>
		/// Called during serialization to wrap the Setting's arbitrary value into a value that a <see cref="ISerializer{T}"/> can work with.
		/// </summary>
		/// <param name="value">
		/// An instance of the wrapper type <typeparamref name="T"/>.<br></br>
		/// Will never be <see langword="null"/>.
		/// </param>
		void OnSerialize (T value);
		/// <summary>
		/// Called during deserialization to extract the Setting's arbitrary value from a given wrapper of type <typeparamref name="T"/>.
		/// </summary>
		/// <param name="value">
		/// An instance of the wrapper type <typeparamref name="T"/>.<br></br>
		/// Will never be <see langword="null"/>.
		/// </param>
		void OnDeserialize (T value);
	}
}