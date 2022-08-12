using System.Reflection;
using UnityEditor;
using UnityEngine;
using Zenvin.Settings.Utility;

namespace Zenvin.Settings.Framework {
	[CustomPropertyDrawer (typeof (DefaultValueAttribute))]
	public class DefaultValueDrawer : PropertyDrawer {

		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
			if (GetIsValueVisible (property)) {
				//base.OnGUI (position, property, label);
				EditorGUI.PropertyField (position, property, label);
			}
		}

		public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
			return GetIsValueVisible (property) ? base.GetPropertyHeight (property, label) : 0f;
		}

		private bool GetIsValueVisible (SerializedProperty prop) {
			return prop?.serializedObject?.targetObject?.GetType ()?.GetCustomAttribute<HasDeviatingDefaultValueAttribute> () == null;
		}

	}
}