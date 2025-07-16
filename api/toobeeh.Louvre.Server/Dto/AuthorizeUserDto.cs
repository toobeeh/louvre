using toobeeh.Louvre.Server.Database.Model;

namespace toobeeh.Louvre.Server.Dto;

public record AuthorizeUserDto(string Login, UserTypeEnum UserType);