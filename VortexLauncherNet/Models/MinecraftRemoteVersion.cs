using Newtonsoft.Json;
using System.Windows;

namespace VortexLauncherNet.Models;

internal class MinecraftRemoteVersions
{
	[JsonProperty("latest")]
	public MinecraftLatestVersion Latest { get; set; } = new();

	[JsonProperty("versions")]
	public List<MinecraftRemoteVersion> Versions { get; set; } = new();
	/*
	{
		"latest": {
			"release": "1.21.1",
			"snapshot": "1.21.1"
		},
		"versions": [
			{
				"id": "1.21.1",
				"releaseTime": "2024-08-08T12:24:45+00:00",
				"time": "2024-08-08T12:40:52+00:00",
				"type": "release",
				"url": "https://piston-meta.mojang.com/v1/packages/94f420093e771cd1e72614184736b044c747a8df/1.21.1.json"
			},
			{
				"id": "1.21.1-rc1",
				"releaseTime": "2024-08-07T14:29:18+00:00",
				"time": "2024-08-08T12:09:28+00:00",
				"type": "snapshot",
				"url": "https://piston-meta.mojang.com/v1/packages/55e14a678982bee4be0ab149f831af572e3afaed/1.21.1-rc1.json"
			},
			...
		]
	}
	*/
}

internal class MinecraftLatestVersion
{
	[JsonProperty("release")]
	public string Release { get; set; } = "";

	[JsonProperty("snapshot")]
	public string Snapshot { get; set; } = "";
}

internal class MinecraftRemoteVersion
{
	[JsonProperty("id")]
	public string Id { get; set; } = "";

	[JsonProperty("type")]
	public string Type { get; set; } = "";

	[JsonProperty("url")]
	public string Url { get; set; } = "";

	[JsonIgnore]
	public string DisplayValue => $"{Id} ({Type})";

	[JsonIgnore]
	public FontWeight ItemFontStyle => Type == "release" ? FontWeights.Bold : FontWeights.Normal;
}
