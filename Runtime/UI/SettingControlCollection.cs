using System.Collections.Generic;
using UnityEngine;
using System;

namespace Zenvin.Settings.UI {
	[Serializable]
	public class SettingControlCollection {

		private readonly Dictionary<Type, SettingControl> controlDict = new Dictionary<Type, SettingControl>();

		[SerializeField] private SettingControl[] controls;


		public SettingControl this[Type type] => GetControl (type);


		/// <summary>
		/// Tries to return a <see cref="SettingControl"/> matching the given <see cref="Type"/>.
		/// </summary>
		/// <param name="type"> The <see cref="Type"/> of <see cref="SettingControl"/> to retrieve. </param>
		/// <param name="control"> The found <see cref="SettingControl"/>, if any. </param>
		public bool TryGetControl (Type type, out SettingControl control) {
			if (type == null) {
				control = null;
				return false;
			}
			InitDict ();
			return controlDict.TryGetValue (type, out control);
		}

		/// <summary>
		/// Returns either the last <see cref="SettingControl"/> matching the given <see cref="Type"/>, or <c>null</c>.
		/// </summary>
		/// <param name="type"> The <see cref="Type"/> of <see cref="SettingControl"/> to retrieve. </param>
		public SettingControl GetControl (Type type) {
			if (TryGetControl (type, out SettingControl control)) {
				return control;
			}
			return null;
		}


		private void InitDict (bool force = false) {
			if (controls.Length == controlDict.Count && !force) {
				return;
			}
			controlDict.Clear ();
			foreach (var sc in controls) {
				controlDict[sc.ControlType] = sc;
			}
		}

	}
}