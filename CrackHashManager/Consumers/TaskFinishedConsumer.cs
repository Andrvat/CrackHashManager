using System.Xml.Serialization;
using DataContracts;
using DataContracts.Dto;
using DataContracts.MassTransit;
using MassTransit;

namespace Manager.Consumers;

public class TaskFinishedConsumer : IConsumer<ITaskFinished>
{
    private readonly MessageService<CrackHashWorkerResponseDto> _messageService;

    public TaskFinishedConsumer(MessageService<CrackHashWorkerResponseDto> messageService)
    {
        _messageService = messageService;
    }

    public Task Consume([XmlElement] ConsumeContext<ITaskFinished> context)
    {
        var message = context.Message;
        var workerResponse = MapperConfig.GetAutomapperInstance().Map<CrackHashWorkerResponseDto>(message);
        _messageService.AddMessage(workerResponse);
        return Task.CompletedTask;
    }
}