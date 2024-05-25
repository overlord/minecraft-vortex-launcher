using System.Net.Http.Json;
using System.Net.Http;
using VortexLauncherNet.Models;

namespace VortexLauncherNet.Services;

internal class MinecraftRemoteService
{
	private readonly HttpClient _httpClient;

	public MinecraftRemoteService(
		HttpClient httpClient
	)
	{
		_httpClient = httpClient;
		
	}

	public async Task<MinecraftRemoteVersions> GetVersions()
	{
		var data = await _httpClient.GetFromJsonAsync<MinecraftRemoteVersions>(
			"https://launchermeta.mojang.com/mc/game/version_manifest.json"
		);

		return data ?? new MinecraftRemoteVersions();

		/*
		If OpenWindow(1, #PB_Ignore, #PB_Ignore, 200, 120, "Client Downloader")

			CheckBoxGadget(110, 5, 40, 130, 20, "Show all versions")

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
