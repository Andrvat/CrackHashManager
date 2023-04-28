using Manager.Database;
using Manager.Logic;

namespace Manager.Utilities;

public class UnpublishedWorkerTasksSender : BackgroundService
{
    private readonly ICrackHashService _crackHashService;
    private readonly CrackHashManager _crackHashManager;
    
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly TaskCompletionSource _source = new();

    public UnpublishedWorkerTasksSender(
        ICrackHashService crackHashService, 
        CrackHashManager crackHashManager, 
        IHostApplicationLifetime hostApplicationLifetime)
    {
        _crackHashService = crackHashService;
        _crackHashManager = crackHashManager;
        _hostApplicationLifetime = hostApplicationLifetime;
        _hostApplicationLifetime.ApplicationStarted.Register(() => _source.SetResult());
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _source.Task.ConfigureAwait(false);
        await SendUnpublishedWorkerTasks();
    }

    private async Task SendUnpublishedWorkerTasks()
    {
        Console.WriteLine("Start to handle unpublished worker tasks");
        var unpublishedWorkerTasks = await _crackHashService.GetUnpublishedWorkerTasks();
        Console.WriteLine($"Unpublished worker tasks total number: {unpublishedWorkerTasks.Count}");
        unpublishedWorkerTasks.ForEach(t => _crackHashManager.SendTaskToWorker(t));
    }

    // public override Task StopAsync(CancellationToken cancellationToken)
    // {
        // return Task.CompletedTask;
    // }
}