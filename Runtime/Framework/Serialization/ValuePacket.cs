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


		public byte[] ToArray () {
			using MemoryStream stream = new MemoryStream ();
			using BinaryWriter writer = new BinaryWriter(stream);

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