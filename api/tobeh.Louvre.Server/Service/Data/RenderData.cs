namespace tobeh.Louvre.Server.Service.Data;

public record RenderData(
    Ulid Id, 
    string CloudId,
    string Title, 
    string Drawer, 
    bool TitleApproved, 
    bool DrawerApproved, 
    bool FinishedRendering, 
    string GifUrl, 
    string ThumbnailUrl,
    int? RenderDurationSeconds,
    int? RenderFramesPerSecond,
    int? RenderOptimizationLevelPercent,
    bool Approved,
    string Language
    );