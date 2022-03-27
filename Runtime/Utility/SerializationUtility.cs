using System.IO;

namespace Zenvin.Settings.Utility {
	public static class SerializationUtility {

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