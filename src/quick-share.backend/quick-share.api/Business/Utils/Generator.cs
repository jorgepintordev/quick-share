namespace quick_share.api.Business.Utils;

public class Generator
{
    public static string NewId(bool withLowercase = false)
    {
        Random random = new Random();
        string chars = withLowercase ? "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789" : "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        string RandomGroup() => new string(Enumerable.Range(0, 4).Select(_ => chars[random.Next(chars.Length)]).ToArray());

        return $"{RandomGroup()}-{RandomGroup()}";
    }
}