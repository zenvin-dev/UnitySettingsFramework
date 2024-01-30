using System;

namespace Zenvin.Settings.Framework {
	/// <summary>
	/// Extension of <see cref="SettingReference{T}"/>, which dynamically updates its referenced <see cref="SettingBase{T}"/> whenever its target asset changes.<br></br>
	/// Use <see cref="SettingsAsset.GetSettingReference{T}(string, T)"/> to obtain an instance of this class.
	/// </summary>
	public class DynamicSettingReference<T> : SettingReference<T>, IDisposable {

		/// <summary>
		/// Delegate type for the <see cref="ReferenceChanged"/> event.
		/// </summary>
		/// <param name="previous"> The <see cref="SettingBase"/> reference encapsulated by the <see cref="DynamicSettingReference{T}"/> before the change. </param>
		/// <param name="current"> The <see cref="SettingBase"/> reference encapsulated by the <see cref="DynamicSettingReference{T}"/> after the change. </param>
		public delegate void ReferenceChangedEvent (SettingBase previous, SettingBase current);
		/// <summary> Invoked every time the reference's encapsulated <see cref="Setting"/> changes. </summary>
		public event ReferenceChangedEvent ReferenceChanged;


		/// <summary>
		/// Whether the refrenced <see cref="SettingBase{T}"/> should be updated, even if it is not <see langword="null"/>.<br></br>
		/// Default: <see langword="true"/>.
		/// </summary>
		public bool OverwriteExistingReference { get; set; } = true;
		/// <summary>
		/// Whether the referenced <see cref="SettingBase{T}"/> can be set to <see langword="null"/> through the setter of <see cref="Setting"/>.<br></br>
		/// Default: <see langword="true"/>.
		/// </summary>
		public bool AllowResettingReference { get; set; } = true;
		/// <summary>
		/// The asset, in which to look for a reference.<br></br>
		/// Uses the <see cref="SettingsAsset.OnInitialize"/> and <see cref="SettingsAsset.OnRuntimeSettingsLoaded"/> events to update when needed.
		/// </summary>
		/// <remarks>
		/// Use <see cref="Dispose"/> to remove the event handlers and open the instance up for garbage collection.
		/// </remarks>
		public SettingsAsset ReferenceAsset { get; private set; }
		/// <summary>
		/// The <see cref="FrameworkObject.GUID"/> used to find a setting reference in the <see cref="ReferenceAsset"/>.
		/// </summary>
		public string ReferenceId { get; private set; }
		/// <summary>
		/// Whether the dynamic reference was disposed of.<br></br>
		/// Disposed references will not get updated anymore, when their <see cref="ReferenceAsset"/> changes.
		/// </summary>
		public bool Disposed { get; private set; }
		/// <inheritdoc/>
		public override SettingBase<T> Setting { get => base.Setting; set => SetSetting (value); }


		private DynamicSettingReference () { }

		internal DynamicSettingReference (SettingsAsset asset, string referenceId) {
			ReferenceAsset = asset;
			ReferenceId = referenceId;

			UpdateReference (asset);

			SettingsAsset.OnInitialize += AssetInitializedHandler;
			SettingsAsset.OnRuntimeSettingsLoaded += AssetLoadedHandler;
		}


		/// <summary>
		/// Disposes of the <see cref="DynamicSettingReference{T}"/> instance.<br></br>
		/// </summary>
		/// <remarks>
		/// Removes all event handlers attached by the class's constructor.
		/// </remarks>
		public void Dispose () {
			SettingsAsset.OnInitialize -= AssetInitializedHandler;
			SettingsAsset.OnRuntimeSettingsLoaded -= AssetLoadedHandler;
			Disposed = true;
		}


		private void UpdateReference (SettingsAsset asset) {
			if (ReferenceAsset == null || ReferenceAsset != asset || asset == null) {
				return;
			}
			if (Setting != null && !OverwriteExistingReference) {
				return;
			}
			if (string.IsNullOrWhiteSpace (ReferenceId)) {
				return;
			}

			if (asset.TryGetSettingByGUID (ReferenceId, out SettingBase<T> setting)) {
				SetSetting (setting);
			}
		}

		private void SetSetting (SettingBase<T> value) {
			if (!AllowResettingReference && value == null) {
				return;
			}
			if (Setting == value) {
				return;
			}

			var previous = Setting;
			settingObj = value;
			ReferenceChanged?.Invoke (previous, value);
		}


		private void AssetInitializedHandler (SettingsAsset asset) {
			UpdateReference (asset);
		}

		private void AssetLoadedHandler (SettingsAsset asset) {
			UpdateReference (asset);
		}
	}
}
