namespace tobeh.Louvre.Server.Controllers.Dto;

public record RenderPreviewDto(
    Ulid Id, 
    string Title, 
    string Drawer, 
    bool TitleApproved, 
    bool DrawerApproved, 
    bool FinishedRendering, 
    string GifUrl, 
    string ThumbnailUrl,
    bool Approved
);