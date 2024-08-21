using Newtonsoft.Json.Linq;
using Zenvin.Settings.Framework.Serialization;

namespace Zenvin.Settings.Framework {
	public class FloatSetting : SettingBase<float>, ISerializable<JObject>, ISerializable<ValuePacket> {

		protected override bool TryGetOverrideValue (StringValuePair[] values, out float value) {
			var text = values[0].Value?.Trim ();
			if (string.IsNullOrEmpty (text)) {
				value = default;
				return false;
			}

			return float.TryParse (text, out value);
		}


		void ISerializable<JObject>.OnDeserialize (JObject value) {
			if (value.TryGetValue ("value", out JToken token)) {
				SetValue ((float)token);
			}
		}

		void ISerializable<ValuePacket>.OnDeserialize (ValuePacket value) {
			if (value.TryRead ("value", out float val)) {
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