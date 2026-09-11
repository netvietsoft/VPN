using System.Text.RegularExpressions;

namespace NextAiVPN.Services.Persistence;

internal class EmailValidator : IEmailValidator
{
	public bool IsValidEmail(string email)
	{
		if (string.IsNullOrEmpty(email))
		{
			return false;
		}
		string pattern = "^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$";
		return Regex.IsMatch(email.ToLower(), pattern);
	}
}
