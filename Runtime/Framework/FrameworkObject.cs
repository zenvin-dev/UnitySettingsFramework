using UnityEngine;

namespace Zenvin.Settings.Framework {
	public abstract class FrameworkObject : ScriptableObject {

		[SerializeField, HideInInspector] private string guid = null;
		[SerializeField, HideInInspector] private bool external = false;

		[SerializeField, HideInInspector] private string label = string.Empty;
		[SerializeField, HideInInspector] private string labelLocalizationKey = string.Empty;

		[SerializeField, HideInInspector] private string description = string.Empty;
		[SerializeField, HideInInspector] private string descriptionLocalizationKey = string.Empty;


		public string GUID {
			get => guid;
			internal set => guid = value;
		}

		/// <summary> Whether this object was imported during runtime. </summary>
		public bool External {
			get => external;
			internal set => external = value;
		}

		public string Name {
			get => label;
			internal set => label = value;
		}

		public string NameLocalizationKey {
			get => labelLocalizationKey;
			internal set => labelLocalizationKey = value;
		}

		public string Description {
			get => description;
			internal set => description = value;
		}

		public string DescriptionLocalizationKey {
			get => descriptionLocalizationKey;
			internal set => descriptionLocalizationKey = value;
		}

	}
}