using System.Text.Json;

namespace quick_share.api.Business.Models;

public class SharedItem
{
    public required Guid Id { get; set; } = Guid.NewGuid();
    public required string Value { get; set; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}