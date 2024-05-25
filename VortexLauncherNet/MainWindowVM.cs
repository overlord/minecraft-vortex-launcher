using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.IO;
using VortexLauncherNet.Helpers;
using VortexLauncherNet.Models;
using VortexLauncherNet.MVVM;
using VortexLauncherNet.Services;

namespace VortexLauncherNet;

internal class MainWindowVM
{
	private readonly JavaService _javaService;

	public Observable<string> WorkingDir { get; set; } = new();
	public Observable<string> MinecraftRootDir { get; set; } = new();

	public Observable<string> PlayerName { get; set; } = new();
	public Observable<int> RamAmount { get; set; } = new();

	public ObservableCollection<string> MinecraftVersions { get; set; } = new();
	public Observable<string> MinecraftVersionSelected { get; set; } = new();

	public ObservableCollection<JavaVersion> JavaVersions { get; set; } = new();
	public Observable<JavaVersion> JavaVersionSelected { get; set; } = new();

	public RelayCommand PlayCommand { get; }

	public MainWindowVM(
		JavaService javaService
	)
	{
		PlayCommand = new RelayCommand(_ => Play());
		_javaService = javaService;
	}

	// ------------------------------

	private void Play()
	{
		var manifestService = new ManifestService(MinecraftRootDir.Value!);

		var customLaunchArgumentsDefault = "-Xss1M -XX:+UnlockExperimentalVMOptions -XX:+UseG1GC -XX:G1NewSizePercent=20 -XX:G1ReservePercent=20 -XX:MaxGCPauseMillis=50 -XX:G1HeapRegionSize=32M";
		var customOldLaunchArgumentsDefault = "-XX:+UseConcMarkSweepGC -XX:+CMSIncrementalMode -XX:-UseAdaptiveSizePolicy -Xmn128M";

		var ramAmount = RamAmount.Value;
		var clientVersion = MinecraftVersionSelected.Value;
		var playerName = PlayerName.Value?.Replace(" ", "") ?? "";
		var javaBinaryPath = JavaVersionSelected.Value!.Path;
		var downloadMissingLibraries = false; //!!! ReadPreferenceInteger("DownloadMissingLibs", downloadMissingLibrariesDefault);

		/*if (forceDownloadMissingLibraries){
		downloadMissingLibraries = true;
		}*/

		Validator.ValidatePlayerName(PlayerName.Value);
		Validator.ValidateRam(RamAmount.Value);

		/*WritePreferenceString("Name", playerName);
		WritePreferenceString("Ram", ramAmount);
		WritePreferenceString("ChosenVer", clientVersion);*/

		_javaService.ValidateJavaw(WorkingDir.Value, javaBinaryPath);

		var manifest = manifestService.XReadManifest(clientVersion);
		var inheritedManifest = manifestService.ReadInheritedManifest(manifest);

		var jarFile = manifestService.XJarPath(manifest.Version);
		jarFile = Path.GetRelativePath(MinecraftRootDir.Value!, jarFile);

		var nativesPath =
			manifestService.NativesPath(inheritedManifest?.Version) ??
			manifestService.NativesPath(clientVersion) ??
			throw new VortexException($"Natives not found");
		nativesPath = Path.GetRelativePath(MinecraftRootDir.Value!, nativesPath);

		var assetsIndex =
			manifestService.ExtractAssetsIndex(inheritedManifest) ??
			manifestService.ExtractAssetsIndex(manifest);

		var releaseTime =
			manifestService.ExtractReleaseTime(inheritedManifest) ??
			manifestService.ExtractReleaseTime(manifest) ??
			throw new VortexException($"ReleaseTime not found");

		var clientArguments = new List<string>();
		var jvmArguments = new List<string>();

		var args = manifestService.ExtractArguments(manifest);
		clientArguments.AddRange(args.ClientArguments);
		jvmArguments.AddRange(args.JvmArguments);

		var inheritedArgs = manifestService.ExtractArguments(inheritedManifest);
		clientArguments.AddRange(inheritedArgs.ClientArguments);
		jvmArguments.AddRange(inheritedArgs.JvmArguments);

		var logConfArgument =
			manifestService.ExtractLogConfArgument(inheritedManifest) ??
			manifestService.ExtractLogConfArgument(manifest);

		var librariesString =
			ParseLibraries(manifest) +
			ParseLibraries(inheritedManifest);

		var clientMainClass = manifest.Data["mainClass"];

		var uuid = manifestService.GetPlayerUuid(playerName);

		/*!!! if (assetsIndex == "pre-1.6" || assetsIndex == "legacy")
		{
			assetsToResources(assetsIndex);
		}*/

		/*if (downloadMissingLibraries)
		{
			downloadFiles(0);
		}*/

		if (jvmArguments.Count <= 0)
		{
			jvmArguments.Add($"\"-Djava.library.path={nativesPath}\"");
			jvmArguments.Add($"-cp \"{librariesString}{jarFile}\"");
		}

		string customLaunchArguments;

		if (releaseTime < new DateTime(2013, 06, 01))
		{
			customLaunchArguments = customOldLaunchArgumentsDefault;
		}
		else
		{
			customLaunchArguments = customLaunchArgumentsDefault;
		}

		/*if (ReadPreferenceInteger("UseCustomParameters", useCustomParamsDefault))
		{
			customLaunchArguments = ReadPreferenceString("LaunchArguments", customLaunchArgumentsDefault);
		}*/

		var fullLaunchString =
			$"-Xmx{ramAmount}M " +
			customLaunchArguments +
			" -Dlog4j2.formatMsgNoLookups=true " +
			logConfArgument + " " +
			string.Join(" ", jvmArguments) + " " +
			clientMainClass + " " +
			string.Join(" ", clientArguments);

		fullLaunchString = fullLaunchString
			.Replace("${auth_player_name}", playerName)
			.Replace("${version_name}", clientVersion)
			.Replace("${game_directory}", $"\"{MinecraftRootDir.Value}\"")
			.Replace("${assets_root}", "assets")
			.Replace("${auth_uuid}", uuid)
			.Replace("${auth_access_token}", "00000000000000000000000000000000")
			.Replace("${clientid}", "0000")
			.Replace("${auth_xuid}", "0000")
			.Replace("${user_properties}", "{}")
			.Replace("${user_type}", "mojang")
			.Replace("${version_type}", "release")
			.Replace("${assets_index_name}", assetsIndex)
			.Replace("${auth_session}", "00000000000000000000000000000000")
			.Replace("${game_assets}", "resources")
			.Replace("${classpath}", librariesString + jarFile)
			.Replace("${library_directory}", "libraries")
			.Replace("${classpath_separator}", ";")
			.Replace("${natives_directory}", nativesPath)
			.Replace("${launcher_name}", "VortexLauncherNet")
			.Replace("${launcher_version}", "1.0")
			//.Replace("\"-Dminecraft.launcher.brand=${launcher_name}\"", "")
			//.Replace("\"-Dminecraft.launcher.version=${launcher_version}\"", "")
			;

		//var saveLaunchString = ReadPreferenceInteger("SaveLaunchString", saveLaunchStringDefault);
		if (true)
		{
			FileHelper.WriteFile("launch_string.txt", $"WorkingDir: {MinecraftRootDir.Value}\nJavaBinaryPath: {javaBinaryPath}\nArguments:\n{fullLaunchString}");
		}

		_javaService.Launch(javaBinaryPath, fullLaunchString, MinecraftRootDir.Value!);

		/*if (!ReadPreferenceInteger("KeepLauncherOpen", keepLauncherOpenDefault))
		{
			Break;
		}*/
	}

	// ------------------------------

	private static string ParseLibraries(ClientManifest? manifest, int prepareForDownload = 0)
	{
		if (manifest == null)
		{
			return "";
		}

		var allowLib = 0;
		string libsString = "";

		/*if (prepareForDownload == 1)
		{
			downloadListFile = OpenFile(PB_Any, tempDirectory + "vlauncher_download_list.txt");
			FileSeek(downloadListFile, Lof(downloadListFile), PB_Relative);
		}*/

		//!!! UseZipPacker();

		var jsonLibrariesArray = manifest.Data["libraries"];

		foreach (var jsonArrayElement in jsonLibrariesArray)
		{
			allowLib = 1;
			var jsonRulesOsName = "empty";

			var jsonRulesMember = jsonArrayElement["rules"];
			if (jsonRulesMember != null)
			{
				foreach (var jRule in jsonRulesMember)
				{
					var jsonRulesOsMember = jRule["os"];

					if (jsonRulesOsMember != null)
					{
						jsonRulesOsName = jsonRulesOsMember["name"].Value<string>();
					}

					if (jRule["action"].Value<string>() == "allow")
					{
						if (jsonRulesOsName != "empty" && jsonRulesOsName != "windows")
						{
							allowLib = 0;
						}
					}
					else if (jsonRulesOsName == "windows")
					{
						allowLib = 0;
					}
				}
			}

			if (allowLib == 1)
			{
				var libName = jsonArrayElement["name"].Value<string>();
				var libSplit = libName.Split(":");

				libName = Path.Combine(
					libSplit[0].Replace('.', Path.DirectorySeparatorChar),
					libSplit[1],
					libSplit[2],
					libSplit[1] + "-" + libSplit[2]
				);

				if (libSplit.Length > 3)
				{
					libName += "-" + libSplit[3];
				}

				/*if (prepareForDownload == 1) {
				  jsonDownloadsMember = GetJSONMember(jsonArrayElement, "downloads");

				  if (jsonDownloadsMember) {
					jsonArtifactsMember = GetJSONMember(jsonDownloadsMember, "artifact");
					jsonClassifiersMember = GetJSONMember(jsonDownloadsMember, "classifiers");

					if (jsonClassifiersMember) {
					  jsonNativesLinuxMember = GetJSONMember(jsonClassifiersMember, "natives-windows");

					  if (jsonNativesLinuxMember) {
						url = GetJSONString(GetJSONMember(jsonNativesLinuxMember, "url"));
						fileSize = GetJSONInteger(GetJSONMember(jsonNativesLinuxMember, "size"));

						libName + "-natives-windows";
					  }
					} else if (jsonArtifactsMember) {
					  url = GetJSONString(GetJSONMember(jsonArtifactsMember, "url"));
					  fileSize = GetJSONInteger(GetJSONMember(jsonArtifactsMember, "size"));
					}
				  } else {
					jsonUrlMember = GetJSONMember(jsonArrayElement, "url");

					if (jsonUrlMember) {
					  url = GetJSONString(jsonUrlMember) + ReplaceString(libName, "\\", "/") + ".jar";
					} else {
					  url = "https://libraries.minecraft.net/" + ReplaceString(libName, "\\", "/") + ".jar";
					}
				  }

				  WriteStringN(downloadListFile, url + "::" + "libraries\\" + libName + ".jar" + "::" + fileSize);
				}*/

				if (jsonArrayElement["natives"] == null)
				{
					libsString += "libraries\\" + libName + ".jar;";
				}
				/*else
				{
					if (libName[^15..] != "natives-windows")
					{
						zipFile = OpenPack(PB_Any, "libraries\\" + libName + "-natives-windows.jar");
					}
					else
					{
						zipFile = OpenPack(PB_Any, "libraries\\" + libName + ".jar");
					}

					if (zipFile)
					{
						CreateDirectoryRecursive("versions\\" + clientVersion + "\\natives");

						if (ExaminePack(zipFile))
						{
							while (NextPackEntry(zipFile))
							{
								if (PackEntryType(zipFile) = PB_Packer_File)
								{
									packFileName = PackEntryName(zipFile);

									if (FileSize("versions\\" + clientVersion + "\\natives\\" + packFileName) < 1)
									{
										UncompressPackFile(zipFile, "versions\\" + clientVersion + "\\natives\\" + packFileName); ;
									}
								}
							}
						}

						ClosePack(zipFile);
					}
				}*/
			}
		}


		/*if (prepareForDownload == 1)
		{
			CloseFile(downloadListFile);
		}*/

		return libsString;
	}
}
