using System.Collections.Concurrent;
using mao.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace mao.Services;

public class LobbyService
{
    public IGameHub? Group { get; private set; }
    public string? Name { get; private set; }

    private readonly ConcurrentDictionary<string, Player> _players = new();
    private readonly IHubContext<GameHub, IGameHub> _hubContext;

    private readonly CancellationTokenSource _completedCts = new();
    public CancellationToken Completed => _completedCts.Token;

    public List<string> PlayerNames { get => _players.Values.Select(p => p.Name).ToList(); }

    public LobbyService(IHubContext<GameHub, IGameHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task AddPlayer(HubCallerContext hubCallerContext, string playerName)
    {
        if (Group == null || Name == null) {
            return;
        }
 
        string connectionId = hubCallerContext.ConnectionId;
        IGameHub connection =_hubContext.Clients.Client(connectionId);

        if (hubCallerContext.Items.ContainsKey(LobbyManagerService.LobbyKey))
        {
            await connection.WriteMessage("Already in a lobby!");
            return;
        }

        if (_players.ContainsKey(connectionId) || PlayerNames.Contains(playerName))
        {
            return;
        }

        if (_players.TryAdd(connectionId, new Player(connection, playerName)))
        {
            await _hubContext.Groups.AddToGroupAsync(connectionId, Name);

            await Group.WriteMessage(string.Join(", ", PlayerNames));

            hubCallerContext.ConnectionAborted.Register(() => RemovePlayer(connectionId));

            hubCallerContext.Items[LobbyManagerService.LobbyKey] = this;
            Completed.Register(() => hubCallerContext.Items.Remove(LobbyManagerService.LobbyKey));
        }
        else
        {
            await connection.WriteMessage("Could not join lobby.");
        }

    }

    public void RemovePlayer(string connectionId)
    {
        _players.TryRemove(connectionId, out _);
    }

    public void Init(string lobbyName)
    {
        if (Group != null && Name != null) {
            return;
        }

        Name = lobbyName;
        Group = _hubContext.Clients.Group(lobbyName);
    }
}