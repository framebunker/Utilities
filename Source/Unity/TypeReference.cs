/*
 *
Copyright 2020 framebunker

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
documentation files (the "Software"), to deal in the Software without restriction, including without limitation the
rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit
persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the
Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 */


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
