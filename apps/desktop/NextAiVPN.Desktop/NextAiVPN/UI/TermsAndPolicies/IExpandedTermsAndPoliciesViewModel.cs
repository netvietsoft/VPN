using System.Windows.Input;

namespace NextAiVPN.UI.TermsAndPolicies;

public interface IExpandedTermsAndPoliciesViewModel
{
	string MainControlHeader { get; set; }

	string TermsOfServiceHeader { get; set; }

	string TermsOfServiceText { get; set; }

	string TermsOfServiceLinkText { get; set; }

	string PrivacyPolicyHeader { get; set; }

	string PrivacyPolicyText { get; set; }

	string PrivacyPolicyLinkText { get; set; }

	string PrivacyPolicyEndText { get; set; }

	string LicensesText { get; set; }

	string LicensesHeader { get; set; }

	string LicensesLinkText { get; set; }

	string PageText { get; set; }

	ICommand LicensesMouseDownCommand { get; set; }

	ICommand TermsAndPoliciesMouseDownCommand { get; set; }

	ICommand PrivacyPolicyMouseDownCommand { get; set; }
}
