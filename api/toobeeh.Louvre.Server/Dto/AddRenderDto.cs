namespace toobeeh.Louvre.Server.Dto;

public record AddRenderDto(string CloudId, int? DurationSeconds, int? FramesPerSecond, int? OptimizationLevelPercent);