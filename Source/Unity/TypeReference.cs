using System;
using System.Linq;

using UnityEngine;


namespace framebunker
{
	[Serializable]
	public class TypeReference
	{
		public class RequireAttribute : Attribute
		{
			[NotNull] private readonly Type m_Type;


			/// <summary>
			/// This field only accepts types implementing the given interface or inheriting from the given MonoBehaviour/ScriptableObject based type
			/// </summary>
			/// <exception cref="ArgumentException">Thrown if passed a non-interface type not inheriting from MonoBehaviour or ScriptableObject</exception>
			public RequireAttribute ([NotNull] Type type)
			{
				if (
					!type.IsInterface &&
					!type.IsSubclassOf (typeof (MonoBehaviour)) &&
					!type.IsSubclassOf (typeof (ScriptableObject))
				)
				{
					throw new ArgumentException ("Required type must be an interface or derived from MonoBehaviour or ScriptableObject");
				}

				m_Type = type;
			}


			public bool Valid ([NotNull] Type type)
			{
				if (m_Type.IsInterface && type.GetInterfaces ().Contains (m_Type))
				{
					return true;
				}

				return type.IsSubclassOf (m_Type);
			}
		}


		private Type m_Cache = null;


		[CanBeNull] public Type Type => m_Cache ?? (string.IsNullOrEmpty (m_TypeName) ? null : m_Cache = Type.GetType (m_TypeName, false));


		[NotNull] public static implicit operator TypeReference ([NotNull] Type type)
		{
			return new TypeReference {m_TypeName = ValidValue (type) ? type.FullName : null};
		}


		[CanBeNull] public static implicit operator Type ([NotNull] TypeReference reference)
		{
			return reference.Type;
		}


		/// <summary>
		/// Does the given type meet the optional requirement or inherit from MonoBehaviour or ScriptableObject?
		/// </summary>
		public static bool ValidValue ([NotNull] Type value, [CanBeNull] RequireAttribute requirement = null)
		{
			if (requirement != null)
			{
				return requirement.Valid (value);
			}

			return value.IsSubclassOf (typeof (MonoBehaviour)) || value.IsSubclassOf (typeof (ScriptableObject));
		}


		[SerializeField] protected UnityEngine.Object m_Script = null;
		[SerializeField, HideInInspector] protected string m_TypeName = null;
	}
}
