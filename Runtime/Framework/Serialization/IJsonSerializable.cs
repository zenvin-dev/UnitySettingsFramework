using Newtonsoft.Json.Linq;

namespace Zenvin.Settings.Framework.Serialization {
	public interface IJsonSerializable {
		void OnDeserializeJson (JObject data);
		void OnSerializeJson (JObject data);
	}
}