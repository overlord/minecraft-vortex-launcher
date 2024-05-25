using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using VortexLauncherNet.Services;

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
				services.AddSingleton<MinecraftService>();
				services.AddSingleton<JavaService>();
			})
			.Build();
		
		var app = host.Services.GetService<App>();
		app!.Run();
	}
}
