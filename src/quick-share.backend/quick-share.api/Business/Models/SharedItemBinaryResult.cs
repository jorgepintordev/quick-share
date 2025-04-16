namespace quick_share.api.Business.Models;

public class SharedItemBinaryResult
{
    public string Filename { get; set; } = string.Empty;
    public Stream? Data { get; set; } = null;
}