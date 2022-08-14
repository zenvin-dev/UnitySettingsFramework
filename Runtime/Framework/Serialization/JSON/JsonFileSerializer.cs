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

		public bool ReadFromFile () {
			return ReadFromFile (SaveFile);
		}

		public bool WriteToFile () {
			return WriteToFile (SaveFile, OutputFormatting, Converters);
		}


		void ISerializerCallbackReceiver.FinalizeSerialization () {
			WriteToFile ();
		}

		void ISerializerCallbackReceiver.InitializeDeserialization () {
			ReadFromFile ();
		}

		void ISerializerCallbackReceiver.FinalizeDeserialization () { }

		void ISerializerCallbackReceiver.InitializeSerialization () { }

	}
}