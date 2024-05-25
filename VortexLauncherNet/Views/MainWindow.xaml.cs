using System.Windows;
using VortexLauncherNet.ViewModels;

namespace VortexLauncherNet.Views;

internal partial class MainWindow : Window
{
	private readonly MainWindowVM _vm;

	public MainWindow(MainWindowVM vm)
	{
		_vm = vm;

		InitializeComponent();
	}
}
