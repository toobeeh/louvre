using toobeeh.Louvre.Server.Database.Model;

namespace toobeeh.Louvre.Server.Dto;

public record AuthorizedUserDto(string Login, UserTypeEnum UserType, string Name);