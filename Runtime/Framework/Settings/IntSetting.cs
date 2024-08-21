using Newtonsoft.Json.Linq;
using Zenvin.Settings.Framework.Serialization;

namespace Zenvin.Settings.Framework {
	public class IntSetting : SettingBase<int>, ISerializable<JObject>, ISerializable<ValuePacket> {

		protected override bool TryGetOverrideValue (StringValuePair[] values, out int value) {
			var text = values[0].Value?.Trim ();
			if (string.IsNullOrEmpty (text)) {
				value = default;
				return false;
			}

			return int.TryParse (text, out value);
		}


		void ISerializable<JObject>.OnDeserialize (JObject value) {
			if (value.TryGetValue ("value", out JToken token)) {
				SetValue ((int)token);
			}
		}

		void ISerializable<ValuePacket>.OnDeserialize (ValuePacket value) {
			if (value.TryRead ("value", out int val)) {
				SetValue (val);
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