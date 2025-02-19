namespace quick_share.api.Logic.Utils;

public class Generator
{
    public static string NewId()
    {
        Random random = new Random();
        //const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        string RandomGroup() => new string(Enumerable.Range(0, 4).Select(_ => chars[random.Next(chars.Length)]).ToArray());

        return $"{RandomGroup()}-{RandomGroup()}";
    }
}