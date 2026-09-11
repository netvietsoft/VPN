using System;
using System.Windows.Input;

namespace NextAiVPN.UI;

internal class RelayCommand : ICommand
{
	private readonly Func<object, bool> _canExecute;

	private readonly Action<object> _execute;

	public event EventHandler CanExecuteChanged
	{
		add
		{
			CommandManager.RequerySuggested += value;
		}
		remove
		{
			CommandManager.RequerySuggested -= value;
		}
	}

	public RelayCommand(Action<object> execute, Func<object, bool> canExecute)
	{
		_canExecute = canExecute;
		_execute = execute ?? throw new Exception("Error!!!!!!");
	}

	public RelayCommand(Action<object> execute)
		: this(execute, null)
	{
	}

	public bool CanExecute(object parameter)
	{
		if (_canExecute != null)
		{
			return _canExecute(parameter);
		}
		return true;
	}

	public void Execute(object parameter)
	{
		_execute(parameter);
	}
}
