using System;
using System.Linq;

using UnityEditor;

using UnityEngine;


namespace framebunker
{
	[CustomPropertyDrawer (typeof (TypeReference), true)]
	public class TypeReferenceDrawer : PropertyDrawer
	{
		public override void OnGUI (Rect position, [NotNull] SerializedProperty property, [NotNull] GUIContent label)
		{
			SerializedProperty
				scriptProperty = property.FindPropertyRelative ("m_Script"),
				typeNameProperty = property.FindPropertyRelative ("m_TypeName");

			if (null == scriptProperty || null == typeNameProperty)
			{
				GUI.Label (position, "Could not find TypeReference properties", EditorStyles.helpBox);
				return;
			}

			bool changed = false;
			MonoScript script = null;

			TypeReference.RequireAttribute requirement = (TypeReference.RequireAttribute)fieldInfo?.FieldType.
				GetCustomAttributes (typeof (TypeReference.RequireAttribute), true).FirstOrDefault ();

			// We have a type name, but no script is populated (properly a default field value from script) - try to populate the script
			if (null == scriptProperty.objectReferenceValue && !string.IsNullOrEmpty (typeNameProperty.stringValue))
			{
				Type value = Type.GetType (typeNameProperty.stringValue, false);
				scriptProperty.objectReferenceValue = script = value == null || !TypeReference.ValidValue (value, requirement)
					? null
					: Helpers.GetScriptAsset (value);

				// Either we set our script property reference or we kept it at null and need to clear the type name
				changed = true;
			}

			// Do the field for the MonoScript value
			EditorGUI.BeginChangeCheck ();
				EditorGUI.ObjectField (position, scriptProperty, typeof (MonoScript), label);
			if (EditorGUI.EndChangeCheck ())
			{
				script = (MonoScript)property.objectReferenceValue;
			}
			// No change, so we return - unless we already changed the script or type name property
			else if (!changed)
			{
				return;
			}

			// Was the field cleared? Clear the type name as well.
			if (script == null)
			{
				typeNameProperty.stringValue = null;
				changed = true;
			}
			// Actual value - evaluate and update or clear
			else
			{
				Type value = script.GetClass ();

				// Strange MonoScript reference with no class
				if (null == value)
				{
					Debug.LogErrorFormat (
						"Value provided for {0} provided no type information",
						fieldInfo.DeclaringType + "." + fieldInfo.Name
					);

					property.objectReferenceValue = null;
					typeNameProperty.stringValue = null;
					changed = true;
				}
				// Given type is not a valid value for this TypeField - clear things out
				else if (!TypeReference.ValidValue (value, requirement))
				{
					Debug.LogErrorFormat (
						"{0} does not support given value of {1}",
						fieldInfo.DeclaringType + "." + fieldInfo.Name,
						value.FullName
					);

					property.objectReferenceValue = null;
					typeNameProperty.stringValue = null;
					changed = true;
				}
				// Given type is valid - update the type name to match
				else
				{
					typeNameProperty.stringValue = value.FullName;
				}
			}

			// Wrap up, applying if necessary
			if (changed)
			{
				property.serializedObject?.ApplyModifiedProperties ();
			}
		}
	}
}
