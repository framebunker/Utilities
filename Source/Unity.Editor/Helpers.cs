using System;

using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;


namespace framebunker
{
	public static class Helpers
	{
		[CanBeNull] public static MonoScript GetScriptAsset ([CanBeNull] Type type)
		{
			if (null == type)
			{
				return null;
			}

			MonoScript result = null;

			if (type.IsSubclassOf (typeof (MonoBehaviour)))
			{
				MonoBehaviour instance = (MonoBehaviour)new GameObject (type.Name, type).GetComponent (type);
				if (null != instance)
				{
					result = MonoScript.FromMonoBehaviour (instance);
					Object.DestroyImmediate (instance.gameObject);
				}
			}
			else if (type.IsSubclassOf (typeof (ScriptableObject)))
			{
				ScriptableObject instance = (ScriptableObject)Activator.CreateInstance (type);
				result = MonoScript.FromScriptableObject (instance);
				Object.DestroyImmediate (instance);
			}

			return result;
		}
	}
}
