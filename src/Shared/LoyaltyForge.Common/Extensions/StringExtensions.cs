namespace LoyaltyForge.Common.Extensions;

/// <summary>
/// String extension methods.
/// </summary>
public static class StringExtensions
{
    public static bool IsNullOrEmpty(this string? value) => string.IsNullOrEmpty(value);
    
    public static bool IsNullOrWhiteSpace(this string? value) => string.IsNullOrWhiteSpace(value);
}
