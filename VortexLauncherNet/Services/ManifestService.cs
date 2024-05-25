using System.IO;
using Newtonsoft.Json.Linq;
using VortexLauncherNet.Models;

namespace VortexLauncherNet.Services;

internal class ManifestService
{
	private string? _minecraftRootDir;

	// ------------------------------

	public void SetMinecraftRootDir(string minecraftRootDir)
	{
		_minecraftRootDir = minecraftRootDir;
	}

	public ClientManifest XReadManifest(string version)
	{
		var manifestPath = XClientManifestPath(version);
		return new ClientManifest
		{
			Path = manifestPath,
			Version = version,
			Data = ReadJsonFile(manifestPath),
		};
	}

	public ClientManifest? ReadInheritedManifest(ClientManifest manifest)
	{
		var inheritsFrom = manifest.Data["inheritsFrom"];
		var jar = manifest.Data["jar"];

		if (inheritsFrom != null)
		{
			return XReadManifest(inheritsFrom.Value<string>());
		}
		else if (jar != null)
		{
			return XReadManifest(jar.Value<string>());
		}

		return null;
	}

	// ------------------------------

	public (List<string> ClientArguments, List<string> JvmArguments) ExtractArguments(ClientManifest? manifest)
	{
		var clientArguments = new List<string>();
		var jvmArguments = new List<string>();

		if (manifest == null)
		{
			return (clientArguments, jvmArguments);
		}

		var minecraftArguments = manifest.Data["minecraftArguments"];
		var arguments = manifest.Data["arguments"];

		if (minecraftArguments != null)
		{
			clientArguments.Add(minecraftArguments.Value<string>());
		}
		else if (arguments != null)
		{
			var game = arguments["game"];
			foreach (var gameArg in game)
			{
				if (gameArg != null && gameArg is not JObject)
				{
					clientArguments.Add(gameArg.Value<string>());
				}
			}

			var jvm = arguments["jvm"];
			if (jvm != null)
			{
				foreach (var jvmArg in jvm)
				{
					if (jvmArg != null && jvmArg is not JObject)
					{
						jvmArguments.Add($"\"{jvmArg.Value<string>()}\"");
					}
				}
			}
		}

		return (clientArguments, jvmArguments);
	}

	public DateTime? ExtractReleaseTime(ClientManifest? manifest)
	{
		if (manifest == null)
		{
			return null;
		}

		return manifest.Data["releaseTime"]?.Value<DateTime>();
	}

	public string? ExtractAssetsIndex(ClientManifest? manifest)
	{
		if (manifest == null)
		{
			return null;
		}

		var releaseTime = ExtractReleaseTime(manifest);
		var assets = manifest.Data["assets"];

		if (assets != null)
		{
			return assets.Value<string>();
		}
		else if (releaseTime.HasValue && releaseTime.Value < new DateTime(2013, 06, 01))
		{
			return "pre-1.6";
		}
		else
		{
			return "legacy";
		}
	}
	public string? ExtractLogConfArgument(ClientManifest? manifest)
	{
		if (manifest == null)
		{
			return null;
		}

		var jLogging = manifest.Data["logging"];
		if (jLogging != null)
		{
			var jClient = jLogging["client"];
			if (jClient != null)
			{
				var jFile = jClient["file"];
				if (jFile != null)
				{
					var jId = jFile["id"].Value<string>();
					var path = Path.Combine("assets", "log_configs", jId);
					return $"-Dlog4j.configurationFile={path}";
				}
			}
		}

		return null;
	}

	// ------------------------------

	public string ClientManifestPath(string? version)
	{
		return Path.Combine(_minecraftRootDir, "versions", version!, $"{version}.json");
	}
	public string XClientManifestPath(string? version)
	{
		var clientManifestPath = ClientManifestPath(version);
		if (!File.Exists(clientManifestPath))
		{
			throw new VortexException($"Client manifest file is missing: '{clientManifestPath}'");
		}

		return clientManifestPath;
	}

	public string? JarPath(string? version)
	{
		if (string.IsNullOrEmpty(version))
		{
			return null;
		}

		return Path.Combine(_minecraftRootDir, "versions", version!, $"{version}.jar");
	}
	public string XJarPath(string? version)
	{
		var jarPath = JarPath(version);
		if (!File.Exists(jarPath))
		{
			throw new VortexException($"Client jar file is missing: '{jarPath}'");
		}
		return jarPath;
	}

	public string? NativesPath(string? version)
	{
		if (string.IsNullOrEmpty(version))
		{
			return null;
		}

		return Path.Combine(_minecraftRootDir, "versions", version!, "natives");
	}

	// ------------------------------

	private JObject ReadJsonFile(string path)
	{
		return JObject.Parse(File.ReadAllText(path));
	}

	// ------------------------------

	public string GetPlayerUuid(string playerName)
	{
		static byte H2B(string d) => Convert.FromHexString(d).Single();
		static string B2H(byte d) => Convert.ToHexString(new byte[] { d }).ToLowerInvariant();

		var uuid = GetMD5("OfflinePlayer:" + playerName);

		return
			uuid[..12] +
			B2H((byte)(H2B(uuid[12..14]) & 0x0f | 0x30)) +
			uuid[14..16] +
			B2H((byte)(H2B(uuid[16..18]) & 0x3f | 0x80)) +
			uuid[18..];
	}

	private static string GetMD5(string content)
	{
		using var md5 = System.Security.Cryptography.MD5.Create();

		byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(content);
		byte[] hashBytes = md5.ComputeHash(inputBytes);

		return Convert.ToHexString(hashBytes).ToLowerInvariant();
	}
}
