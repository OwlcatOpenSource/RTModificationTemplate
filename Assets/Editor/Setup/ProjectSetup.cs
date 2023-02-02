using System;
using System.IO;
using System.Linq;
using UnityEditor;
using Microsoft.Win32;
using UnityEditor.Build;
using UnityEngine;

namespace OwlcatModification.Editor.Setup
{
	public static class ProjectSetup
	{
		private const string WhDirectoryKey = "whrt_directory";
		private const string WhRepositoryKey = "wh_repository_directory";


		[MenuItem("Modification Tools/ Internal/ Enable Dev Mode", false, -1000)]
		public static void EndableDevMode()
		{
			var defines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone);
			defines = $"OWLCAT_MODS_DEV;{defines}";
			PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Standalone, defines);
		}
		
		[MenuItem("Modification Tools/ Internal/ Disable Dev Mode", false, -1000)]
		public static void DisableDevMode()
		{
			var defines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone);
			defines = defines.Replace("OWLCAT_MODS_DEV;", "");
			PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Standalone, defines);
		}
		
		[MenuItem("Modification Tools/ Setup project with Steam", false, -1000)]
		public static void SteamSetup()
		{
			EditorUtility.DisplayProgressBar("Setup project", "trying find in steam automatically", 0);
			var whInstallDirName = "";
			//Wh 40k Rogue Trader Steam app id
			// var whSteamId = "2186680";
			//The New OCG Demo Steam app id
			var whSteamId = "1613010";
			var steamInstallPath = "";
			
			try
			{
				RegistryKey steamInfo = Registry.CurrentUser.OpenSubKey("Software\\Valve\\Steam");
				steamInstallPath = steamInfo.GetValue("SteamPath").ToString();
				Debug.LogError($"Steam path : {steamInstallPath}");
				var steamAppManifestPath =
					Path.Combine(steamInstallPath, "steamapps", $"appmanifest_{whSteamId}.acf");
				Debug.LogError($"appmanifest path: {steamAppManifestPath}");
				if (File.Exists(steamAppManifestPath))
				{
					Debug.LogError("App manifest file exists");
					var lines = File.ReadLines(steamAppManifestPath);
					foreach (var line in lines)
					{
						if (!line.Contains("installdir")) continue;
						// var splitLine = line.Replace(" ", "");
						var splitLine = line.Replace("\t", "");
						splitLine = splitLine.Replace("installdir", "");
						whInstallDirName = splitLine.Replace("\"", "");

						Debug.LogError($"Split: {whInstallDirName}");
						break;
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				Setup();
			}

			if (string.IsNullOrEmpty(whInstallDirName))
			{
				Setup();
			}

			var whDllPath = Path.Combine(steamInstallPath, $"steamapps\\common\\{whInstallDirName}");
			EditorPrefs.SetString(WhDirectoryKey, whDllPath);
			SetupAssemblies(whDllPath);
			
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

		private static void SetupExternalPluginsDlls(string whrtDirectory)
		{
			const string targetAssembliesDirectory = "Assets/RogueTraderAssemblies";
			var assembliesDirectory = Path.Combine(whrtDirectory, "Assets/Plugins/External");
			Debug.LogError($"External plugins dll search path: {assembliesDirectory}");
			if (!Directory.Exists(assembliesDirectory)) return;

			foreach (string assemblyPath in Directory.GetFiles(assembliesDirectory, "*.dll", SearchOption.AllDirectories))
			{
				string filename = Path.GetFileName(assemblyPath);
				File.Copy(assemblyPath, Path.Combine(targetAssembliesDirectory, filename), true);
			}
		}
		
		private static void SetupAssemblies(string whrtDirectory)
		{
			string[] skipAssemblies = {
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

			#if OWLCAT_MODS_DEV
			
			SetupExternalPluginsDlls(whrtDirectory);
			string assembliesDirectory = Path.Combine(whrtDirectory, "Library/ScriptAssemblies");
			
			#else
			string assembliesDirectory = Path.Combine(whrtDirectory, "WH40KRT_Data/Managed");
			#endif
			Debug.LogError($"ASM dir: {assembliesDirectory}");
			foreach (string assemblyPath in Directory.GetFiles(assembliesDirectory, "*.dll"))
			{
				//skip all default unity dlls
				if (assemblyPath.Contains("Unity.") 
				    || assemblyPath.Contains("UnityEngine.") 
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