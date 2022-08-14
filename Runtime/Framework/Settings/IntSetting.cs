using Newtonsoft.Json.Linq;
using Zenvin.Settings.Framework.Serialization;

namespace Zenvin.Settings.Framework {
	public class IntSetting : SettingBase<int>, ISerializable<JObject>, ISerializable<ValuePacket> {

		void ISerializable<JObject>.OnDeserialize (JObject value) {
			if (value.TryGetValue ("value", out JToken token)) {
				SetValue ((int)token);
				ApplyValue ();
			}
		}

		void ISerializable<ValuePacket>.OnDeserialize (ValuePacket value) {
			if (value.TryRead ("value", out int val)) {
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