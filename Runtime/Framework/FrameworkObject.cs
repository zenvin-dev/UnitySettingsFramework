using UnityEngine;

namespace Zenvin.Settings.Framework {
	public abstract class FrameworkObject : ScriptableObject {

		[SerializeField, HideInInspector] private string guid = null;
		[SerializeField, HideInInspector] private bool external = false;

		[SerializeField, HideInInspector] private string label = string.Empty;
		[SerializeField, HideInInspector] private string labelLocalizationKey = string.Empty;

		[SerializeField, HideInInspector] private string description = string.Empty;
		[SerializeField, HideInInspector] private string descriptionLocalizationKey = string.Empty;


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

		/// <summary> The name assigned to the object. </summary>
		public string Name {
			get => label;
			internal set => label = value;
		}

		/// <summary> And arbitrary value that can be used to localize the object's <see cref="Name"/>. </summary>
		public string NameLocalizationKey {
			get => labelLocalizationKey;
			internal set => labelLocalizationKey = value;
		}

		/// <summary> A description of this object. </summary>
		public string Description {
			get => description;
			internal set => description = value;
		}

		/// <summary> And arbitrary value that can be used to localize the object's <see cref="Description"/>. </summary>
		public string DescriptionLocalizationKey {
			get => descriptionLocalizationKey;
			internal set => descriptionLocalizationKey = value;
		}

	}
}