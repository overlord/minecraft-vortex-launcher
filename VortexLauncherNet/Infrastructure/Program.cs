using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using VortexLauncherNet.Services;
using VortexLauncherNet.Views;
using VortexLauncherNet.ViewModels;

namespace VortexLauncherNet;

public class Program
{
	[STAThread]
	public static void Main()
	{
		var host = Host.CreateDefaultBuilder()
			.ConfigureServices(services =>
			{
				services.AddSingleton<App>();

				services.AddSingleton<MainWindow>();
				services.AddSingleton<MainWindowVM>();

				services.AddSingleton<ManifestService>();
				services.AddSingleton<MinecraftLocalService>();
				services.AddSingleton<MinecraftRemoteService>();
				services.AddSingleton<JavaService>();

				services.AddHttpClient();
			})
			.Build();
		
		host.Services.GetRequiredService<App>().Run();
	}
}
