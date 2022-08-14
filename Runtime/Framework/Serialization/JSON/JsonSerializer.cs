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


		public static JsonSerializer ReadFromFile (FileInfo file) {
			if (file == null || !file.Exists) {
				return null;
			}

			JsonSerializer obj = new JsonSerializer ();

			// prepare json reader
			using FileStream stream = file.OpenRead ();
			using StreamReader reader = new StreamReader (stream);
			using JsonTextReader jsonReader = new JsonTextReader (reader);

			// read json data
			JObject data = JToken.ReadFrom (jsonReader) as JObject;
			obj.data = data;

			// return serializer instance of reading data was successful
			return data == null ? null : obj;
		}

		public void WriteToFile (FileInfo file, params JsonConverter[] converters) {
			if (file == null) {
				return;
			}

			// prepare json writer
			using StreamWriter stream = file.CreateText ();
			using JsonTextWriter writer = new JsonTextWriter (stream);

			// write json to stream
			data.WriteTo (writer, converters);
			// save changes to disk
			stream.Flush ();
		}


		IEnumerable<KeyValuePair<string, JObject>> ISerializer<JObject>.GetSerializedData () {
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