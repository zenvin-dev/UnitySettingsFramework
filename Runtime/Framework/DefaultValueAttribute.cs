using System;
using UnityEngine;

namespace Zenvin.Settings.Framework {
	/// <summary>
	/// Attribute for marking a Setting's default value field as such, in order for its custom property drawer to work.
	/// </summary>
	[AttributeUsage (AttributeTargets.Field)]
	public class DefaultValueAttribute : PropertyAttribute { }
}