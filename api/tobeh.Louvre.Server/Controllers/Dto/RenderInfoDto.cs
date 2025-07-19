namespace tobeh.Louvre.Server.Controllers.Dto;

public record RenderInfoDto(
    Ulid Id, 
    string Title, 
    string Drawer, 
    bool TitleApproved, 
    bool DrawerApproved, 
    bool FinishedRendering, 
    string GifUrl, 
    string ThumbnailUrl,
    DateTimeOffset SubmittedAt,
    RenderParametersDto? RenderParameters,
    bool Approved,
    string Language
    );