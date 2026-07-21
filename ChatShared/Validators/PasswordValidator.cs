using System.Text.RegularExpressions;

namespace ChatShared.Validators;
public static class PasswordValidator
{
    private static readonly Regex PasswordRegex =
        new Regex(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,}$"
        );

    public static bool IsValid(string password)
    {
        return !string.IsNullOrEmpty(password)
            && PasswordRegex.IsMatch(password);
    }
}