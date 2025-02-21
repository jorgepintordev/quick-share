using System.Text.Json;

namespace quick_share.api.Models;

public class SharedItemBinary : SharedItem
{
    public string? FileExtension { get; set; }
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}