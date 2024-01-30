using System.IO;

namespace Zenvin.Settings.Framework.Serialization {
	/// <summary>
	/// A class providing utility methods to write and read byte arrays to and from streams.
	/// </summary>
	public static class SerializationUtility {

		/// <summary>
		/// Writes a byte array to a stream by means of a given <see cref="BinaryWriter"/>.
		/// </summary>
		/// <param name="writer"> The <see cref="BinaryWriter"/> to write the array to. </param>
		/// <param name="array"> The byte array to write to the stream. May be <see langword="null"/>. </param>
		/// <remarks>
		/// This method assumes that the <paramref name="writer"/> is valid (i.e. it wraps an open stream that can be written to).
		/// </remarks>
		public static void WriteArray (this BinaryWriter writer, byte[] array) {
			if (array == null) {
				writer.Write (0);
				return;
			}
			writer.Write (array.Length);
			for (int i = 0; i < array.Length; i++) {
				writer.Write (array[i]);
			}
		}

		/// <summary>
		/// Reads a byte array from a stream by means of a given <see cref="BinaryReader"/>.
		/// </summary>
		/// <param name="reader"> The <see cref="BinaryReader"/> used to read the values into the array. </param>
		/// <returns> The read byte array. </returns>
		/// <remarks>
		/// This method assumes that the <paramref name="reader"/> is valid (i.e. it wraps an open stream that can be read from).<br></br>
		/// It also assumes that the array was written by (or in the same format used by) <see cref="WriteArray(BinaryWriter, byte[])"/>.
		/// </remarks>
		public static byte[] ReadArray (this BinaryReader reader) {
			int len = reader.ReadInt32 ();
			byte[] arr = new byte[len];
			for (int i = 0; i < len; i++) {
				arr[i] = reader.ReadByte ();
			}
			return arr;
		}

	}
}