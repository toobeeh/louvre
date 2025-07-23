using tobeh.Louvre.Server.Database.Model;

namespace tobeh.Louvre.Server.Controllers.Dto;

public record AuthorizeUserDto(int TypoId, UserTypeEnum UserType);