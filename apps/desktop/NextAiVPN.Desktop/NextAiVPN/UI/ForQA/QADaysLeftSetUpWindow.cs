using System;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;

namespace NextAiVPN.UI.ForQA;

public partial class QADaysLeftSetUpWindow : Window, IComponentConnector
{
	public int Days { get; set; }

	public int DaysLeftAfterUserCloseFeedbackWindow { get; set; }

	public DateTime DaysFeedback { get; set; }

	public bool ConsiderFeedback { get; set; }

	public QADaysLeftSetUpWindow()
	{
		InitializeComponent();
		DatePicker.SelectedDate = DateTime.Now.AddDays(-45.0);
	}

	private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
	{
		Days = ((!string.IsNullOrEmpty(TextBoxDays.Text)) ? int.Parse(TextBoxDays.Text, CultureInfo.InvariantCulture) : 0);
		DaysLeftAfterUserCloseFeedbackWindow = ((!string.IsNullOrEmpty(TextBoxDays.Text)) ? int.Parse(TextBoxCloseFeedbackWindowDays.Text, CultureInfo.InvariantCulture) : 0);
		DaysFeedback = DatePicker.SelectedDate.Value;
		ConsiderFeedback = CheckBox.IsChecked.Value;
		Close();
	}
}
