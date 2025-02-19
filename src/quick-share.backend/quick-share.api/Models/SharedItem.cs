namespace quick_share.api.Models;

public class SharedItem
{
    public required Guid Id { get; set; } = Guid.NewGuid();
    public required string Value { get; set; }
    public required bool IsBinary { get; set; }
}