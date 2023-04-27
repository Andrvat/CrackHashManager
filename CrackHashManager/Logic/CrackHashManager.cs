using DataContracts.Dto;
using DataContracts.Enum;
using DataContracts.MassTransit;
using Manager.Database;
using Manager.DataContracts.Entities;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace Manager.Logic;

public class CrackHashManager
{
    private readonly IBus _bus;
    private readonly ICrackHashService _crackHashService;

    public CrackHashManager(IBus bus, ICrackHashService crackHashService)
    {
        _bus = bus;
        _crackHashService = crackHashService;
    }

    public async Task<IActionResult> SendTasksToWorkers(string requestId, int totalWorkersNumber,
        UserDataDto userDataDto)
    {
        SetClientRequestProcessingStatus(requestId, RequestProcessingStatus.InProgress);

        var alphabet = Environment.GetEnvironmentVariable("CRACK_HASH_ALPHABET")!;
        
        var requestResultEntity = new CrackHashRequestResultEntity
        {
            RequestId = requestId,
            Status = RequestProcessingStatus.InProgress,
            Data = new List<string>()
        };
        await _crackHashService.AddOrUpdateRequestInfo(requestResultEntity);

        
        for (var i = 1; i <= totalWorkersNumber; i++)
        {
            await _crackHashService.AddOrUpdateWorkerRequestProcessingStatus(new CrackHashWorkerRequestProcessingStatusEntity
            {
                RequestId = requestId,
                WorkerId = i,
                Status = RequestProcessingStatus.InProgress
            });
            
            Console.WriteLine($"Sends message with guid {requestId} to {i}-worker via RabbitMq: {_bus.Address}");
            await _bus.Publish<ISendWorkerTask>(new
            {
                RequestId = requestId,
                PartNumber = i,
                PartCount = totalWorkersNumber,
                Hash = userDataDto.Hash,
                MaxLength = userDataDto.MaxLength,
                Alphabet = new Alphabet(alphabet.ToCharArray())
            });
        }

        return new OkResult();
    }

    public async Task AddCrackedHashWords(CrackHashWorkerResponseDto workerResponse)
    {
        var clientRequestId = workerResponse.RequestId;

        await SetClientRequestWords(clientRequestId, workerResponse.Words.ToList());
        await SetWorkerClientTaskProcessingStatus(new CrackHashWorkerRequestProcessingStatusEntity
        {
            RequestId = clientRequestId,
            WorkerId = workerResponse.PartNumber,
            Status = RequestProcessingStatus.Ready
        });
    }

    public async Task UpdateClientRequestStatus(string clientRequestId)
    {
        var workerRequestProcessingStatusEntities = await _crackHashService.GetWorkerRequestProcessingStatusesByRequestId(clientRequestId);
        var clientRequestWorkerStatuses = workerRequestProcessingStatusEntities
            .Select(e => e.Status)
            .ToList();

        var errorStatusCount = clientRequestWorkerStatuses.Count(s => s == RequestProcessingStatus.Error);
        if (errorStatusCount != 0)
        {
            SetClientRequestProcessingStatus(clientRequestId, RequestProcessingStatus.Error);
            return;
        }

        var inProgressStatusCount = clientRequestWorkerStatuses.Count(s => s == RequestProcessingStatus.InProgress);
        if (inProgressStatusCount == 0)
        {
            SetClientRequestProcessingStatus(clientRequestId, RequestProcessingStatus.Ready);
        }
    }

    public async Task<CrackResultDto?> GetCrackHashResult(string requestId)
    {
        var requestInfo = await _crackHashService.GetRequestResultByRequestId(requestId);
        if (requestInfo == null)
        {
            return null;
        }

        return new CrackResultDto
        {
            Status = requestInfo.Status,
            Data = requestInfo.Status == RequestProcessingStatus.Ready
                ? requestInfo.Data
                : null
        };
    }

    public async void SetClientRequestProcessingStatus(string requestId, RequestProcessingStatus status)
    {
        await _crackHashService.UpdateRequestProcessingStatus(requestId, status);
    }

    private async Task SetWorkerClientTaskProcessingStatus(CrackHashWorkerRequestProcessingStatusEntity entity)
    {
        await _crackHashService.AddOrUpdateWorkerRequestProcessingStatus(entity);
    }

    private async Task SetClientRequestWords(string requestId, List<string> words)
    {
        await _crackHashService.UpdateRequestData(requestId, words);
    }
}