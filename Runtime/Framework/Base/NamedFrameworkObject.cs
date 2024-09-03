using UnityEngine;

namespace Zenvin.Settings.Framework {
	/// <summary>
	/// An class extending the <see cref="FrameworkObject"/> to include human-readable labels.
	/// </summary>
	public abstract class NamedFrameworkObject : FrameworkObject {

		[SerializeField, HideInInspector] private string label = string.Empty;
		[SerializeField, HideInInspector] private string labelLocalizationKey = string.Empty;

		[SerializeField, HideInInspector] private string description = string.Empty;
		[SerializeField, HideInInspector] private string descriptionLocalizationKey = string.Empty;


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

		/// <summary> A description of the object. </summary>
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
