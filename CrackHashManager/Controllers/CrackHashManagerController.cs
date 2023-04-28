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

    public CrackHashManagerController(CrackHashManager crackHashManager)
    {
        _crackHashManager = crackHashManager;
    }

    [HttpPost("/api/hash/crack")]
    public async Task<RequestInfoDto> RunCrackHash([FromBody] UserDataDto userDataDto)
    {
        Console.WriteLine($"Handle request to run crack hash from user by path: {Request.Path}");
        var requestInfoDto = new RequestInfoDto();
        requestInfoDto.SetRandomGuid();
        Console.WriteLine($"Generate new Guid to user request: {requestInfoDto.RequestId}");

        _crackHashManager.SendTasksToWorkers(requestInfoDto.RequestId, userDataDto);

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
                Console.WriteLine($"Request {requestId} have not finished. Set status Error. (Timeout)");
                _crackHashManager.SetClientRequestProcessingStatus(requestId, RequestProcessingStatus.Error);
            }
        });
    }
}