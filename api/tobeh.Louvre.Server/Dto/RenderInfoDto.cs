namespace tobeh.Louvre.Server.Dto;

public record RenderInfoDto(Ulid Id, string Title, string Drawer, bool TitleApproved, bool DrawerApproved, bool FinishedRendering, string GifUrl, string ThumbnailUrl);