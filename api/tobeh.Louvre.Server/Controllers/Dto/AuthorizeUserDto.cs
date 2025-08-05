using tobeh.Louvre.Server.Database.Model;

namespace tobeh.Louvre.Server.Controllers.Dto;

public record AuthorizeUserDto(string DiscordId, UserTypeEnum UserType);