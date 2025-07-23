using tobeh.Louvre.Server.Database.Model;

namespace tobeh.Louvre.Server.Controllers.Dto;

public record UserDto(int TypoId, UserTypeEnum UserType, string Name);