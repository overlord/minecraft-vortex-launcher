using System.Windows;
using VortexLauncherNet.Services;

namespace VortexLauncherNet;

internal class App : Application
{
	private readonly MainWindow _mainWindow;

	public App(
		MainWindow mainWindow,
		MainWindowVM mainWindowVM,
		MinecraftService minecraftLocalStorageService,
		JavaService javaService
	)
	{
		mainWindowVM.WorkingDir = AppDomain.CurrentDomain.BaseDirectory;
		mainWindowVM.MinecraftRootDir = @"F:\#Games\Minecraft";
		mainWindowVM.PlayerName = "Ghh";
		mainWindowVM.RamAmount = 4096;

		mainWindowVM.MinecraftVersions = minecraftLocalStorageService.FindInstalledVersions(mainWindowVM.MinecraftRootDir.Value!);
		mainWindowVM.JavaVersions = javaService.FindJava();

		mainWindowVM.MinecraftVersionSelected = "1.17.1";
		if (!mainWindowVM.MinecraftVersions.Contains(mainWindowVM.MinecraftVersionSelected.Value, StringComparer.OrdinalIgnoreCase))
		{
			mainWindowVM.MinecraftVersionSelected = mainWindowVM.MinecraftVersions.FirstOrDefault();
		}

		_mainWindow = mainWindow;
		_mainWindow.DataContext = mainWindowVM;
	}

	protected override void OnStartup(StartupEventArgs e)
	{
		_mainWindow.Show();
		base.OnStartup(e);
	}
}
