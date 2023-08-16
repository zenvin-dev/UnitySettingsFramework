using System;
using System.Collections.Generic;
using UnityEngine;
using Zenvin.Settings.Framework;

namespace Zenvin.Settings.Loading {
	/// <summary>
	/// Wrapper class for arguments used by <see cref="RuntimeSettingLoader.LoadSettingsIntoAsset(SettingLoaderOptions)"/>.<br></br>
	/// </summary>
	public class SettingLoaderOptions {
		internal readonly Dictionary<string, ISettingFactory> SettingFactories = new Dictionary<string, ISettingFactory> ();
		internal readonly Dictionary<string, IGroupFactory> GroupFactories = new Dictionary<string, IGroupFactory> ();

		/// <summary> The <see cref="SettingsAsset"/> into which new Settings should be loaded. </summary>
		public SettingsAsset Asset { get; private set; }
		/// <summary> Data defining Settings and Setting Groups to load. </summary>
		public SettingsImportData Data { get; private set; }
		/// <summary> An optional <see cref="IGroupIconLoader"/> instance, used to load Setting Group icons. </summary>
		public IGroupIconLoader IconLoader { get; private set; }
		/// <summary>
		/// The type of <see cref="SettingsGroup"/> which is used if the <see cref="ObjectDataBase.Type"/> is empty or does not have an associated <see cref="IGroupFactory"/>.
		/// </summary>
		public Type DefaultGroupType { get; private set; } = typeof (SettingsGroup);

		/// <summary> Determines whether the options are valid. </summary>
		public bool IsValid => Asset != null && SettingFactories.Count > 0 && Data != null && Data.Settings != null && Data.Settings.Length > 0;

		private SettingLoaderOptions () { }

		public SettingLoaderOptions (SettingsAsset asset) {
			if (asset == null) {
				throw new ArgumentNullException (nameof (asset), $"A {nameof (SettingsAsset)} is required.");
			}
			Asset = asset;
		}


		/// <summary>
		/// Fluent builder to set the options' <see cref="Data"/>.
		/// </summary>
		public SettingLoaderOptions WithData (SettingsImportData data) {
			Data = data;
			return this;
		}

		/// <summary>
		/// Fluent builder to set the options' <see cref="Data"/> given a JSON string.
		/// </summary>
		/// <remarks>
		/// Internally uses <see cref="TrySetDataFromJson(string)"/>.
		/// </remarks>
		/// <param name="dataJson"> A string in JSON format that will be parsed into a <see cref="SettingsImportData"/> object. </param>
		public SettingLoaderOptions WithData (string dataJson) {
			TrySetDataFromJson (dataJson);
			return this;
		}

		/// <summary>
		/// Fluent builder to add an <see cref="ISettingFactory"/>, using its default type string.<br></br>
		/// Will not do anything if the given name was empty or <see langword="null"/>, or if the <paramref name="factory"/> was <see langword="null"/>.
		/// </summary>
		/// <remarks>
		/// If a factory with the given name already existed, its instance will be replaced.
		/// </remarks>
		public SettingLoaderOptions WithSettingFactory (ISettingFactory factory) {
			if (factory != null) {
				SettingFactories[factory.GetDefaultValidType ()] = factory;
			}
			return this;
		}

		/// <summary>
		/// Fluent builder to add an <see cref="ISettingFactory"/>, overriding its default type string.<br></br>
		/// Will not do anything if the given name was empty or <see langword="null"/>, or if the <paramref name="factory"/> was <see langword="null"/>.<br></br>
		/// </summary>
		/// <remarks>
		/// If a factory with the given name already existed, its instance will be replaced.
		/// </remarks>
		public SettingLoaderOptions WithSettingFactory (string type, ISettingFactory factory) {
			if (!string.IsNullOrWhiteSpace (type) && factory != null) {
				SettingFactories[type] = factory;
			}
			return this;
		}

		/// <summary>
		/// Fluent builder to add an <see cref="IGroupFactory"/>, using its default type string.<br></br>
		/// Will not do anything if the given name was empty or <see langword="null"/>, or if the <paramref name="factory"/> was <see langword="null"/>.
		/// </summary>
		/// <remarks>
		/// If a factory with the given name already existed, its instance will be replaced.
		/// </remarks>
		public SettingLoaderOptions WithGroupFactory (IGroupFactory factory) {
			if (factory != null) {
				GroupFactories[factory.GetDefaultValidType ()] = factory;
			}
			return this;
		}

		/// <summary>
		/// Fluent builder to add an <see cref="IGroupFactory"/>, overriding its default type string.<br></br>
		/// Will not do anything if the given name was empty or <see langword="null"/>, or if the <paramref name="factory"/> was <see langword="null"/>.<br></br>
		/// </summary>
		/// <remarks>
		/// If a factory with the given name already existed, its instance will be replaced.
		/// </remarks>
		public SettingLoaderOptions WithGroupFactory (string type, IGroupFactory factory) {
			if (!string.IsNullOrWhiteSpace (type) && factory != null) {
				GroupFactories[type] = factory;
			}
			return this;
		}

		/// <summary>
		/// Fluent builder to set <see cref="DefaultGroupType"/>.
		/// </summary>
		public SettingLoaderOptions WithDefaultGroupType<T> () where T : SettingsGroup {
			DefaultGroupType = typeof (T);
			return this;
		}

		/// <summary>
		/// Fluent builder to disable <see cref="DefaultGroupType"/>.<br></br>
		/// Doing so will result in no group being created if there is no factory for it.
		/// </summary>
		public SettingLoaderOptions WithoutDefaultGroupType () {
			DefaultGroupType = null;
			return this;
		}

		/// <summary>
		/// Attempts to parse a given JSON string to a <see cref="SettingsImportData"/> object.<br></br>
		/// If successful, the parsed objects is set as the options' <see cref="Data"/>. Otherwise, the Data value is reset to <see langword="null"/>.
		/// </summary>
		public bool TrySetDataFromJson (string json) {
			if (string.IsNullOrWhiteSpace (json)) {
				return false;
			}

			try {
				Data = JsonUtility.FromJson<SettingsImportData> (json);
				return true;
			} catch {
				Data = null;
				return false;
			}
		}

		/// <summary>
		/// Fluent builder to set <see cref="IconLoader"/>.
		/// </summary>
		public SettingLoaderOptions WithIconLoader (IGroupIconLoader iconLoader) {
			IconLoader = iconLoader;
			return this;
		}


		internal SettingLoaderOptions WithTypeFactories (RuntimeSettingLoader.TypeFactoryWrapper[] factories) {
			RuntimeLoaderHelpers.PopulateFactoryDicts (factories, SettingFactories, GroupFactories);
			return this;
		}
	}
}