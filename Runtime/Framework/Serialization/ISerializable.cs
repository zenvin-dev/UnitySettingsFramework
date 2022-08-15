namespace Zenvin.Settings.Framework.Serialization {
	/// <summary>
	/// Interface to allow a <see cref="SettingBase"/> to be serialized using a <see cref="ISerializer{T}"/>.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ISerializable<T> where T : class, new() {
		void OnSerialize (T value);
		void OnDeserialize (T value);
	}
}