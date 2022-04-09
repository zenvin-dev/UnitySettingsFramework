using UnityEngine.Localization.Settings;
using Zenvin.Settings.Framework;
using UnityEngine.Localization;
using Zenvin.Settings.UI;

namespace Zenvin.Settings.Samples {
	public abstract class LocalizedControlBase<T, U> : SettingControl<T, U> where T : SettingBase<U> {

		protected sealed override void OnSetup () {
			LocalizationSettings.SelectedLocaleChanged += OnLocalizationChanged;
			OnLocalizedSetup ();
			OnLocalizationChanged (LocalizationSettings.SelectedLocale);
		}

		private void OnDestroy () {
			LocalizationSettings.SelectedLocaleChanged -= OnLocalizationChanged;
		}

		/// <summary>
		/// Method to replace <see cref="OnSetup"/>, because that is used by <see cref="LocalizedControlBase{T, U}"/>.<br></br>
		/// <inheritdoc cref="OnSetup"/>
		/// </summary>
		protected virtual void OnLocalizedSetup () { }

		protected virtual void OnLocalizationChanged (Locale locale) { }

	}
}