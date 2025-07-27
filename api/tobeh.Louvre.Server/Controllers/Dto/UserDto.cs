using System.ComponentModel.DataAnnotations;
using tobeh.Louvre.Server.Database.Model;

namespace tobeh.Louvre.Server.Controllers.Dto;

public record UserDto(int TypoId, [Required] UserTypeEnum UserType, string Name);