namespace tobeh.Louvre.Server.Dto;

public record RenderSubmissionDto(string CloudId, int? DurationSeconds, int? FramesPerSecond, int? OptimizationLevelPercent);