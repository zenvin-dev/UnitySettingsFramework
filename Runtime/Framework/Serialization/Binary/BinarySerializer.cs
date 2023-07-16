using System.Collections.Generic;
using System.IO;

namespace Zenvin.Settings.Framework.Serialization {
	public class BinarySerializer : ISerializer<ValuePacket> {

		private readonly Dictionary<string, byte[]> data = new Dictionary<string, byte[]> ();


		public bool ReadFromFile (FileInfo file) {
			if (file == null || !file.Exists) {
				return false;
			}

			// open save file
			using FileStream stream = file.OpenRead ();
			using BinaryReader reader = new BinaryReader (stream);

			// read length of saved data
			int count = reader.ReadInt32 ();

			for (int i = 0; i < count; i++) {
				// read guid of saved data
				string guid = reader.ReadString ();

				// read saved data
				byte[] binData = reader.ReadArray ();

				// associate data with guid
				data[guid] = binData;
			}

			// return filled instance
			return true;
		}

		public bool WriteToFile (FileInfo file) {
			if (file == null) {
				return false;
			}

			// create save file
			using FileStream stream = file.Create ();
			using BinaryWriter writer = new BinaryWriter (stream);

			// write length of the data
			writer.Write (data.Count);

			foreach (var kvp in data) {
				// write guid
				writer.Write (kvp.Key);

				// write data array
				writer.WriteArray (kvp.Value);
			}

			// save changes to disk
			stream.Flush ();
			return true;
		}


		IEnumerator<KeyValuePair<string, ValuePacket>> ISerializer<ValuePacket>.GetSerializedData () {
			foreach (var kvp in data) {
				yield return new KeyValuePair<string, ValuePacket> (kvp.Key, new ValuePacket (kvp.Value));
			}
		}

		void ISerializer<ValuePacket>.Serialize (string guid, ValuePacket value) {
			data[guid] = value;
		}

	}
}