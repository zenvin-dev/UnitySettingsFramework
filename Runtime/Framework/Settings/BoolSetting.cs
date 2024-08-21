using Newtonsoft.Json.Linq;
using Zenvin.Settings.Framework.Serialization;

namespace Zenvin.Settings.Framework {
	public class BoolSetting : SettingBase<bool>, ISerializable<JObject>, ISerializable<ValuePacket> {

		protected override bool TryGetOverrideValue (StringValuePair[] values, out bool value) {
			var text = values[0].Value?.Trim ();

			if (string.IsNullOrEmpty (text)) {
				value = default;
				return false;
			}
			if (text.Equals ("true", System.StringComparison.OrdinalIgnoreCase)) {
				value = true;
				return true;
			}
			if (text.Equals ("false", System.StringComparison.OrdinalIgnoreCase)) {
				value = false;
				return true;
			}

			value = default;
			return false;
		}


		void ISerializable<JObject>.OnDeserialize (JObject data) {
			if (data.TryGetValue ("value", out JToken token)) {
				SetValue ((bool)token);
			}
		}

		void ISerializable<ValuePacket>.OnDeserialize (ValuePacket value) {
			if (value.TryRead ("value", out bool val)) {
				SetValue (val);
			}
		}

		void ISerializable<JObject>.OnSerialize (JObject data) {
			data.Add ("value", CurrentValue);
		}

		void ISerializable<ValuePacket>.OnSerialize (ValuePacket value) {
			value.Write ("value", CurrentValue);
		}

	}
}