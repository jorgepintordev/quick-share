namespace quick_share.api.Business.Consts;

public static class SessionServiceErrors
{
    public static string DeserializeEmpty = "Deserialize returned empty";
    public static string SessionStartFail = "Unable to start session";
    public static string SessionIdEmpty = "Session Id is empty";
    public static string SessionNotFound = "Session not found";
    public static string SessionItemNotFound = "Session item not found";
    public static string SessionBinaryItemNotFound = "Session binary item not found";
    public static string SessionItemDeleteFail = "Unable to remove item from session";
    public static string SimpleItemAddFail = "Unable to add simple item to session";
    public static string BinaryItemAddFail = "Unable to add binary item to session";
    public static string BinaryItemServerCopyFail = "Unable to copy binary on the local path";
    public static string BinaryItemServerDeleteFail = "Unable to delete binary on the local path";
    public static string BinaryItemGetFail = "Unable to get binary from server";
}