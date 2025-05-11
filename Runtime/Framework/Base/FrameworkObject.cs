using System;
using UnityEngine;

namespace Zenvin.Settings.Framework {
	/// <summary>
	/// Base class for various <see cref="ScriptableObject"/>s inside the Settings Framework.
	/// </summary>
	public abstract class FrameworkObject : ScriptableObject {

		[NonSerialized] private bool wasInitialized;
		[SerializeField, HideInInspector] private string guid = null;
		[SerializeField, HideInInspector] private bool external = false;


		/// <summary> Unique ID of the object. </summary>
		public string GUID {
			get => guid;
			internal set => guid = value;
		}

		/// <summary> Whether the object was imported during runtime. </summary>
		public bool External {
			get => external;
			internal set => external = value;
		}

		/// <summary> Whether the object has already been initialized. </summary>
		public bool Initialized {
			get => wasInitialized;
			private protected set => wasInitialized = value;
		}
	}
}
