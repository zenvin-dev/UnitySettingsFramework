namespace Zenvin.Settings.Framework.Serialization {
	public interface IJsonSerializable {
		void OnDeserializeJson (string data);
		string OnSerializeJson ();
	}
}