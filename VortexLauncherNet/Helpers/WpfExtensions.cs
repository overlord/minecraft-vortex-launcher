using System.Collections.ObjectModel;

namespace VortexLauncherNet.Helpers;

internal static class WpfExtensions
{
	public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> collection)
	{
		return new ObservableCollection<T>(collection);
	}
}
