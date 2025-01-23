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

app.UseAuthorization();

app.MapControllers();

app.UseCors("CorsPolicy");

app.MapHub<GameHub>("/game");

app.Run();
