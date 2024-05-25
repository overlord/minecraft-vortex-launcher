using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.IO;
using System.Net.Http;
using VortexLauncherNet.Models;

namespace VortexLauncherNet.Services;

internal class MinecraftService
{
	private static HttpClient _httpClient = new HttpClient();

	public ObservableCollection<string> FindInstalledVersions(string minecraftRootDir)
	{
		var res = new ObservableCollection<string>();

		if (!Directory.Exists(minecraftRootDir))
		{
			throw new DirectoryNotFoundException($"Directory '{minecraftRootDir}' not found.");
		}

		var versionsRootDir = Path.Combine(minecraftRootDir, "versions");

		var versionDirs = Directory.GetDirectories(versionsRootDir);
		foreach (var versionDir in versionDirs)
		{
			var versionDirName = Path.GetFileName(versionDir);
			var versionManifestJson = Path.Combine(versionDir, $"{versionDirName}.json");
			if (File.Exists(versionManifestJson))
			{
				var fi = new FileInfo(versionManifestJson);
				if (fi.Length > 1)
				{
					res.Add(versionDirName);
				}
			}
		}

		return res;
	}

	public async Task<ObservableCollection<MinecraftRemoteVersion>> GetVersions()
	{
		var data = await _httpClient.GetFromJsonAsync<MinecraftRemoteVersions>(
			"https://launchermeta.mojang.com/mc/game/version_manifest.json"
		);

		return new(data ?? new List<MinecraftRemoteVersion>());

		/*
		If OpenWindow(1, #PB_Ignore, #PB_Ignore, 200, 120, "Client Downloader")
			DisableGadget(downloadButton, 1)


			ComboBoxGadget(325, 5, 5, 190, 25)

			versionsDownloadGadget = 325
			CheckBoxGadget(110, 5, 40, 130, 20, "Show all versions")

			versionsTypeGadget = 110
			SetGadgetState(versionsTypeGadget, ReadPreferenceInteger("ShowAllVersions", versionsTypeDefault))
			CheckBoxGadget(817, 5, 60, 130, 20, "Redownload all files")

			downloadAllFilesGadget = 817
			SetGadgetState(downloadAllFilesGadget, ReadPreferenceInteger("RedownloadFiles", downloadAllFilesDefault))
			downloadVersionButton = ButtonGadget(#PB_Any, 5, 85, 190, 30, "Download")

			If IsThread(downloadThread) : DisableGadget(downloadVersionButton, 1) : EndIf

			versionsManifestString = PeekS(*FileBuffer, MemorySize(*FileBuffer), #PB_UTF8)
			FreeMemory(*FileBuffer)


			parseVersionsManifest(GetGadgetState(versionsTypeGadget))
		*/
	}
}
