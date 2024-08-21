using Newtonsoft.Json.Linq;
using System.Text;
using Zenvin.Settings.Framework.Serialization;

namespace Zenvin.Settings.Framework {
	public class StringSetting : SettingBase<string>, ISerializable<JObject>, ISerializable<ValuePacket> {

		private Encoding StringEncoding => Encoding.UTF8;


		protected override bool TryGetOverrideValue (StringValuePair[] values, out string value) {
			value = values[0].Value;
			return true;
		}


		void ISerializable<JObject>.OnDeserialize (JObject value) {
			if (value.TryGetValue ("value", out JToken token)) {
				SetValue ((string)token);
			}
		}

		void ISerializable<ValuePacket>.OnDeserialize (ValuePacket value) {
			if (value.TryRead ("value", out string val)) {
				SetValue (val);
			}
		}

		void ISerializable<JObject>.OnSerialize (JObject value) {
			value.Add ("value", CurrentValue);
		}

		void ISerializable<ValuePacket>.OnSerialize (ValuePacket value) {
			value.Write ("value", CurrentValue, StringEncoding);
		}
	}
}
