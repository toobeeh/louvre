namespace tobeh.Louvre.Server.Dto;

public record FindRendersFilterDto(string? NameIncludeQuery, bool? Rendered, string? DrawerName, string? ApprovedDrawerLogin, string? Language);