using AutoMapper;
using DataContracts.Dto;
using DataContracts.MassTransit;

namespace DataContracts;

public class MapperConfig
{
    public static Mapper GetAutomapperInstance()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ITaskFinished, CrackHashWorkerResponseDto>();
            cfg.CreateMap<ISendWorkerTask, CrackHashManagerRequestDto>()
                .ForMember(dest => dest.Alphabet, opts => opts.MapFrom(src => new string(src.Alphabet.Symbols)));
        });

        var mapper = new Mapper(config);
        return mapper;
    }
}