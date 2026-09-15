using System.Text.RegularExpressions;

namespace WasteToValue.Api.Modules.Collections.Validators;

public static partial class HandoverCodeValidator
{
    private static readonly Regex CodePattern = new(@"^\d{6}$", RegexOptions.Compiled);

    public static IReadOnlyList<string> Validate(string? code)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(code))
        {
            errors.Add("One-time code is required.");
            return errors;
        }

        if (!CodePattern.IsMatch(code))
            errors.Add("One-time code must be exactly 6 digits.");

        return errors;
    }
}
