using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using VortexLauncherNet.Models;
using VortexLauncherNet.MVVM;

namespace VortexLauncherNet.ViewModels;

internal class OptionsVM
{
	public Observable<string> WorkingDir { get; set; } = new();
	public Observable<string> MinecraftRootDir { get; set; } = new();

	public Observable<string> PlayerName { get; set; } = new();
	public Observable<int> RamAmount { get; set; } = new();

	public ObservableCollection<JavaVersion> JavaVersions { get; set; } = new();
	public Observable<JavaVersion> JavaVersionSelected { get; set; } = new();

	public ObservableCollection<string> MinecraftVersions { get; set; } = new();
	public Observable<string> MinecraftVersionSelected { get; set; } = new();

	public ObservableCollection<MinecraftRemoteVersion> MinecraftRemoteVersions { get; set; } = new();
	public Observable<MinecraftRemoteVersion> MinecraftRemoteVersionSelected { get; set; } = new();
}
