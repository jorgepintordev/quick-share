using System.Text.Json;
using quick_share.api.Business.Models;

namespace quick_share.api.Business.Extensions;

public static class SharedItemExtensions
{
    public static SharedItem? ToSharedItem(this object json)
    {
        ArgumentNullException.ThrowIfNull(json);
        return ToSharedItem((JsonElement)json);
    }

    public static SharedItemBinary? ToSharedItemBinary(this object json)
    {
        ArgumentNullException.ThrowIfNull(json);
        return ToSharedItemBinary((JsonElement)json);
    }


    public static SharedItem? ToSharedItem(this JsonElement json)
    {
        ArgumentNullException.ThrowIfNull(json);
        return JsonSerializer.Deserialize<SharedItem>(json.ToString());
    }

    public static SharedItemBinary? ToSharedItemBinary(this JsonElement json)
    {
        ArgumentNullException.ThrowIfNull(json);
        return JsonSerializer.Deserialize<SharedItemBinary>(json.ToString());
    }
}