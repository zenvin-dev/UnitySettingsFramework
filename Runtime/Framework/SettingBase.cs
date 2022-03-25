using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;

namespace Zenvin.Settings.Framework {
	public abstract class SettingBase : ScriptableObject {

		[SerializeField, HideInInspector] internal SettingsAsset asset;
		[SerializeField, HideInInspector] internal SettingsGroup group;

		private protected abstract Type ValueType { get; }


		internal void Setup (SettingsAsset asset) {
			this.asset = asset;
			OnSetup ();
		}

		private protected virtual void OnSetup () { }

	}

	public abstract class SettingBase<T> : SettingBase where T : struct {




		private protected sealed override Type ValueType => typeof (T);

	}
}