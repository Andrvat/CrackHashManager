using System.Collections.Concurrent;
using DataContracts.Dto;
using DataContracts.Enum;
using DataContracts.MassTransit;
using Manager.Config;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Manager.Logic;

public class CrackHashManager
{
    private ConcurrentDictionary<string, RequestProcessingStatus> ClientRequestProcessingStatuses { get; set; } = new ();
    private ConcurrentDictionary<(string ClientRequestId, int WorkerId), RequestProcessingStatus> WorkerClientTaskProcessingStatuses { get; set; } = new();
    private ConcurrentDictionary<string, List<string>> ClientCrackHashWords { get; set; } = new ();

    private readonly IBus _bus;

    public CrackHashManager(IBus bus)
    {
        _bus = bus;
    }

    public async Task<IActionResult> SendTasksToWorkers(string requestId, int totalWorkersNumber,
        UserDataDto userDataDto)
    {
        SetClientRequestProcessingStatus(requestId, RequestProcessingStatus.InProgress);

        var alphabet = Environment.GetEnvironmentVariable("CRACK_HASH_ALPHABET")!;

        for (var i = 1; i <= totalWorkersNumber; i++)
        {
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

            WorkerClientTaskProcessingStatuses[(requestId, i)] = RequestProcessingStatus.InProgress;
        }

        return new OkResult();
    }

    public void AddCrackedHashWords(CrackHashWorkerResponseDto workerResponse)
    {
        var clientRequestId = workerResponse.RequestId;

        SetClientRequestWords(clientRequestId, workerResponse.Words.ToList());
        SetWorkerClientTaskProcessingStatus(clientRequestId, workerResponse.PartNumber, RequestProcessingStatus.Ready);
    }

    public void UpdateClientRequestStatus(string clientRequestId)
    {
        var clientRequestWorkerStatuses = WorkerClientTaskProcessingStatuses
            .Where(r => r.Key.ClientRequestId.Equals(clientRequestId))
            .Select(r => r.Value)
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

    public CrackResultDto GetCrackHashResult(string requestId)
    {
        var clientRequestProcessingResult = ClientRequestProcessingStatuses
            .FirstOrDefault(r => r.Key.Equals(requestId));

        return clientRequestProcessingResult.Value == RequestProcessingStatus.InProgress
            ? new CrackResultDto
            {
                Status = RequestProcessingStatus.InProgress,
                Data = null
            }
            : new CrackResultDto
            {
                Status = clientRequestProcessingResult.Value,
                Data = ClientCrackHashWords[requestId]
            };
    }

    private void SetClientRequestProcessingStatus(string requestId, RequestProcessingStatus status)
    {
        ClientRequestProcessingStatuses.AddOrUpdate(requestId, status, (_, _) => status);
    }

    private void SetWorkerClientTaskProcessingStatus(string requestId, int workerId, RequestProcessingStatus status)
    {
        WorkerClientTaskProcessingStatuses.AddOrUpdate((requestId, workerId), status, (_, _) => status);
    }
    
    private void SetClientRequestWords(string requestId, List<string> words)
    {
        if (ClientCrackHashWords.TryGetValue(requestId, out List<string> value))
        {
            value.AddRange(words);
        }
        else
        {
            ClientCrackHashWords.AddOrUpdate(requestId, words, (_, value) => value);
        }
    }
}