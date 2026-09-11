using System.Windows.Controls;
using System.Windows.Markup;

namespace NextAiVPN.UI.TermsAndPolicies;

public partial class TermsAndPoliciesTabButton : UserControl, IComponentConnector
{
	public string Capture { get; set; } = "Terms of Service";

	public TermsAndPoliciesTabButton()
	{
		InitializeComponent();
	}
}
