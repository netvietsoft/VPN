using System.Windows.Input;
using Microsoft.Xaml.Behaviors.Core;
using NextAiVPN.Services.Persistence;

namespace NextAiVPN.UI.TermsAndPolicies;

public class ExpandedTermsAndPoliciesViewModel : ViewModelBase, IExpandedTermsAndPoliciesViewModel
{
	private readonly IBrowserLinksOpener _browserLinksOpener;

	public string PageText { get; set; } = "";

	public string MainControlHeader { get; set; } = "Legal Information";

	public string TermsOfServiceHeader { get; set; } = "Terms of Service";

	public string TermsOfServiceText { get; set; } = "Learn more about our";

	public string TermsOfServiceLinkText { get; set; } = "terms of service.";

	public string PrivacyPolicyHeader { get; set; } = "Privacy Policy";

	public string PrivacyPolicyText { get; set; } = "Read our ";

	public string PrivacyPolicyLinkText { get; set; } = "privacy policy";

	public string PrivacyPolicyEndText { get; set; } = "updates.";

	public string LicensesHeader { get; set; } = "Licenses";

	public string LicensesText { get; set; } = "Get information about our";

	public string LicensesLinkText { get; set; } = "licenses.";

	public ICommand TermsAndPoliciesMouseDownCommand { get; set; }

	public ICommand PrivacyPolicyMouseDownCommand { get; set; }

	public ICommand LicensesMouseDownCommand { get; set; }

	public ExpandedTermsAndPoliciesViewModel(IBrowserLinksOpener browserLinksOpener)
	{
		_browserLinksOpener = browserLinksOpener;
		InitializeCommands();
	}

	private void InitializeCommands()
	{
		TermsAndPoliciesMouseDownCommand = new RelayCommand(TermsAndPoliciesMouseDownCommandExecute);
		PrivacyPolicyMouseDownCommand = new RelayCommand(PrivacyPolicyMouseDownCommandExecute);
		LicensesMouseDownCommand = new ActionCommand(LicensesMouseDownCommandExecute);
	}

	private void LicensesMouseDownCommandExecute()
	{
		_browserLinksOpener.OpenBrowserLink(5);
	}

	private void TermsAndPoliciesMouseDownCommandExecute(object obj)
	{
		_browserLinksOpener.OpenBrowserLink(1);
	}

	private void PrivacyPolicyMouseDownCommandExecute(object obj)
	{
		_browserLinksOpener.OpenBrowserLink(2);
	}
}
