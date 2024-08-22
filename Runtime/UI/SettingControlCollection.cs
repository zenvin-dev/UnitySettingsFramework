using System.Collections.Generic;
using UnityEngine;
using System;
using Zenvin.Settings.Framework;

namespace Zenvin.Settings.UI {
	/// <summary>
	/// Helper class for managing references to <see cref="SettingControl"/> prefabs.
	/// </summary>
	[Serializable]
	public class SettingControlCollection {

		private readonly Dictionary<Type, SettingControl> controlDict = new Dictionary<Type, SettingControl> ();

		[field: SerializeField] public bool AssignBaseTypes { get; private set; } = true;
		[SerializeField] private SettingControl[] controls;


		public SettingControl this[Type type] => GetControl (type);


		/// <summary>
		/// Tries to return a <see cref="SettingControl"/> matching the given <see cref="Type"/>.
		/// </summary>
		/// <param name="type"> The <see cref="Type"/> of <see cref="SettingControl"/> to retrieve. </param>
		/// <param name="control"> The found <see cref="SettingControl"/>, if any. </param>
		/// <param name="allowRecursion">Whether to allow recursive searches for valid controls. Slower the first time, but may yield more accurate results. Enabled by default.</param>
		public bool TryGetControl (Type type, out SettingControl control, bool allowRecursion = true) {
			if (type == null) {
				control = null;
				return false;
			}
			InitDict ();
			bool success = controlDict.TryGetValue (type, out control);

			if (!success && allowRecursion) {
				Type _type = type;
				Type baseType = typeof (SettingBase<>);

				while (_type != null && (!type.IsConstructedGenericType || type.GetGenericTypeDefinition () != baseType)) {
					_type = _type.BaseType;
					if (_type is null)
						break;

					if (controlDict.TryGetValue (_type, out control)) {
						controlDict[type] = control;
						break;
					}
				}
				return control != null;
			}

			return success;
		}

		/// <summary>
		/// Tries to return a <see cref="SettingControl"/> matching the given <see cref="Type"/>.
		/// </summary>
		/// <param name="control"> The found <see cref="SettingControl"/>, if any. </param>
		public bool TryGetControl<T> (out SettingControl control) where T : SettingBase {
			return TryGetControl (typeof (T), out control);
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

		/// <summary>
		/// Returns either the last <see cref="SettingControl"/> matching the given <see cref="Type"/>, or <c>null</c>.
		/// </summary>
		public SettingControl GetControl<T> () where T : SettingBase {
			if (TryGetControl (typeof (T), out SettingControl control)) {
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
				if (sc != null) {
					controlDict[sc.ControlType] = sc;
				}
			}
			if (AssignBaseTypes) {
				SetupBaseTypeKeys ();
			}
		}

		private void SetupBaseTypeKeys () {
			Type baseType = typeof (SettingBase<>);

			var coll = new List<Type> (controlDict.Keys);
			foreach (var type in coll) {
				Type _type;
				do {
					_type = type.BaseType;
					controlDict[_type] = controlDict[type];

				} while (_type != baseType && (!_type.IsConstructedGenericType || _type.GetGenericTypeDefinition () == baseType) && !controlDict.ContainsKey (_type));
			}
		}
	}
}
