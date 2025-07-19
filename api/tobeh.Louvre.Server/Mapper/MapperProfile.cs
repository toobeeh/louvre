using AutoMapper;
using tobeh.Louvre.Server.Controllers.Dto;
using tobeh.Louvre.Server.Service.Data;

namespace tobeh.Louvre.Server.Mapper;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<RenderData, RenderInfoDto>()
            .ForCtorParam("SubmittedAt", opt => opt.MapFrom(data => data.Id.Time.UtcDateTime))
            .ForCtorParam("RenderParameters", opt => opt.MapFrom(data => MapRenderParameters(data)));
        CreateMap<RenderData, RenderPreviewDto>();
    }

    private static RenderParametersDto? MapRenderParameters(RenderData data)
    {
        if (
            !data.FinishedRendering 
            || data.RenderDurationSeconds is not { } duration 
            || data.RenderFramesPerSecond is not { } fps 
            || data.RenderOptimizationLevelPercent is not { } optimizationLevel
        ) return null;

        return new RenderParametersDto(duration, fps, optimizationLevel);
    }
}