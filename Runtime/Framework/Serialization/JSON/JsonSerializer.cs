using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;

namespace Zenvin.Settings.Framework.Serialization {
	public class JsonSerializer : ISerializer<JObject> {

		private JObject data;


		public JsonSerializer () {
			data = new JObject ();
		}


		public bool ReadFromFile (FileInfo file) {
			if (file == null || !file.Exists) {
				return false;
			}

			// prepare json reader
			using FileStream stream = file.OpenRead ();
			using StreamReader reader = new StreamReader (stream);
			using JsonTextReader jsonReader = new JsonTextReader (reader);

			// read json data
			JObject jData = JToken.ReadFrom (jsonReader) as JObject;
			data = jData;

			// return whether reading data was successful
			return data != null;
		}

		public bool WriteToFile (FileInfo file, Formatting formatting = Formatting.Indented, params JsonConverter[] converters) {
			if (file == null) {
				return false;
			}

			string json = JsonConvert.SerializeObject (data, formatting, converters);
			File.WriteAllText (file.FullName, json);
			return true;
		}


		IEnumerator<KeyValuePair<string, JObject>> ISerializer<JObject>.GetSerializedData () {
			foreach (var item in data) {
				if (item.Value is JObject jObject) {
					yield return new KeyValuePair<string, JObject> (item.Key, jObject);
				}
			}
		}

		void ISerializer<JObject>.Serialize (string guid, JObject value) {
			data[guid] = value;
		}


		public override string ToString () {
			return data.ToString ();
		}

	}
}