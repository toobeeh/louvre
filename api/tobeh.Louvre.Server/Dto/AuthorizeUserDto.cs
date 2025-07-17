using tobeh.Louvre.Server.Database.Model;

namespace tobeh.Louvre.Server.Dto;

public record AuthorizeUserDto(string Login, UserTypeEnum UserType);