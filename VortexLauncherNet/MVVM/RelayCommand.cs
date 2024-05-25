using System.Diagnostics;
using System.Windows.Input;

namespace VortexLauncherNet.MVVM;

internal class RelayCommand<T> : ICommand
{
	#region Fields

	public Action<T> ExecuteHandler { get; set; }
	public Predicate<T> CanExecuteHandler { get; set; }
	public Action<Exception> ExceptionHandler { get; set; }

	/// <summary> Флаг, указывающий что в процессе вычисления CanExecute произошел выброс exception-а и ошибка была отправлена в ExceptionHandler </summary>
	private bool _canExecuteFailed = false;

	/// <summary> Command text to display in menu. </summary>
	public string CommandText { get; set; }

	#endregion // Fields

	#region Constructors

	public RelayCommand() { }

	public RelayCommand(Action<T> execute)
		: this(execute, null, null) { }

	public RelayCommand(
		Action<T> execute,
		Predicate<T> canExecute)
		: this(execute, canExecute, null) { }

	public RelayCommand(
		Action<T> execute,
		Predicate<T> canExecute,
		Action<Exception> exceptionHandler)
	{
		if(execute == null)
		{
			throw new ArgumentNullException("execute");
		}

		ExecuteHandler = execute;
		CanExecuteHandler = canExecute;
		ExceptionHandler = exceptionHandler;
	}

	#endregion // Constructors

	#region ICommand Members

	[DebuggerStepThrough]
	public bool CanExecute(object parameter)
	{
		if(_canExecuteFailed)
			return false;

		try
		{
			return CanExecuteHandler == null || CanExecuteHandler((T)parameter);
		}
		catch(Exception exc)
		{
			if (ExceptionHandler != null)
			{
				_canExecuteFailed = true;
				ExceptionHandler(exc);
			}
			else
			{
				throw;
			}

			return false;
		}
	}

	public event EventHandler? CanExecuteChanged { add { CommandManager.RequerySuggested += value; } remove { CommandManager.RequerySuggested -= value; } }

	public void Execute(object parameter)
	{
		try
		{
			if(ExecuteHandler == null)
				throw new InvalidOperationException("Обработчик команды не указан.");

			ExecuteHandler((T)parameter);
		}
		catch(Exception ex)
		{
			if(ExceptionHandler != null)
			{
				ExceptionHandler(ex);
			}
			else
			{
				throw;
			}
		}
	}
	public void Execute() { Execute(null); }

	#endregion // ICommand Members

	public static RelayCommand EmptyCommand { get; private set; }

	static RelayCommand() { EmptyCommand = new RelayCommand(s => { }, s => false); }
}

internal class RelayCommand : RelayCommand<object>
{
	#region Constructors

	public RelayCommand() { }

	public RelayCommand(Action<object> execute)
		: base(execute, null, null) { }

	public RelayCommand(
		Action<object> execute,
		Predicate<object> canExecute)
		: base(execute, canExecute, null) { }

	public RelayCommand(
		Action<object> execute,
		Predicate<object> canExecute,
		Action<Exception> exceptionHandler)
		: base(execute, canExecute, exceptionHandler) { }
		
	#endregion // Constructors
}
