using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.IO;
using VortexLauncherNet.Helpers;
using VortexLauncherNet.Models;
using VortexLauncherNet.MVVM;
using VortexLauncherNet.Services;

namespace VortexLauncherNet.ViewModels;

internal class MainWindowVM
{
	private readonly JavaService _javaService;
	private readonly ManifestService _manifestService;

	public OptionsVM Options { get; set; } = new();

	public RelayCommand PlayCommand { get; }

	public MainWindowVM()
	{
	}

	public MainWindowVM(
		JavaService javaService,
		ManifestService manifestService
	)
	{
		PlayCommand = new RelayCommand(_ => Play());
		_javaService = javaService;
		_manifestService = manifestService;
	}

	// ------------------------------

	private void Play()
	{
		_manifestService.SetMinecraftRootDir(Options.MinecraftRootDir.Value!);

		var customLaunchArgumentsDefault = "-Xss1M -XX:+UnlockExperimentalVMOptions -XX:+UseG1GC -XX:G1NewSizePercent=20 -XX:G1ReservePercent=20 -XX:MaxGCPauseMillis=50 -XX:G1HeapRegionSize=32M";
		var customLaunchArgumentsDefaultOld = "-XX:+UseConcMarkSweepGC -XX:+CMSIncrementalMode -XX:-UseAdaptiveSizePolicy -Xmn128M";

		var ramAmount = Options.RamAmount.Value;
		var clientVersion = Options.MinecraftVersionSelected.Value;
		var playerName = Options.PlayerName.Value?.Replace(" ", "") ?? "";
		var javaBinaryPath = Options.JavaVersionSelected.Value!.Path;
		var downloadMissingLibraries = false; //!!! ReadPreferenceInteger("DownloadMissingLibs", downloadMissingLibrariesDefault);

		/*if (forceDownloadMissingLibraries){
		downloadMissingLibraries = true;
		}*/

		Validator.ValidatePlayerName(Options.PlayerName.Value);
		Validator.ValidateRam(Options.RamAmount.Value);

		/*WritePreferenceString("Name", playerName);
		WritePreferenceString("Ram", ramAmount);
		WritePreferenceString("ChosenVer", clientVersion);*/

		_javaService.ValidateJavaw(Options.WorkingDir.Value, javaBinaryPath);

		var manifest = _manifestService.XReadManifest(clientVersion);
		var inheritedManifest = _manifestService.ReadInheritedManifest(manifest);

		var jarFile = _manifestService.XJarPath(manifest.Version);
		jarFile = Path.GetRelativePath(Options.MinecraftRootDir.Value!, jarFile);

		var nativesPath =
			_manifestService.NativesPath(inheritedManifest?.Version) ??
			_manifestService.NativesPath(clientVersion) ??
			throw new VortexException($"Natives not found");
		nativesPath = Path.GetRelativePath(Options.MinecraftRootDir.Value!, nativesPath);

		var assetsIndex =
			_manifestService.ExtractAssetsIndex(inheritedManifest) ??
			_manifestService.ExtractAssetsIndex(manifest);

		var releaseTime =
			_manifestService.ExtractReleaseTime(inheritedManifest) ??
			_manifestService.ExtractReleaseTime(manifest) ??
			throw new VortexException($"ReleaseTime not found");

		var clientArguments = new List<string>();
		var jvmArguments = new List<string>();

		var args = _manifestService.ExtractArguments(manifest);
		clientArguments.AddRange(args.ClientArguments);
		jvmArguments.AddRange(args.JvmArguments);

		var inheritedArgs = _manifestService.ExtractArguments(inheritedManifest);
		clientArguments.AddRange(inheritedArgs.ClientArguments);
		jvmArguments.AddRange(inheritedArgs.JvmArguments);

		var logConfArgument =
			_manifestService.ExtractLogConfArgument(inheritedManifest) ??
			_manifestService.ExtractLogConfArgument(manifest);

		var librariesString =
			ParseLibraries(manifest) +
			ParseLibraries(inheritedManifest);

		var clientMainClass = manifest.Data["mainClass"];

		var uuid = _manifestService.GetPlayerUuid(playerName);

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

		var customLaunchArguments = releaseTime < new DateTime(2013, 06, 01)
			? customLaunchArgumentsDefaultOld
			: customLaunchArgumentsDefault;

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
			.Replace("${game_directory}", $"\"{Options.MinecraftRootDir.Value}\"")
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
			FileHelper.WriteFile("launch_string.txt", $"WorkingDir: {Options.MinecraftRootDir.Value}\nJavaBinaryPath: {javaBinaryPath}\nArguments:\n{fullLaunchString}");
		}

		_javaService.Launch(javaBinaryPath, fullLaunchString, Options.MinecraftRootDir.Value!);

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
