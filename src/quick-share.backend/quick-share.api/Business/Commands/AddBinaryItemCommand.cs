using quick_share.api.Business.Models;

namespace quick_share.api.Business.Commands;

public record AddBinaryItemCommand(Session Session, IFormFile FormFile);