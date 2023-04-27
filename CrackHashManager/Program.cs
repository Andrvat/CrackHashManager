using DataContracts.Dto;
using DataContracts.MassTransit;
using Manager.Config;
using Manager.Consumers;
using Manager.Database;
using Manager.Logic;
using MassTransit;

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
        });
        
        busFactoryConfigurator.ReceiveEndpoint("worker-task-finished", e =>
        {
            e.Consumer<TaskFinishedConsumer>(busRegistrationContext);
            e.PurgeOnStartup = true;
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