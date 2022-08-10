namespace Zenvin.Settings.Framework {
	public interface IJsonSerializable {
		void OnDeserializeJson (string data);
		string OnSerializeJson ();
	}
}