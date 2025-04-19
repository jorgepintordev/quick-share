namespace quick_share.api.Business.Consts;

public class MaintenanceServiceMessages
{
    internal class Trace
    {
        public const string ProcessingStart = "Processing {DirectoryPath} for cleanup";
        public const string DeletedPath = "Deleted {DeletePath}";
        public const string FileCleanupOk = "File cleanup Ok";
    }
    internal class Error
    {
        public const string FileCleanup = "Unable to delete directory on the server";
    }
}