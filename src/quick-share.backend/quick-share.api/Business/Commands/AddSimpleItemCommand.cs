using quick_share.api.Business.Models;

namespace quick_share.api.Business.Commands;

public record AddSimpleItemCommand(Session Session, string ItemValue);