using DataContracts.Dto;
using DataContracts.MassTransit;
using Manager.Config;
using Manager.Logic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Manager.Controllers;

[ApiController]
public class CrackHashManagerController : ControllerBase
{
    private readonly CrackHashManager _crackHashManager;
    private readonly MessageService<CrackHashWorkerResponseDto> _messageService;
    private readonly IOptions<AppSettings> _appSettings;

    public CrackHashManagerController(
        CrackHashManager crackHashManager,
        MessageService<CrackHashWorkerResponseDto> messageService,
        IOptions<AppSettings> appSettings)
    {
        _crackHashManager = crackHashManager;
        _messageService = messageService;
        _appSettings = appSettings;

        var httpClientHandler = new HttpClientHandler();
        httpClientHandler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
    }

    [HttpPost("/api/hash/crack")]
    public async Task<RequestInfoDto> RunCrackHash([FromBody] UserDataDto userDataDto)
    {
        Console.WriteLine($"Handle request to run crack hash from user by path: {Request.Path}");
        var totalWorkersNumber = int.Parse(Environment.GetEnvironmentVariable("WORKERS_NUMBER")!);
        var requestInfoDto = new RequestInfoDto();
        requestInfoDto.SetRandomGuid();
        Console.WriteLine($"Generate new Guid to user request: {requestInfoDto.RequestId}");
        
        Console.WriteLine($"Send tasks to {totalWorkersNumber} workers");
        await _crackHashManager.SendTasksToWorkers(requestInfoDto.RequestId, totalWorkersNumber, userDataDto);

        return requestInfoDto;
    }

    [HttpGet("/api/hash/status")]
    public async Task<CrackResultDto> GetCrackHashResult([FromQuery] string requestId)
    {
        Console.WriteLine(
            $"Handle request to get crack hash result from user by path: {Request.Path}. Request id: {requestId}");
        var result = _crackHashManager.GetCrackHashResult(requestId);
        return result;
    }

    [HttpPatch("/internal/api/manager/hash/crack/request")]
    public async Task<IActionResult> AddCrackedHashWords()
    {
        Console.WriteLine($"Handle response from worker by path: {Request.Path}");

        var workerResponse = _messageService.GetMessage();
        Console.WriteLine($"Get worker response {workerResponse}");

        _crackHashManager.AddCrackedHashWords(workerResponse);
        _crackHashManager.UpdateClientRequestStatus(workerResponse.RequestId);
        return Ok();
    }
}