using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN.UI;

public partial class SendFeedbackButtonControl : UserControl, IComponentConnector
{
	private ImageSource _emojImageSource;

	public SendFeedbackButtonControl()
	{
		InitializeComponent();
	}

	public void SetFeedbackVisibility(Visibility visibility)
	{
		ControlFinder.FindVisualChildren<Grid>(this).First((Grid x) => x.Name.Equals("FeedbackGridGrid")).Visibility = visibility;
	}

	public void SetEmojiVisibility(Visibility visibility)
	{
		ControlFinder.FindVisualChildren<Image>(this).First((Image x) => x.Name.Equals("Emoji")).Visibility = visibility;
	}

	public void SetEmojiIcon(int emojiCode)
	{
		SetFeedbackVisibility(Visibility.Collapsed);
		Image image = ControlFinder.FindVisualChildren<Image>(this).First((Image x) => x.Name.Equals("Emoji"));
		image.Visibility = Visibility.Visible;
		string uriString;
		switch (emojiCode)
		{
		case -99:
			uriString = string.Empty;
			break;
		case -1:
			uriString = "/Assets/error.png";
			break;
		case 1:
			uriString = "/Assets/mad.png";
			_emojImageSource = new BitmapImage(new Uri(uriString, UriKind.Relative));
			break;
		case 2:
			uriString = "/Assets/mah.png";
			_emojImageSource = new BitmapImage(new Uri(uriString, UriKind.Relative));
			break;
		case 3:
			uriString = "/Assets/smile.png";
			_emojImageSource = new BitmapImage(new Uri(uriString, UriKind.Relative));
			break;
		case 10:
			uriString = IconHelper.GetIcon("arrowbackNormal");
			break;
		case 11:
			uriString = IconHelper.GetIcon("arrowbackClicked");
			break;
		case 21:
			uriString = "/Assets/thumbsup.png";
			break;
		default:
			throw new ArgumentOutOfRangeException(base.Name);
		}
		image.Source = new BitmapImage(new Uri(uriString, UriKind.Relative));
	}

	public void SetControlColor()
	{
		Border border = ControlFinder.FindVisualChildren<Border>(this).First((Border x) => x.Name.Equals("btnBorder"));
		if (border == null)
		{
			return;
		}
		switch (StyleModeDefiner.DefineAppStyle())
		{
		case NextAiVPN.Services.Persistence.Style.Dark:
			if (IsEmojiVisible())
			{
				border.Background = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#2A2A2C");
			}
			else
			{
				border.Background = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#FFFFFF");
			}
			break;
		case NextAiVPN.Services.Persistence.Style.Light:
			if (IsEmojiVisible())
			{
				border.Background = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#FFFFFF");
			}
			else
			{
				border.Background = NextAiVPN.Services.Persistence.ColorConverter.ConvertColorCodeToBrush("#FFAB33");
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public bool EmojiSourceContains(string image)
	{
		Image emojiImg = GetEmojiImg();
		if (emojiImg.Source == null)
		{
			return false;
		}
		return emojiImg.Source.ToString().Contains(image);
	}

	private void SetPrevEmoji()
	{
		if (_emojImageSource != null)
		{
			SetFeedbackVisibility(Visibility.Collapsed);
			Image emojiImg = GetEmojiImg();
			emojiImg.Visibility = Visibility.Visible;
			emojiImg.Source = _emojImageSource;
		}
	}

	public bool IsEmoji()
	{
		Image emojiImg = GetEmojiImg();
		if (emojiImg.Source == null)
		{
			return false;
		}
		if (!emojiImg.Source.ToString().Contains("smile") && !emojiImg.Source.ToString().Contains("mah"))
		{
			return emojiImg.Source.ToString().Contains("mad");
		}
		return true;
	}

	private bool IsArrowNormal()
	{
		Image emojiImg = GetEmojiImg();
		if (emojiImg.Source == null)
		{
			return false;
		}
		return emojiImg.Source.ToString().Contains("arrowbackNormal");
	}

	private bool IsArrowClicked()
	{
		Image emojiImg = GetEmojiImg();
		if (emojiImg.Source == null)
		{
			return false;
		}
		return emojiImg.Source.ToString().Contains("arrowbackClicked");
	}

	public bool IsEmojiVisible()
	{
		Image emojiImg = GetEmojiImg();
		if (emojiImg.Source == null)
		{
			return false;
		}
		return emojiImg.IsVisible;
	}

	private Image GetEmojiImg()
	{
		return ControlFinder.FindVisualChildren<Image>(this).First((Image x) => x.Name.Equals("Emoji"));
	}

	private void SendFeedbackBtn_OnMouseEnter(object sender, MouseEventArgs e)
	{
		if (IsEmojiVisible())
		{
			SetEmojiIcon(10);
		}
		SetControlColor();
	}

	private void SendFeedbackBtn_OnMouseLeave(object sender, MouseEventArgs e)
	{
		if (IsArrowNormal())
		{
			SetPrevEmoji();
		}
		SetControlColor();
	}
}
