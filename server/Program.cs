using mao.Hubs;
using mao.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

builder.Services.AddSingleton<LobbyManagerService>();
builder.Services.AddTransient<LobbyService>();

builder.Services.AddCors(options => options.AddPolicy("CorsPolicy",
        policy =>
        {
            policy.AllowAnyHeader()
                  .AllowAnyMethod()
                  .SetIsOriginAllowed((host) => true)
                  .AllowCredentials();
        }));

var app = builder.Build();

app.UseRouting();

app.UseCors("CorsPolicy");

app.MapHub<GameHub>("/game");

app.Run();
