using System.Windows;
using VortexLauncherNet.Helpers;
using VortexLauncherNet.Services;
using VortexLauncherNet.Views;
using VortexLauncherNet.ViewModels;

namespace VortexLauncherNet;

internal class App : Application
{
	private readonly MainWindow _mainWindow;

	public App(
		MainWindow mainWindow,
		MainWindowVM mainWindowVM,
		MinecraftLocalService minecraftLocalService,
		MinecraftRemoteService minecraftRemoteService,
		JavaService javaService
	)
	{
		var options = mainWindowVM.Options;
		options.WorkingDir = AppDomain.CurrentDomain.BaseDirectory;
		options.MinecraftRootDir = @"F:\#Games\Minecraft";
		options.PlayerName = "Ghh";
		options.RamAmount = 4096;

		options.MinecraftVersions = minecraftLocalService.FindInstalledVersions(options.MinecraftRootDir.Value!);
		options.JavaVersions = javaService.FindJava();

		options.MinecraftVersionSelected = "1.17.1";
		if (!options.MinecraftVersions.Contains(options.MinecraftVersionSelected.Value, StringComparer.OrdinalIgnoreCase))
		{
			options.MinecraftVersionSelected = options.MinecraftVersions.FirstOrDefault();
		}

		var minecraftRemoteVersions = minecraftRemoteService.GetVersions().GetAwaiter().GetResult();
		options.MinecraftRemoteVersions = minecraftRemoteVersions.Versions.ToObservableCollection();
		options.MinecraftRemoteVersionSelected = minecraftRemoteVersions.Versions.FirstOrDefault();

		_mainWindow = mainWindow;
		_mainWindow.DataContext = mainWindowVM;
	}

	protected override void OnStartup(StartupEventArgs e)
	{
		_mainWindow.Show();
		base.OnStartup(e);
	}
}
