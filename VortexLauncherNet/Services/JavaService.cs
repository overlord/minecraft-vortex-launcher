using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using VortexLauncherNet.Helpers;
using VortexLauncherNet.Models;

namespace VortexLauncherNet.Services;

internal class JavaService
{
	public ObservableCollection<JavaVersion> FindJava()
	{
		var res = new List<JavaVersion>();

		var javaPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		AddPath(javaPaths, Environment.GetEnvironmentVariable("ProgramW6432"), "Java");
		AddPath(javaPaths, Environment.GetEnvironmentVariable("PROGRAMFILES"), "Java");
		AddPath(javaPaths, Environment.GetEnvironmentVariable("programfiles(x86)"), "Java");
		AddPath(javaPaths, Path.Combine("C:", "Java"), "");
		AddPath(javaPaths, Path.Combine("C:", "JDK"), "");
		AddPath(javaPaths, Path.Combine("C:", "JDK"), "OpenJDK");
		AddPath(javaPaths, Path.Combine("C:", "JDK"), "OracleJDK");

		foreach (var javaPath in javaPaths)
		{
			var javaDirs = Directory.GetDirectories(javaPath);
			foreach (var javaDir in javaDirs)
			{
				var javaDirName = Path.GetFileName(javaDir);
				var javawExe = Path.Combine(javaDir, "bin", "javaw.exe");

				if (File.Exists(javawExe))
				{
					var version = javaDirName
						.Replace("jre-", "", StringComparison.OrdinalIgnoreCase)
						.Replace("jdk-", "", StringComparison.OrdinalIgnoreCase);

					res.Add(new JavaVersion
					{
						Path = javawExe,
						DisplayName = $"{version} - {javawExe}",
					});
				}
			}
		}

		return new ObservableCollection<JavaVersion>(res.OrderBy(x => x.DisplayName));
	}

	public void ValidateJavaw(string? workingDir, string javawPath)
	{
		var checkJava = new ProcessStartInfo
		{
			FileName = javawPath,
			UseShellExecute = false,
			WorkingDirectory = workingDir,
		};
		checkJava.ArgumentList.Add("-version");

		var process = Process.Start(checkJava)!;
		process.WaitForExit();
		if (process.ExitCode != 0)
		{
			throw new VortexException($"Java not found at '{javawPath}'.\nCheck if Java installed.\nOr check if path to Java binary is correct.");
		}
	}

	public void Launch(string javawPath, string args, string workingDir) => Task.Run(() =>
	{
		var launchArgs = new ProcessStartInfo
		{
			FileName = javawPath,
			Arguments = args,
			UseShellExecute = false,
			WorkingDirectory = workingDir,
			RedirectStandardError = true,
			RedirectStandardOutput = true,
		};

		var process = Process.Start(launchArgs)!;
		var output = process.StandardOutput.ReadToEnd();
		var error = process.StandardError.ReadToEnd();
		process.WaitForExit();

		if (process.ExitCode != 0)
		{
			FileHelper.AppendFile("launch_string.txt", $"ExitCode: {process.ExitCode}\n{error}");
			//throw new VortexException($"Minecraft launch failed!");
		}
	});


	// ------------------------------

	private static void AddPath(HashSet<string> javaPaths, string? path1, string path2)
	{
		if (!string.IsNullOrEmpty(path1) && Directory.Exists(path1))
		{
			var javaPath = Path.Combine(path1, path2);
			if (Directory.Exists(javaPath))
			{
				javaPaths.Add(javaPath);
			}
		}
	}
}
