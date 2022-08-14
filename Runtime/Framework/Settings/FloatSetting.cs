using Newtonsoft.Json.Linq;
using Zenvin.Settings.Framework.Serialization;

namespace Zenvin.Settings.Framework {
	public class FloatSetting : SettingBase<float>, ISerializable<JObject>, ISerializable<ValuePacket> {

		void ISerializable<JObject>.OnDeserialize (JObject value) {
			if (value.TryGetValue ("value", out JToken token)) {
				SetValue ((float)token);
				ApplyValue ();
			}
		}

		void ISerializable<ValuePacket>.OnDeserialize (ValuePacket value) {
			if (value.TryRead ("value", out float val)) {
				SetValue (val);
				ApplyValue ();
			}
		}

		void ISerializable<JObject>.OnSerialize (JObject value) {
			value.Add ("value", CurrentValue);
		}

		void ISerializable<ValuePacket>.OnSerialize (ValuePacket value) {
			value.Write ("value", CurrentValue);
		}

	}
}