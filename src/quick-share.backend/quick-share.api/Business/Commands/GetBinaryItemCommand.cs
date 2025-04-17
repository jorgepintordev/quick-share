using quick_share.api.Business.Models;

namespace quick_share.api.Business.Commands;

public record GetBinaryItemCommand(Session Session, Guid ItemId);