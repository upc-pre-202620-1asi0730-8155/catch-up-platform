namespace CatchUpPlatform.API.News.Domain.Model.ValueObjects;

public sealed record NewsApiKey
{
    public string Value { get; }
    
    private const int MaxLength = 255;

    public NewsApiKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) 
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(value));
        if (value.Length > MaxLength)
            throw new ArgumentException($"Value cannot exceed {MaxLength} characters.", nameof(value));
        Value = value;
    }
    
    public override string ToString() => Value;

    public static string MaskedForLogging(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;

        var visibleChars = Math.Min(4, value.Length);
        var maskedPart = new string('*', value.Length - visibleChars);
        return maskedPart + value[^visibleChars..];
    }
    
    public string MaskedForLogging() => MaskedForLogging(Value);
}