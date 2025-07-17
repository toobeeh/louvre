using tobeh.Louvre.Server.Database.Model;

namespace tobeh.Louvre.Server.Dto;

public record AuthorizedUserDto(string Login, UserTypeEnum UserType, string Name);