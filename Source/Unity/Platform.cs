using System.IO;
using UnityEditor;
using UnityEngine;


namespace framebunker
{
	public static class Platform
	{
		public static bool IsWindows { get; }
		public static bool IsMacOS { get; }
		public static bool IsLinux { get; }
		[NotNull] public static string BinaryExtension { get; }
		public static char PathSeparator { get; }
		public static char PathEnvironmentSeparator { get; }
		[NotNull] public static string FileProtocolPrefix { get; }

		/// <summary>
		/// Path to the xbuild/msbuild binary (editor only)
		/// </summary>
		[NotNull] public static string BuilderBinaryPath { get; }
		/// <summary>
		/// Path to the mono binary (editor only)
		/// </summary>
		[NotNull] public static string MonoBinaryPath { get; }
		/// <summary>
		/// The mono library path (editor only)
		/// </summary>
		[NotNull] public static string MonoLibraryPath { get; }

		/// <summary>
		/// The Unity library path (editor only)
		/// </summary>
		[NotNull] public static string UnityLibraryPath { get; }
		/// <summary>
		/// The path to the split Unity libraries (editor only)
		/// </summary>
		[NotNull] public static string UnitySplitLibraryPath { get; }
		/// <summary>
		/// Path to the project folder (editor only)
		/// </summary>
		[NotNull] public static string ProjectFolderPath { get; }

		/// <summary>
		/// Path to the temp folder
		/// </summary>
		[NotNull] public static string TempPath { get; }


		static Platform ()
		{
			IsWindows =
				Application.platform == RuntimePlatform.WindowsEditor ||
				Application.platform == RuntimePlatform.WindowsPlayer;
			IsMacOS =
				Application.platform == RuntimePlatform.OSXEditor ||
				Application.platform == RuntimePlatform.OSXPlayer;
			IsLinux =
				Application.platform == RuntimePlatform.LinuxEditor ||
				Application.platform == RuntimePlatform.LinuxPlayer;

			if (IsWindows)
			{
				BinaryExtension = ".exe";
				PathSeparator = '\\';
				PathEnvironmentSeparator = ';';
				FileProtocolPrefix = "file:///";
			}
			else
			{
				BinaryExtension = "";
				PathSeparator = '/';
				PathEnvironmentSeparator = ':';
				FileProtocolPrefix = "file://";
			}

			if (Application.isEditor)
			{
				BuilderBinaryPath = EditorApplication.applicationContentsPath + "/Mono/bin/xbuild" + (IsWindows ? ".bat" : "");
				MonoBinaryPath = EditorApplication.applicationContentsPath + "/MonoBleedingEdge/bin/mono" + BinaryExtension;
				MonoLibraryPath = EditorApplication.applicationContentsPath + "/MonoBleedingEdge/lib/mono/4.5";

				UnityLibraryPath = EditorApplication.applicationContentsPath + "/Managed";
				UnitySplitLibraryPath = EditorApplication.applicationContentsPath + "/Managed/UnityEngine";
				ProjectFolderPath = Path.GetDirectoryName (Application.dataPath) ?? "";
				TempPath = ConvertUnityPath (ProjectFolderPath + "/Temp/");
			}
			else
			{
				BuilderBinaryPath = "";
				MonoBinaryPath = "";
				MonoLibraryPath = "";

				UnityLibraryPath = "";
				UnitySplitLibraryPath = "";
				ProjectFolderPath = "";
				TempPath = Path.GetTempPath ();
			}
		}


		[NotNull] public static string ConvertUnityPath ([NotNull] string unityPath)
		{
			return IsWindows
				? unityPath.Replace ('/', '\\')
				: unityPath;
		}


		[NotNull] public static string GetTempFileName ([NotNull] string extension = "")
		{
			string tempPath = TempPath, tempName, result;

			do
			{
				tempName = System.Guid.NewGuid ().ToString ();
			}
			while (File.Exists (result = tempPath + tempName + extension));

			return result;
		}
	}
}
