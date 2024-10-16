using AiChatbot.Controller;
using AiChatbot.DB;
using AiChatbotService.Utils;
using Microsoft.EntityFrameworkCore;
using WatchDog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ApplicationDBContext>(opt =>
{
    opt.UseNpgsql(builder.Configuration.GetConnectionString("PostgresAiChatbot"));
});

builder.Services.AddWatchDogServices(opt =>
{
    opt.SetExternalDbConnString = builder.Configuration.GetConnectionString("PostgresAiChatbot");
    opt.DbDriverOption = WatchDog.src.Enums.WatchDogDbDriverEnum.PostgreSql;
});

builder.Configuration.AddEnvironmentVariables("twitch_");


var app = builder.Build();

app.Configuration.GetSection("OpenAI").Bind(Config.openAI);
app.Configuration.GetSection("watchdog").Bind(UtilsLib.Config.watchdog);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapChatBotEndpoints();
app.UseHttpsRedirection();

app.UseWatchDogExceptionLogger();

app.UseWatchDog(opt =>
{
    opt.WatchPageUsername = UtilsLib.Config.watchdog.username;
    opt.WatchPagePassword = UtilsLib.Config.watchdog.password;
});

app.Run();
