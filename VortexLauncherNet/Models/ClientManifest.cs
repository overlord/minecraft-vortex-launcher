using Newtonsoft.Json.Linq;

namespace VortexLauncherNet.Models;

internal class ClientManifest
{
	public string Path { get; init; } = "";
	public string Version { get; init; } = "";
	public JObject? Data { get; init; }
}
