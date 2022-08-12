using System;

namespace Zenvin.Settings.Utility {
	public static class EnumUtility {

		public static object[] ValuesToArray<T> () where T : struct {
			Type type = typeof (T);
			if (!type.IsEnum) {
				throw new Exception ($"Given type ({type.FullName}) must be an enum.");
			}

			var values = Enum.GetValues (type);
			object[] tempValues = new object[values.Length];
			for (int i = 0; i < tempValues.Length; i++) {
				tempValues[i] = values.GetValue (i);
			}
			return tempValues;
		}

	}
}