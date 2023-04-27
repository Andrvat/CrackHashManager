using DataContracts.Dto;
using DataContracts.Enum;
using DataContracts.MassTransit;
using Manager.Logic;
using Microsoft.AspNetCore.Mvc;

namespace Manager.Controllers;

[ApiController]
public class CrackHashManagerController : ControllerBase
{
    private readonly CrackHashManager _crackHashManager;
    private readonly MessageService<CrackHashWorkerResponseDto> _messageService;

    public CrackHashManagerController(
        CrackHashManager crackHashManager,
        MessageService<CrackHashWorkerResponseDto> messageService)
    {
        _crackHashManager = crackHashManager;
        _messageService = messageService;
    }

    [HttpPost("/api/hash/crack")]
    public async Task<RequestInfoDto> RunCrackHash([FromBody] UserDataDto userDataDto)
    {
        Console.WriteLine($"Handle request to run crack hash from user by path: {Request.Path}");
        var requestInfoDto = new RequestInfoDto();
        requestInfoDto.SetRandomGuid();
        Console.WriteLine($"Generate new Guid to user request: {requestInfoDto.RequestId}");

        await _crackHashManager.SendTasksToWorkers(requestInfoDto.RequestId, userDataDto);

        SetRequestProcessingTimeout(requestInfoDto);

        return requestInfoDto;
    }

    [HttpGet("/api/hash/status")]
    public async Task<CrackResultDto?> GetCrackHashResult([FromQuery] string requestId)
    {
        Console.WriteLine(
            $"Handle request to get crack hash result from user by path: {Request.Path}. Request id: {requestId}");
        
        var result = await _crackHashManager.GetCrackHashResult(requestId);
        return result;
    }

    [HttpPatch("/internal/api/manager/hash/crack/request")]
    public async Task<IActionResult> AddCrackedHashWords()
    {
        Console.WriteLine($"Handle response from worker by path: {Request.Path}");

        var workerResponse = _messageService.GetMessage();
        Console.WriteLine($"Get worker response {workerResponse}");

        await _crackHashManager.AddCrackedHashWords(workerResponse);
        await _crackHashManager.UpdateClientRequestStatus(workerResponse.RequestId);
        return Ok();
    }

    private void SetRequestProcessingTimeout(RequestInfoDto requestInfo)
    {
        var millisecondsDelay = (int) TimeSpan.FromSeconds(
            int.Parse(Environment.GetEnvironmentVariable("MESSAGE_ERROR_TIMEOUT_SEC")!)).TotalMilliseconds;
        Task.Delay(millisecondsDelay).ContinueWith(async _ =>
        {
            var requestId = requestInfo.RequestId;
            Console.WriteLine($"Delay for {millisecondsDelay} millis with request id: {requestId} completed");
            var crackHashResult = await _crackHashManager.GetCrackHashResult(requestId);
            if (crackHashResult == null || crackHashResult.Status != RequestProcessingStatus.Ready)
            {
                Console.WriteLine($"Request {requestId} have not finished. Set status Error");
                _crackHashManager.SetClientRequestProcessingStatus(requestId, RequestProcessingStatus.Error);
            }
        });
    }
}