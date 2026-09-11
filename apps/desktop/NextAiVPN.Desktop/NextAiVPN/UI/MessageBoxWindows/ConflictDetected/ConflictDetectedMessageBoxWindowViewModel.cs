using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors.Core;
using NextAiVPN.Common;

namespace NextAiVPN.UI.MessageBoxWindows.ConflictDetected;

public class ConflictDetectedMessageBoxWindowViewModel : ViewModelBase
{
	private ConflictDetectedMessageBoxWindow _parent;

	private string _description;

	private Visibility _windowVisibility;

	private int _windowHeight = 198;

	private readonly IAppLogger _logger;

	public ICommand CancelButtonClickCommand { get; set; }

	public ICommand OkButtonClickCommand { get; set; }

	public string Title { get; set; } = "Title";

	public string Description
	{
		get
		{
			return _description;
		}
		set
		{
			if (_description != value)
			{
				_description = value;
				SetWindowHeight();
				OnPropertyChanged("Description");
			}
		}
	}

	public string OkButtonText { get; set; } = "TopButtonTextN";

	public string CancelButtonText { get; set; } = "BottomButtonText";

	public bool DialogResult { get; set; }

	public Visibility WindowVisibility
	{
		get
		{
			return _windowVisibility;
		}
		set
		{
			if (_windowVisibility != value)
			{
				_windowVisibility = value;
				OnPropertyChanged("WindowVisibility");
			}
		}
	}

	public int WindowHeight
	{
		get
		{
			return _windowHeight;
		}
		set
		{
			if (_windowHeight != value)
			{
				_windowHeight = value;
				OnPropertyChanged("WindowHeight");
			}
		}
	}

	public ConflictDetectedMessageBoxWindowViewModel(string title, string description, string okButtonText, string cancelButtonText, IAppLogger logger)
	{
		Title = title;
		Description = description;
		OkButtonText = okButtonText;
		CancelButtonText = cancelButtonText;
		_logger = logger;
		BindCommands();
	}

	private void SetWindowHeight()
	{
		try
		{
			if (Description.Length / 50 > 1)
			{
				int num = (Description.Length / 50 + 1) * 13;
				WindowHeight += num;
			}
		}
		catch (Exception ex)
		{
			_logger?.Error(ex.Message, "SetWindowHeight", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\UI\\MessageBoxWindows\\ConflictDetected\\ConflictDetectedMessageBoxWindowViewModel.cs", 135);
		}
	}

	public ConflictDetectedMessageBoxWindowViewModel(IAppLogger logger)
	{
		_logger = logger;
		BindCommands();
	}

	public void SetParent(ConflictDetectedMessageBoxWindow parent)
	{
		_parent = parent;
	}

	private void BindCommands()
	{
		CancelButtonClickCommand = new ActionCommand(CancelButtonClickCommandExecute);
		OkButtonClickCommand = new ActionCommand(OkButtonClickCommandExecute);
	}

	private void OkButtonClickCommandExecute()
	{
		Close(dialogResult: true);
	}

	private void CancelButtonClickCommandExecute()
	{
		Close(dialogResult: false);
	}

	private void Close(bool dialogResult)
	{
		DialogResult = dialogResult;
		_parent?.Close();
	}

	public void OnWindowClosing(object sender, CancelEventArgs e)
	{
		WindowVisibility = Visibility.Hidden;
		e.Cancel = true;
	}
}
