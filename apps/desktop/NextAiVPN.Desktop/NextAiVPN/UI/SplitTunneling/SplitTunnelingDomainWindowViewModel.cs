using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Xaml.Behaviors.Core;
using NextAiVPN.Common;
using NextAiVPN.Entities;
using NextAiVPN.Services;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN.UI.SplitTunneling;

public class SplitTunnelingDomainWindowViewModel : ViewModelBase
{
	private readonly ISplitTunnelingService _splitTunnelingService;

	private const string Placeholder = "Enter hostname";

	private int _windowHeight = 240;

	private bool _isSubdomainsIncluded = true;

	private int _scrollHeight = 147;

	private int _listBoxWidth;

	private int _textBoxItemWidth = 275;

	private string _addDomainText = "Enter hostname";

	private ScrollBarVisibility _verticalScrollBarVisibility = ScrollBarVisibility.Hidden;

	private Visibility _parentWindowVisibility = Visibility.Hidden;

	private Visibility _addItemVisibility = Visibility.Collapsed;

	private Visibility _domainErrorTextVisibility = Visibility.Collapsed;

	private Visibility _domainListVisibility;

	private Thickness _removeImageItemMargin;

	private Thickness _domainTextBoxMargin = new Thickness(0.0, 4.0, 0.0, 4.0);

	private SolidColorBrush _domainTextBoxBorderBrush;

	private SolidColorBrush _domainTextBoxCaretBrush;

	private readonly IAppLogger _logger;

	private readonly IBrowserLinksOpener _browserLinksOpener;

	public ObservableCollection<SplitTunnelingDomain> DomainList { get; set; } = new ObservableCollection<SplitTunnelingDomain>();

	public SplitTunnelingMainControlViewModel SplitTunnelingMainControlViewModel { get; set; }

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

	public bool IsSubdomainsIncluded
	{
		get
		{
			return _isSubdomainsIncluded;
		}
		set
		{
			if (_isSubdomainsIncluded != value)
			{
				_isSubdomainsIncluded = value;
				OnPropertyChanged("IsSubdomainsIncluded");
			}
		}
	}

	public int ScrollHeight
	{
		get
		{
			return _scrollHeight;
		}
		set
		{
			if (_scrollHeight != value)
			{
				_scrollHeight = value;
				OnPropertyChanged("ScrollHeight");
			}
		}
	}

	public int ListBoxWidth
	{
		get
		{
			return _listBoxWidth;
		}
		set
		{
			if (_listBoxWidth != value)
			{
				_listBoxWidth = value;
				OnPropertyChanged("ListBoxWidth");
			}
		}
	}

	public int TextBoxItemWidth
	{
		get
		{
			return _textBoxItemWidth;
		}
		set
		{
			if (_textBoxItemWidth != value)
			{
				_textBoxItemWidth = value;
				OnPropertyChanged("TextBoxItemWidth");
			}
		}
	}

	public string AddDomainText
	{
		get
		{
			return _addDomainText;
		}
		set
		{
			if (_addDomainText != value)
			{
				if (value.Equals(string.Empty))
				{
					AddDomainText = "Enter hostname";
				}
				_addDomainText = value;
				if (!string.IsNullOrEmpty(_addDomainText) && !_addDomainText.Equals("Enter hostname"))
				{
					AddItemVisibility = Visibility.Visible;
					DomainTextBoxMargin = new Thickness(0.0, 4.0, 36.0, 4.0);
				}
				else
				{
					AddItemVisibility = Visibility.Collapsed;
					DomainTextBoxMargin = new Thickness(0.0, 4.0, 0.0, 4.0);
				}
				OnPropertyChanged("AddDomainText");
			}
		}
	}

	public ScrollBarVisibility VerticalScrollBarVisibility
	{
		get
		{
			return _verticalScrollBarVisibility;
		}
		set
		{
			if (_verticalScrollBarVisibility != value)
			{
				_verticalScrollBarVisibility = value;
				OnPropertyChanged("VerticalScrollBarVisibility");
			}
		}
	}

	public string Title { get; private set; } = "Hostname";

	public string Description { get; private set; } = "Add the hostname you want to bypass VPN";

	public string ErrorMessage { get; private set; } = "Enter a valid hostname";

	public Visibility ParentWindowVisibility
	{
		get
		{
			return _parentWindowVisibility;
		}
		set
		{
			if (_parentWindowVisibility != value)
			{
				_parentWindowVisibility = value;
				if (_parentWindowVisibility == Visibility.Visible)
				{
					GetItemList();
					SetVisibilities();
				}
				OnPropertyChanged("ParentWindowVisibility");
			}
		}
	}

	public Visibility AddItemVisibility
	{
		get
		{
			return _addItemVisibility;
		}
		set
		{
			if (_addItemVisibility != value)
			{
				_addItemVisibility = value;
				OnPropertyChanged("AddItemVisibility");
			}
		}
	}

	public Visibility DomainErrorTextVisibility
	{
		get
		{
			return _domainErrorTextVisibility;
		}
		set
		{
			if (_domainErrorTextVisibility != value)
			{
				_domainErrorTextVisibility = value;
				OnPropertyChanged("DomainErrorTextVisibility");
			}
		}
	}

	public Visibility DomainListVisibility
	{
		get
		{
			return _domainListVisibility;
		}
		set
		{
			if (_domainListVisibility != value)
			{
				_domainListVisibility = value;
				OnPropertyChanged("DomainListVisibility");
			}
		}
	}

	public Thickness RemoveImageItemMargin
	{
		get
		{
			return _removeImageItemMargin;
		}
		set
		{
			if (_removeImageItemMargin != value)
			{
				_removeImageItemMargin = value;
				OnPropertyChanged("RemoveImageItemMargin");
			}
		}
	}

	public Thickness DomainTextBoxMargin
	{
		get
		{
			return _domainTextBoxMargin;
		}
		set
		{
			if (_domainTextBoxMargin != value)
			{
				_domainTextBoxMargin = value;
				OnPropertyChanged("DomainTextBoxMargin");
			}
		}
	}

	public SolidColorBrush DomainTextBoxBorderBrush
	{
		get
		{
			return _domainTextBoxBorderBrush;
		}
		set
		{
			_domainTextBoxBorderBrush = value;
			OnPropertyChanged("DomainTextBoxBorderBrush");
		}
	}

	public SolidColorBrush DomainTextBoxCaretBrush
	{
		get
		{
			return _domainTextBoxCaretBrush;
		}
		set
		{
			_domainTextBoxCaretBrush = value;
			OnPropertyChanged("DomainTextBoxCaretBrush");
		}
	}

	public ICommand DoneButtonClickCommand { get; set; }

	public ICommand AddItemClickCommand { get; set; }

	public ICommand RemoveItemClickCommand { get; set; }

	public ICommand DomainTextBoxLostFocusCommand { get; set; }

	public ICommand DomainTextBoxGotFocusCommand { get; set; }

	public ICommand EnterKeyPressedCommand { get; set; }

	public ICommand DomainsItemDoubleClickPressedCommand { get; set; }

	public SplitTunnelingDomainWindowViewModel(ISplitTunnelingService splitTunnelingService, IAppLogger logger, IBrowserLinksOpener browserLinksOpener)
	{
		_splitTunnelingService = splitTunnelingService;
		_logger = logger;
		_browserLinksOpener = browserLinksOpener;
		DoneButtonClickCommand = new ActionCommand(DoneButtonClickCommandExecute);
		AddItemClickCommand = new ActionCommand(AddItemClickCommandExecute);
		RemoveItemClickCommand = new ActionCommand(RemoveItemClickCommandExecute);
		DomainTextBoxLostFocusCommand = new ActionCommand(DomainTextBoxLostFocusCommandExecute);
		DomainTextBoxGotFocusCommand = new ActionCommand(DomainTextBoxGotFocusCommandExecute);
		EnterKeyPressedCommand = new ActionCommand(EnterKeyPressedCommandExecute);
		DomainsItemDoubleClickPressedCommand = new ActionCommand(DomainsItemDoubleClickPressedCommandExecute);
		SetDomainTextBoxBorderBrush();
		GetItemList();
		SetVisibilities();
	}

	private void DomainsItemDoubleClickPressedCommandExecute(object obj)
	{
		if (obj is SplitTunnelingDomain splitTunnelingDomain)
		{
			_browserLinksOpener.OpenBrowserLink(splitTunnelingDomain.Name.StartsWith("www.") ? splitTunnelingDomain.Name : ("www." + splitTunnelingDomain.Name));
		}
	}

	private void EnterKeyPressedCommandExecute()
	{
		AddItem();
	}

	private void DomainTextBoxGotFocusCommandExecute()
	{
		if (AddDomainText.Equals("Enter hostname"))
		{
			AddDomainText = string.Empty;
		}
	}

	private void DomainTextBoxLostFocusCommandExecute()
	{
		if (AddDomainText.Equals(string.Empty) || string.IsNullOrWhiteSpace(AddDomainText))
		{
			AddDomainText = "Enter hostname";
		}
	}

	private static SolidColorBrush GetBrushFromString(string colorCode)
	{
		return NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush(colorCode);
	}

	private void RemoveItemClickCommandExecute(object obj)
	{
		if (obj is SplitTunnelingDomain splitTunnelingDomain)
		{
			if (DomainList.Contains(splitTunnelingDomain))
			{
				DomainList.Remove(splitTunnelingDomain);
			}
			_splitTunnelingService.Remove(GetOnlyDomainName(splitTunnelingDomain.Name));
			SetItemCountToMainWindow();
			SetVisibilities();
		}
	}

	private void GetItemList()
	{
		DomainList.Clear();
		foreach (string item in _splitTunnelingService.GetList().ToList())
		{
			try
			{
				string[] source = item.Split('*');
				string text = source.LastOrDefault();
				DomainList.Add(new SplitTunnelingDomain
				{
					ToolTip = text,
					Name = GetFullDomainName(text),
					IsSubDomainsIncluded = source.FirstOrDefault().Equals("1")
				});
			}
			catch (Exception ex)
			{
				_logger?.Error($"method: {"GetItemList"}. item: {item} - message : {ex.Message}", "GetItemList", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\UI\\SplitTunneling\\SplitTunnelingDomainWindowViewModel.cs", 465);
			}
		}
	}

	private static string GetFullDomainName(string validatedDomain)
	{
		if (!validatedDomain.StartsWith("www."))
		{
			return ("www." + validatedDomain).ToLower();
		}
		return validatedDomain;
	}

	private void SetVisibilities()
	{
		if (DomainList.Count > 0)
		{
			DomainListVisibility = Visibility.Visible;
			WindowScaling();
			ShowHideVerticalScrollBar();
		}
		else
		{
			DomainListVisibility = Visibility.Collapsed;
			WindowHeight = 240;
		}
	}

	private void ShowHideVerticalScrollBar()
	{
		VerticalScrollBarVisibility = ((DomainList.Count > 6) ? ScrollBarVisibility.Visible : ScrollBarVisibility.Hidden);
	}

	private void WindowScaling()
	{
		RemoveImageItemMargin = new Thickness(0.0);
		ListBoxWidth = 273;
		switch (DomainList.Count)
		{
		case 0:
			WindowHeight = 240;
			TextBoxItemWidth = 258;
			break;
		case 1:
			WindowHeight = 271;
			TextBoxItemWidth = 258;
			break;
		case 2:
			WindowHeight = 294;
			TextBoxItemWidth = 258;
			break;
		case 3:
			WindowHeight = 320;
			TextBoxItemWidth = 258;
			break;
		case 4:
			WindowHeight = 343;
			TextBoxItemWidth = 258;
			break;
		case 5:
			WindowHeight = 368;
			TextBoxItemWidth = 258;
			break;
		case 6:
			WindowHeight = 391;
			TextBoxItemWidth = 258;
			break;
		default:
			RemoveImageItemMargin = new Thickness(-15.0, 0.0, 0.0, 0.0);
			TextBoxItemWidth = 245;
			WindowHeight = 391;
			ListBoxWidth = 257;
			break;
		}
	}

	private void AddItemClickCommandExecute()
	{
		if (!string.IsNullOrEmpty(AddDomainText) && !AddDomainText.Equals("Enter hostname"))
		{
			AddItem();
		}
	}

	private void AddItem()
	{
		if (DomainUtility.IsValidDomain(AddDomainText, out var validatedDomain))
		{
			if (DomainList.Any((SplitTunnelingDomain domain) => domain.Name.Equals(GetFullDomainName(validatedDomain))))
			{
				AddDomainText = string.Empty;
				return;
			}
			SetDomainTextBoxBorderBrush();
			string text = (IsSubdomainsIncluded ? "1" : "0");
			validatedDomain = GetOnlyDomainName(validatedDomain);
			_splitTunnelingService.Add(text + "*" + validatedDomain);
			string fullDomainName = GetFullDomainName(validatedDomain);
			DomainList.Add(new SplitTunnelingDomain
			{
				Name = fullDomainName,
				ToolTip = fullDomainName
			});
			AddDomainText = string.Empty;
			ShowError(isVisible: false);
			SetItemCountToMainWindow();
		}
		else
		{
			ShowError(isVisible: true);
		}
		SetVisibilities();
	}

	private static string GetOnlyDomainName(string validatedDomain)
	{
		if (validatedDomain.StartsWith("www."))
		{
			validatedDomain = validatedDomain.Substring(4);
		}
		return validatedDomain;
	}

	private void ShowError(bool isVisible)
	{
		if (isVisible)
		{
			DomainTextBoxBorderBrush = GetBrushFromString("#F55B5B");
			DomainErrorTextVisibility = Visibility.Visible;
		}
		else
		{
			SetDomainTextBoxBorderBrush();
			DomainErrorTextVisibility = Visibility.Collapsed;
		}
	}

	private void SetDomainTextBoxBorderBrush()
	{
		DomainTextBoxBorderBrush = ((StyleModeDefiner.DefineAppStyle() == NextAiVPN.Services.Persistence.Style.Dark) ? GetBrushFromString("#59595F") : GetBrushFromString("#F4F4F5"));
	}

	private static bool IsValidDomainName(string name)
	{
		return Uri.CheckHostName(name) != UriHostNameType.Unknown;
	}

	private bool IsHostnameOrIpValid()
	{
		try
		{
			Dns.GetHostEntry(AddDomainText);
			return true;
		}
		catch (Exception ex)
		{
			_logger?.Information($"Method: {"IsHostnameOrIpValid"} No such domain {AddDomainText}\n{ex.Message}", "IsHostnameOrIpValid", "C:\\Users\\viktorpavlenko\\source\\repos\\windows-app\\NextAiVPN\\UI\\SplitTunneling\\SplitTunnelingDomainWindowViewModel.cs", 625);
		}
		return ValidateIPv4(AddDomainText);
	}

	public bool ValidateIPv4(string ipString)
	{
		if (string.IsNullOrWhiteSpace(ipString))
		{
			return false;
		}
		string[] array = ipString.Split('.');
		if (array.Length != 4)
		{
			return false;
		}
		return array.All((string r) => byte.TryParse(r, out var _));
	}

	private void SetItemCountToMainWindow()
	{
		SplitTunnelingMainControlViewModel.DomainCounterControlViewModel.CountText = DomainList.Count.ToString();
	}

	private void DoneButtonClickCommandExecute()
	{
		Close();
	}

	public void OnWindowClosing(object sender, CancelEventArgs e)
	{
		Close();
		e.Cancel = true;
	}

	private void Close()
	{
		AddDomainText = string.Empty;
		ShowError(isVisible: false);
		SetItemCountToMainWindow();
		ParentWindowVisibility = Visibility.Hidden;
	}

	public void OnScrollPreviewMouseWheel(object sender, MouseWheelEventArgs e)
	{
		ScrollViewer obj = (ScrollViewer)sender;
		obj.ScrollToVerticalOffset(obj.VerticalOffset - (double)e.Delta);
		e.Handled = true;
	}
}
