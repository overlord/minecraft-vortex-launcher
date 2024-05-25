using System.ComponentModel;

namespace VortexLauncherNet.MVVM;

internal class Observable<T> : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;

	public Observable()
	{		
	}

	public Observable(T? value)
	{
		_value = value;
	}

	private T? _value;

	public T? Value
	{
		get => _value;
		set
		{
			_value = value;
			NotifyPropertyChanged(nameof(Value));
		}
	}

	internal void NotifyPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public static implicit operator Observable<T>(T? value) => new Observable<T>(value);
}
