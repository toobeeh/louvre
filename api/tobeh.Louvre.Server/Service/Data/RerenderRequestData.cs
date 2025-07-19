using tobeh.Louvre.Server.Controllers.Dto;
using tobeh.Louvre.TypoApiClient;

namespace tobeh.Louvre.Server.Service.Data;

public record RerenderRequestData(
    RenderData Render,
    RenderParametersDto? Parameters,
    CloudImageDto CloudImage
);