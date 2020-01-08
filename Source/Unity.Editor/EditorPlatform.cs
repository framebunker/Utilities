using System.IO;

using UnityEditor;
using UnityEngine;


namespace framebunker
{
	[InitializeOnLoad]
	internal static class EditorPlatform
	{
		static EditorPlatform ()
		{
			Platform.BuilderBinaryPath = EditorApplication.applicationContentsPath + "/Mono/bin/xbuild" + (Platform.IsWindows ? ".bat" : "");
			Platform.MonoBinaryPath = EditorApplication.applicationContentsPath + "/MonoBleedingEdge/bin/mono" + Platform.BinaryExtension;
			Platform.MonoLibraryPath = EditorApplication.applicationContentsPath + "/MonoBleedingEdge/lib/mono/4.5";

			Platform.UnityLibraryPath = EditorApplication.applicationContentsPath + "/Managed";
			Platform.UnitySplitLibraryPath = EditorApplication.applicationContentsPath + "/Managed/UnityEngine";
			Platform.ProjectFolderPath = Path.GetDirectoryName (Application.dataPath) ?? "";
			Platform.TempPath = Platform.ConvertUnityPath (Platform.ProjectFolderPath + "/Temp/");
		}
	}
}
