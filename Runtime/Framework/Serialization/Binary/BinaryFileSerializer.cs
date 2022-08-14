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

		public bool ReadFromFile () {
			return ReadFromFile (SaveFile);
		}

		public bool WriteToFile () {
			return WriteToFile (SaveFile);
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