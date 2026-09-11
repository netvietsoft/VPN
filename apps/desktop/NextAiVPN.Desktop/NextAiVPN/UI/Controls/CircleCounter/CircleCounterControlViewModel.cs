using System.Windows;

namespace NextAiVPN.UI.Controls.CircleCounter;

public class CircleCounterControlViewModel : ViewModelBase
{
	private int _borderWidth = 18;

	private string _countText = "0";

	private Visibility _controlVisibility;

	public int BorderWidth
	{
		get
		{
			return _borderWidth;
		}
		set
		{
			if (_borderWidth != value)
			{
				_borderWidth = value;
				OnPropertyChanged("BorderWidth");
			}
		}
	}

	public string CountText
	{
		get
		{
			return _countText;
		}
		set
		{
			if (_countText != value)
			{
				_countText = value;
				ControlVisibility = ((string.IsNullOrEmpty(_countText) || _countText.Equals("0")) ? Visibility.Hidden : Visibility.Visible);
				SetWidth();
				OnPropertyChanged("CountText");
			}
		}
	}

	public Visibility ControlVisibility
	{
		get
		{
			return _controlVisibility;
		}
		set
		{
			if (_controlVisibility != value)
			{
				_controlVisibility = value;
				OnPropertyChanged("ControlVisibility");
			}
		}
	}

	public CircleCounterControlViewModel()
	{
		CountText = string.Empty;
	}

	private void SetWidth()
	{
		if (CountText.Length > 2)
		{
			int num = (CountText.Length - 2) * 5;
			BorderWidth = 18 + num;
		}
		else
		{
			BorderWidth = 18;
		}
	}
}
