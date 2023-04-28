using System.Xml.Serialization;
using DataContracts;
using DataContracts.Dto;
using DataContracts.MassTransit;
using Manager.Controllers;
using Manager.Logic;
using MassTransit;

namespace Manager.Consumers;

public class TaskFinishedConsumer : IConsumer<ITaskFinished>
{
    private readonly MessageService<CrackHashWorkerResponseDto> _messageService;
    private readonly CrackHashManager _crackHashManager;

    public TaskFinishedConsumer(MessageService<CrackHashWorkerResponseDto> messageService, CrackHashManager crackHashManager)
    {
        _messageService = messageService;
        _crackHashManager = crackHashManager;
    }

    public Task Consume([XmlElement] ConsumeContext<ITaskFinished> context)
    {
        var message = context.Message;
        var workerResponse = MapperConfig.GetAutomapperInstance().Map<CrackHashWorkerResponseDto>(message);

        _messageService.AddMessage(workerResponse);
        
        while (_messageService.ContainsUnreadMessages())
        { 
            _crackHashManager?.ReceiveWorkerResponse();
        }

        return Task.CompletedTask;
    }
}