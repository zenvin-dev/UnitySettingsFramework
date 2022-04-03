using UnityEngine;

namespace Zenvin.Settings.Framework {
	public abstract class FrameworkObject : ScriptableObject {

		[SerializeField, HideInInspector] private string guid = null;
		[SerializeField, HideInInspector] private bool external = false;

		public string GUID {
			get => guid;
			internal set => guid = value;
		}

		public bool External {
			get => external;
			internal set => external = value;
		}

	}
}