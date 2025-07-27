namespace tobeh.Louvre.Server.Controllers.Dto;

public record FindRendersFilterDto(
    string? TitleIncludeQuery,
    bool? Rendered, 
    string? DrawerName, 
    int? ApprovedDrawerTypoId, 
    string? Language,
    bool? Approved,
    int? PageSize,
    int? Page
    );