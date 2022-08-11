using System;
using System.Text;

namespace Zenvin.Settings.Framework.Serialization {
	public partial class ValuePacket {

		public bool Write (string key, sbyte value, bool allowOverwrite = false) {
			return Write (key, BitConverter.GetBytes (value), allowOverwrite);
		}

		public bool Write (string key, byte value, bool allowOverwrite = false) {
			return Write (key, BitConverter.GetBytes (value), allowOverwrite);
		}

		public bool Write (string key, short value, bool allowOverwrite = false) {
			return Write (key, BitConverter.GetBytes (value), allowOverwrite);
		}

		public bool Write (string key, ushort value, bool allowOverwrite = false) {
			return Write (key, BitConverter.GetBytes (value), allowOverwrite);
		}

		public bool Write (string key, int value, bool allowOverwrite = false) {
			return Write (key, BitConverter.GetBytes (value), allowOverwrite);
		}

		public bool Write (string key, uint value, bool allowOverwrite = false) {
			return Write (key, BitConverter.GetBytes (value), allowOverwrite);
		}

		public bool Write (string key, long value, bool allowOverwrite = false) {
			return Write (key, BitConverter.GetBytes (value), allowOverwrite);
		}

		public bool Write (string key, ulong value, bool allowOverwrite = false) {
			return Write (key, BitConverter.GetBytes (value), allowOverwrite);
		}

		public bool Write (string key, float value, bool allowOverwrite = false) {
			return Write (key, BitConverter.GetBytes (value), allowOverwrite);
		}

		public bool Write (string key, double value, bool allowOverwrite = false) {
			return Write (key, BitConverter.GetBytes (value), allowOverwrite);
		}

		public bool Write (string key, bool value, bool allowOverwrite = false) {
			return Write (key, BitConverter.GetBytes (value), allowOverwrite);
		}

		public bool Write (string key, string value, Encoding encoding = null, bool allowOverwrite = false) {
			if (value == null) {
				return false;
			}
			encoding = encoding ?? Encoding.UTF8;
			return Write (key, encoding.GetBytes (value), allowOverwrite);
		}

		public bool Write (string key, char value, bool allowOverwrite = false) {
			return Write (key, BitConverter.GetBytes (value), allowOverwrite);
		}

		public bool Write (string key, byte[] value, bool allowOverwrite = false) {
			if (string.IsNullOrEmpty (key)) {
				return false;
			}
			if (value == null || value.Length == 0) {
				return false;
			}
			if (data.ContainsKey (key) && !allowOverwrite) {
				return false;
			}

			data[key] = value;
			return true;
		}

	}
}