using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace NextAiVPN.UI.Feedback.FeedbackValidationError;

public partial class FeedbackValidationErrorMessage : UserControl, IComponentConnector
{
	public static readonly DependencyProperty MessageProperty = DependencyProperty.Register("Message", typeof(string), typeof(FeedbackValidationErrorMessage), new PropertyMetadata(string.Empty));

	public string Message
	{
		get
		{
			return (string)GetValue(MessageProperty);
		}
		set
		{
			SetValue(MessageProperty, value);
		}
	}

	public FeedbackValidationErrorMessage()
	{
		InitializeComponent();
		Message = "Unable to send your feedback. Please check your Internet connection and try again.";
	}
}
