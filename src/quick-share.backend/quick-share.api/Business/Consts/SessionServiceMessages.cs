namespace quick_share.api.Business.Consts;

public class SessionServiceMessages
{
    internal class Trace
    {
        public const string StartSessionOk = "Start session Ok {SessionId}";
        public const string GetSessionOk = "Get session Ok {Session}";
        public const string EndSessionOk = "End session Ok {SessionId}";
        public const string AddSimpleItemOk = "Add simple item into session Ok {SessionId} {ItemId}";
        public const string CreatedUploadPath = "Create server upload path completed {UploadPath}";
        public const string BinaryItemUploaded = "Binary uploaded to server completed {FilePath}";
        public const string AddBinaryItemOk = "Add binary item into session Ok {SessionId} {ItemId}";
        public const string DeleteItemOk = "Delete item from session Ok {SessionId} {ItemId}";
        public const string BinaryItemDeleted = "Binary deleted from server completed {FilePath}";
        public const string GetBinaryItemOk = "Get binary item from session Ok {SessionId} {ItemId}";
    }
    internal class Error
    {
        public const string ValidationFail = "Validation fail";
        public const string DeserializeEmpty = "Deserialize returned empty";
        public const string SessionStartFail = "Unable to start session";
        public const string SessionNotFound = "Session not found";
        public const string SessionItemNotFound = "Session item not found";
        public const string SessionBinaryItemNotFound = "Session binary item not found";
        public const string SessionItemDeleteFail = "Unable to remove item from session";
        public const string SimpleItemAddFail = "Unable to add simple item to session";
        public const string BinaryItemAddFail = "Unable to add binary item to session";
        public const string BinaryItemServerCopyFail = "Unable to copy binary on the server";
        public const string BinaryItemServerDeleteFail = "Unable to delete binary on the server";
        public const string BinaryItemGetFail = "Unable to get binary from server";
    }
}