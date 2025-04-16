using System.Text.Json;

namespace quick_share.api.Business.Models;

public class Session
{
    public required string Id { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public List<object>? Items { get; set; } = null;

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}