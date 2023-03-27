using System;
using System.IO;
using System.Linq;
using UnityEditor;
using Microsoft.Win32;
using NeXt.Vdf;
using UnityEditor.Build;
using UnityEngine;

namespace OwlcatModification.Editor.Setup
{
	public static class ProjectSetup
	{
		private const string WhDirectoryKey = "whrt_directory";
		private const string WhRepositoryKey = "wh_repository_directory";

		[MenuItem("Modification Tools/ Setup project with Steam", false, -1000)]
		public static void SteamSetup()
		{
			EditorUtility.DisplayProgressBar("Setup project", "trying find in steam automatically", 0);

			var whInstallDirName = "";
			var whSteamId = "2186680";
			var steamInstallPath = "";
			string steamLibrarySettingsPath = null;
			
			//Find Steam library settings, which contain all steam library directories
			try
			{
				RegistryKey steamInfo = Registry.CurrentUser.OpenSubKey("Software\\Valve\\Steam");
				steamInstallPath = steamInfo.GetValue("SteamPath").ToString();
				Debug.LogError($"Steam path : {steamInstallPath}");
				steamLibrarySettingsPath =
					Path.Combine(steamInstallPath, "steamapps", $"libraryfolders.vdf");
			}
			catch (Exception e)
			{
				EditorUtility.ClearProgressBar();
				Debug.LogException(e);
				Setup();
				return;
			}
			
			//Parse settings file to find paths of installed games
			if (File.Exists(steamLibrarySettingsPath))
			{
				var deserializer = VdfDeserializer.FromFile(steamLibrarySettingsPath);
				var root = deserializer.Deserialize();
				Debug.LogError(root.Type);
				var table = (VdfTable)root;
				if (table.Count < 1)
				{
					EditorUtility.ClearProgressBar();
					Debug.LogError("Something went wrong parsing Steam internals...");
					Setup();
					return;
				}

				string steamLibraryPathNeeded = null;
				string steamAppManifestPath = null;
				
				//Try to find app manifest file in library path
				for (int i = 0; i < table.Count; i++)
				{
					var steamLibraryPath = (VdfTable)table[i];
					var pathValue = ((VdfString)steamLibraryPath["path"]).Content;
					Debug.LogError($"Got path: {pathValue}");
					var manifestPossiblePath = Path.Combine(pathValue, "steamapps", $"appmanifest_{whSteamId}.acf");
					Debug.LogError(manifestPossiblePath);
					if (!File.Exists(manifestPossiblePath))
					{
						Debug.LogError($"No file ${manifestPossiblePath}. Looking further...");
						continue;
					}
					Debug.LogError($"Got the manifest: {manifestPossiblePath}");
					steamAppManifestPath = manifestPossiblePath;
					steamLibraryPathNeeded = Path.Combine( pathValue, "steamapps", "common");
					break;
				}

				if (string.IsNullOrEmpty(steamAppManifestPath))
				{
					EditorUtility.ClearProgressBar();
					Debug.LogError("Steam set up failed.");
					Setup();
					return;
				}
				
				//Parse app manifest to get game installation directory.
				var lines = File.ReadLines(steamAppManifestPath);
				foreach (var line in lines)
				{
					if (!line.Contains("installdir")) continue;
					var splitLine = line.Replace("\t", "");
					splitLine = splitLine.Replace("installdir", "");
					whInstallDirName = splitLine.Replace("\"", "");

					whInstallDirName = Path.Combine(steamLibraryPathNeeded, whInstallDirName);
					Debug.LogError($"Game install dir path: {whInstallDirName}");
					break;
				}
				
			}
			else
			{
				Debug.LogError("Couldn't find a file steam library settings file!");
				EditorUtility.ClearProgressBar();
				Setup();
				return;
			}
			
			//Get dlls of WH Rogue Trader
			EditorPrefs.SetString(WhDirectoryKey, whInstallDirName);
			SetupAssemblies(whInstallDirName);
			
			EditorUtility.ClearProgressBar();
		}

		[MenuItem("Modification Tools/ Internal/ Setup project", false, -1000)]
		public static void DevSetup()
		{
			try
			{
				EditorUtility.DisplayProgressBar("Setup project", "", 0);

				var whrtDirectory = EditorUtility.OpenFolderPanel(
					"Warhammer 40,000: Rogue Trader repository folder", EditorPrefs.GetString(WhDirectoryKey, ""), "");
				
				if (!Directory.Exists(whrtDirectory))
				{
					throw new Exception("Repository folder is missing!");
				}
				
				EditorPrefs.SetString(WhRepositoryKey, whrtDirectory);
				SetupAssemblies(whrtDirectory);
			}
			catch (Exception e)
			{
				EditorUtility.DisplayDialog("Error!", $"{e.Message}\n\n{e.StackTrace}", "Close");
			}
			finally
			{
				EditorUtility.ClearProgressBar();
			}
		}

		[MenuItem("Modification Tools/Setup project", false, -1000)]
		public static void Setup()
		{
			try
			{
				EditorUtility.DisplayProgressBar("Setup project", "", 0);

				var whrtDirectory = EditorUtility.OpenFolderPanel(
					"Warhammer 40,000: Rogue Trader", EditorPrefs.GetString(WhDirectoryKey, ""), "");
				
				if (!Directory.Exists(whrtDirectory))
				{
					throw new Exception("WHRT folder is missing!");
				}
				
				EditorPrefs.SetString(WhDirectoryKey, whrtDirectory);
				SetupAssemblies(whrtDirectory);
			}
			catch (Exception e)
			{
				EditorUtility.DisplayDialog("Error!", $"{e.Message}\n\n{e.StackTrace}", "Close");
			}
			finally
			{
				EditorUtility.ClearProgressBar();
			}
		}

		private static void SetupAssemblies(string whrtDirectory)
		{
			string[] skipAssemblies = {
				"RogueTrader.SharedTypes.dll",
				"mscorlib.dll",
				"Owlcat.SharedTypes.dll",
				"Autodesk.Fbx.dll",
				"Cinemachine.dll",
				"FbxBuildTestAssets.dll",
				"clipper_library.dll",
				"com.unity.cinemachine.editor.dll",
				"Autodesk.Fbx.Editor.dll",
				"PsdPlugin.dll",
				"Analytics.dll"
			};
			EditorUtility.DisplayProgressBar("Setup project", "copying dlls to project", 0.5f);
			
			const string targetAssembliesDirectory = "Assets/RogueTraderAssemblies";
			Directory.CreateDirectory(targetAssembliesDirectory);
			
			string assembliesDirectory = Path.Combine(whrtDirectory, "WH40KRT_Data/Managed");
			Debug.LogError($"ASM dir: {assembliesDirectory}");
			foreach (string assemblyPath in Directory.GetFiles(assembliesDirectory, "*.dll"))
			{
				//skip all default unity dlls
				if (assemblyPath.Contains("Unity.") && !assemblyPath.Contains("AK.Wwise"))
				{
					continue;
				}
				if (assemblyPath.Contains("UnityEngine.") 
				    || assemblyPath.Contains("UnityEditor."))
				{
					continue;
				}
				if (skipAssemblies.Any(assemblyPath.EndsWith))
				{
					continue;
				}

				string filename = Path.GetFileName(assemblyPath);
				File.Copy(assemblyPath, Path.Combine(targetAssembliesDirectory, filename), true);
			}
			
			AssetDatabase.Refresh();
		}
	}
}