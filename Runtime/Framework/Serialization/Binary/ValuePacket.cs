using System;
using System.Collections.Generic;
using System.IO;

namespace Zenvin.Settings.Framework.Serialization {
	/// <summary>
	/// Helper class to serialize key/value pairs as byte array.<br></br>
	/// Contains an implicit conversion to <see cref="byte"/>[].
	/// </summary>
	public partial class ValuePacket {

		private readonly Dictionary<string, byte[]> data = new Dictionary<string, byte[]> ();


		public ValuePacket () { }

		public ValuePacket (byte[] byteData) {
			FromArray (byteData);
		}


		private void FromArray (byte[] byteData) {
			using MemoryStream stream = new MemoryStream (byteData);
			using BinaryReader reader = new BinaryReader (stream);

			int count = reader.ReadInt32 ();
			for (int i = 0; i < count; i++) {
				string key = reader.ReadString ();
				byte[] value = reader.ReadArray ();
				data[key] = value;
			}
		}

		/// <summary>
		/// Returns the contents of the packet as a byte array.
		/// </summary>
		public byte[] ToArray () {
			using MemoryStream stream = new MemoryStream ();
			using BinaryWriter writer = new BinaryWriter(stream);

			writer.Write (data.Count);

			foreach (var val in data) {
				writer.Write (val.Key);
				writer.WriteArray (val.Value);
			}

			return stream.ToArray ();
		}

		public static implicit operator byte[] (ValuePacket packet) {
			if (packet == null) {
				return Array.Empty<byte> ();
			}
			return packet.ToArray ();
		}

	}
}