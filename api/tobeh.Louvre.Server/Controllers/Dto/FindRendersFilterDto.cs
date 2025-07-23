namespace tobeh.Louvre.Server.Controllers.Dto;

public record FindRendersFilterDto(string? NameIncludeQuery, bool? Rendered, string? DrawerName, int? ApprovedDrawerTypoId, string? Language);