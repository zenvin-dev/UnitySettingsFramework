using Newtonsoft.Json;
using System.IO;

namespace Zenvin.Settings.Framework.Serialization {
	/// <summary>
	/// Extension of <see cref="JsonSerializer"/>, which automatically writes changes to a file post serialization.
	/// </summary>
	public sealed class JsonFileSerializer : JsonSerializer, ISerializerCallbackReceiver {

		public FileInfo SaveFile { get; private set; }
		public Formatting OutputFormatting { get; set; } = Formatting.Indented;
		public JsonConverter[] Converters { get; set; }


		public JsonFileSerializer (string filePath) {
			SaveFile = new FileInfo (filePath);
		}

		public JsonFileSerializer (FileInfo file) {
			SaveFile = file;
		}

		public static JsonFileSerializer CreateFromFile (FileInfo file) {
			JsonFileSerializer serializer = ReadFromFile (file) as JsonFileSerializer;
			if (serializer != null) {
				serializer.SaveFile = file;
			}
			return serializer;
		}


		void ISerializerCallbackReceiver.FinalizeSerialization () {
			WriteToFile (SaveFile, OutputFormatting, Converters);
		}

		void ISerializerCallbackReceiver.FinalizeDeserialization () { }

		void ISerializerCallbackReceiver.InitializeDeserialization () { }

		void ISerializerCallbackReceiver.InitializeSerialization () { }

	}
}