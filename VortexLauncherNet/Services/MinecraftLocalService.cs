using System.Collections.ObjectModel;
using System.IO;

namespace VortexLauncherNet.Services;

internal class MinecraftLocalService
{
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
}
