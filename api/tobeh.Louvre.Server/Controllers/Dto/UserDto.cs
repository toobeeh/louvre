using tobeh.Louvre.Server.Database.Model;

namespace tobeh.Louvre.Server.Controllers.Dto;

public record UserDto(string Login, UserTypeEnum UserType, string Name);