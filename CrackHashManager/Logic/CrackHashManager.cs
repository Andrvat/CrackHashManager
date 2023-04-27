using DataContracts.Dto;
using DataContracts.Enum;
using DataContracts.MassTransit;
using Manager.Database;
using Manager.DataContracts.Entities;
using Manager.Utilities;
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

    public async Task<IActionResult> SendTasksToWorkers(string requestId, UserDataDto userDataDto)
    {
        SetClientRequestProcessingStatus(requestId, RequestProcessingStatus.InProgress);

        var requestResultEntity = new CrackHashRequestResultEntity
        {
            RequestId = requestId,
            Status = RequestProcessingStatus.InProgress,
            Data = new List<string>()
        };
        await _crackHashService.AddOrUpdateRequestInfo(requestResultEntity);

        var totalWorkersNumber = int.Parse(Environment.GetEnvironmentVariable("WORKERS_NUMBER")!);
        for (var i = 1; i <= totalWorkersNumber; i++)
        {
            var workerTaskEntity = new CrackHashWorkerTaskEntity
            {
                RequestId = requestId,
                WorkerId = i,
                Hash = userDataDto.Hash,
                MaxLength = userDataDto.MaxLength,
                Status = RequestProcessingStatus.InProgress
            };

            try
            {
                await SendTaskToWorker(workerTaskEntity);
            }
            catch (Exception)
            {
                break;
            }
        }

        return new OkResult();
    }

    private async Task SendTaskToWorker(CrackHashWorkerTaskEntity entity)
    {
        await _crackHashService.AddOrUpdateWorkerTask(entity);
        try
        {
            await PublishWorkerTask(entity);
        }
        catch (Exception e)
        {
            Console.WriteLine("Message publish failed. Too many attempts to publish message to RabbitMQ.");
            await _crackHashService.AddOrUpdateRequestInfo(new CrackHashRequestResultEntity
            {
                RequestId = entity.RequestId,
                Status = RequestProcessingStatus.Error,
                Data = new List<string>()
            });
            
            throw;
        }
        
        SetWorkerRequestProcessingTimeout(entity);
    }

    private async Task PublishWorkerTask(CrackHashWorkerTaskEntity entity)
    {
        await RetryHelper.RetryOnExceptionAsync(
            int.Parse(Environment.GetEnvironmentVariable("REPUBLISH_MESSAGE_DELAY_SECONDS")!),
            TimeSpan.FromSeconds(int.Parse(Environment.GetEnvironmentVariable("REPUBLISH_MESSAGE_MAX_ATTEMPTS")!)),
            async () =>
            {
                Console.WriteLine($"Sends message with guid {entity.RequestId} to {entity.WorkerId}-worker via RabbitMq: {_bus.Address}");
                await _bus.Publish<ISendWorkerTask>(new
                {
                    RequestId = entity.RequestId,
                    PartNumber = entity.WorkerId,
                    PartCount = int.Parse(Environment.GetEnvironmentVariable("WORKERS_NUMBER")!),
                    Hash = entity.Hash,
                    MaxLength = entity.MaxLength,
                    Alphabet = new Alphabet(Environment.GetEnvironmentVariable("CRACK_HASH_ALPHABET")!.ToCharArray())
                });
            });
    }

    public async Task AddCrackedHashWords(CrackHashWorkerResponseDto workerResponse)
    {
        var clientRequestId = workerResponse.RequestId;

        await SetClientRequestWords(clientRequestId, workerResponse.Words.ToList());
        await SetWorkerClientTaskProcessingStatus(new CrackHashWorkerTaskEntity
        {
            RequestId = clientRequestId,
            WorkerId = workerResponse.PartNumber,
            Status = RequestProcessingStatus.Ready
        });
    }

    public async Task UpdateClientRequestStatus(string clientRequestId)
    {
        var workerRequestProcessingStatusEntities = await _crackHashService.GetWorkerTasksByRequestId(clientRequestId);
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

    private async Task SetWorkerClientTaskProcessingStatus(CrackHashWorkerTaskEntity entity)
    {
        await _crackHashService.AddOrUpdateWorkerTask(entity);
    }

    private async Task SetClientRequestWords(string requestId, List<string> words)
    {
        await _crackHashService.UpdateRequestData(requestId, words);
    }

    private void SetWorkerRequestProcessingTimeout(CrackHashWorkerTaskEntity entity)
    {
        var millisecondsDelay = (int)TimeSpan.FromSeconds(
            int.Parse(Environment.GetEnvironmentVariable("WORKER_TASK_PROCESSING_TIMEOUT_SEC")!)).TotalMilliseconds;
        Task.Delay(millisecondsDelay).ContinueWith(async _ =>
        {
            var requestId = entity.RequestId;
            Console.WriteLine(
                $"Delay for {millisecondsDelay} millis with request id: {requestId} completed for worker {entity.WorkerId}");
            var requestResult = await _crackHashService.GetRequestResultByRequestId(requestId);
            if (requestResult != null && requestResult.Status == RequestProcessingStatus.InProgress)
            {
                var crackHashResult = await _crackHashService.GetWorkerTasksByRequestId(requestId);
                var taskProcessingStatus = crackHashResult
                    .FirstOrDefault(e => e.WorkerId == entity.WorkerId);
                if (taskProcessingStatus == null || taskProcessingStatus.Status == RequestProcessingStatus.InProgress)
                {
                    Console.WriteLine(
                        $"Request {requestId} on worker {entity.WorkerId} have not finished. (Rerun worker task)");
                    await SendTaskToWorker(entity);
                }
                else
                {
                    Console.WriteLine($"Request {requestId} on worker {entity.WorkerId} have finished. (Stop rerun)");
                }
            }
            else
            {
                entity.Status = RequestProcessingStatus.Error;
                await _crackHashService.AddOrUpdateWorkerTask(entity);
                Console.WriteLine(
                    $"Request {requestId} on worker {entity.WorkerId} have finished or failed. (Stop rerun)");
            }
        });
    }
}