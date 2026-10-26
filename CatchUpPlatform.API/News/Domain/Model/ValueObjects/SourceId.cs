namespace CatchUpPlatform.API.News.Domain.Model.ValueObjects;

public sealed record SourceId
{
    private const int MaxLength = 255;
    
    public string Value { get; }
    
    public SourceId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("SourceId cannot be null or empty.", nameof(value));

        if (value.Length > MaxLength)
            throw new ArgumentException($"SourceId cannot exceed {MaxLength} characters.", nameof(value));

        Value = value;
    }
    
    
}