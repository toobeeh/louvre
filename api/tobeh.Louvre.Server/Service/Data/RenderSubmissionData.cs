using tobeh.Louvre.Server.Controllers.Dto;
using tobeh.Louvre.TypoApiClient;

namespace tobeh.Louvre.Server.Service.Data;

public record RenderSubmissionData(
    CloudImageDto CloudImage,
    RenderData Render,
    RenderSubmissionDto RenderSubmissionDetails,
    UserDto User
);