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
