using System.IO;
using System.Text;

namespace VortexLauncherNet.Helpers;

internal static class FileHelper
{
	public static void WriteFile(string path, string content)
	{
		using var fs = File.Open(path, FileMode.Create);
		using var sw = new StreamWriter(fs, Encoding.UTF8);
		sw.WriteLine(content);
	}

	public static void AppendFile(string path, string content)
	{
		using var fs = File.Open(path, FileMode.Append);
		using var sw = new StreamWriter(fs, Encoding.UTF8);
		sw.WriteLine(content);
	}
}
