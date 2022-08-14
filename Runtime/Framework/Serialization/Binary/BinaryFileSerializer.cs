using System.IO;

namespace Zenvin.Settings.Framework.Serialization {
	/// <summary>
	/// Extension of <see cref="BinarySerializer"/>, which automatically writes changes to a file post serialization.
	/// </summary>
	public sealed class BinaryFileSerializer : BinarySerializer, ISerializerCallbackReceiver {

		public FileInfo SaveFile { get; private set; }


		public BinaryFileSerializer (string filePath) {
			SaveFile = new FileInfo (filePath);
		}

		public BinaryFileSerializer (FileInfo file) {
			SaveFile = file;
		}

		public static BinaryFileSerializer CreateFromFile (FileInfo file) {
			BinaryFileSerializer serializer = ReadFromFile (file) as BinaryFileSerializer;
			if (serializer != null) {
				serializer.SaveFile = file;
			}
			return serializer;
		}


		void ISerializerCallbackReceiver.FinalizeSerialization () {
			WriteToFile (SaveFile);
		}

		void ISerializerCallbackReceiver.FinalizeDeserialization () { }

		void ISerializerCallbackReceiver.InitializeDeserialization () { }

		void ISerializerCallbackReceiver.InitializeSerialization () { }

	}
}