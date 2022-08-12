using System;

namespace Zenvin.Settings.Utility {
	/// <summary>
	/// Attribute to be attached to any SettingBase class, to hide the default value from the inspector.<br></br>
	/// This can be useful to avoid confusion, in case the Setting in question implements its own "default" value.
	/// </summary>
	[AttributeUsage (AttributeTargets.Class)]
	public class HasDeviatingDefaultValueAttribute : Attribute { }
}