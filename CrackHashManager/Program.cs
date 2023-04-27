using DataContracts.Dto;
using DataContracts.MassTransit;
using Manager.Config;
using Manager.Consumers;
using Manager.Database;
using Manager.Logic;
using MassTransit;
using MassTransit.Transports.Fabric;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection(nameof(AppSettings)));

builder.Services.AddScoped<ICrackHashDbContext, CrackHashDbContext>();
builder.Services.AddScoped<IMongoDbConfig, MongoDbConfig>();
builder.Services.AddScoped<ICrackHashService, CrackHashService>();

builder.Services.AddScoped<CrackHashManager>();
builder.Services.AddScoped<TaskFinishedConsumer>();
builder.Services.AddSingleton<MessageService<CrackHashWorkerResponseDto>>();

builder.Services.AddMassTransit(x => { 
    x.UsingRabbitMq((busRegistrationContext, busFactoryConfigurator) =>
    {
        
        
        busFactoryConfigurator.Host(new Uri(Environment.GetEnvironmentVariable("RABBITMQ_3_URI")!), h =>
        {
            h.Username(Environment.GetEnvironmentVariable("RABBITMQ_3_LOGIN")!);
            h.Password(Environment.GetEnvironmentVariable("RABBITMQ_3_PASSWORD")!);
            h.Heartbeat(TimeSpan.Zero);
        });
        
        // busFactoryConfigurator.Publish<ISendWorkerTask>(x => { x.ExchangeType = "direct";});

        busFactoryConfigurator.ReceiveEndpoint("worker-task-finished", e =>
        {
            // e.Bind("worker-task-finished-exchange", x =>
            // {
            //     x.Durable = true;
            //     x.AutoDelete = false;
            //     x.ExchangeType = "direct";
            // });
            // e.Bind<ITaskFinished>();
            e.Consumer<TaskFinishedConsumer>(busRegistrationContext);
            e.PurgeOnStartup = false;
            e.Durable = true;
            e.AutoDelete = false;
        });
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();