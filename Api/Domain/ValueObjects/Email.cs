using System.Text.RegularExpressions;
using Vogen;

namespace Api.Domain.ValueObjects;

[ValueObject<string>]
public readonly partial struct Email
{
    private static readonly Regex EmailPattern = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static string NormalizeInput(string input) =>
        input.Trim().ToLowerInvariant();

    private static Validation Validate(string value) =>
        string.IsNullOrWhiteSpace(value)
            ? Validation.Invalid("El email es obligatorio.")
            : EmailPattern.IsMatch(value)
                ? Validation.Ok
                : Validation.Invalid("El formato del email no es válido.");
}
