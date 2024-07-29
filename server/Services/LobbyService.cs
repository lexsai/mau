using System.Collections.Concurrent;
using mao.Hubs;
using mao.GameLogic;
using mao.Services.LobbyStates;
using Microsoft.AspNetCore.SignalR;

namespace mao.Services;

public class LobbyService {
    public IGameHub? Group { get; private set; }
    public string? Name { get; private set; }
    public ConcurrentDictionary<string, User> Users { get; }= new();

    private readonly IHubContext<GameHub, IGameHub> _hubContext;

    public LobbyWaiting WaitingState { get; }
    public LobbyWaiting InGameState { get; }

    private ILobbyState _state;
    public ILobbyState State {
        get => _state;
        set {
            _state = value;
            _state.OnStateChange();
        }
    }

    private readonly CancellationTokenSource _completedCts = new();
    public CancellationToken Completed => _completedCts.Token;

    public List<string> UserNames { get => Users.Values.Select(p => p.Name).ToList(); }

    public LobbyService(IHubContext<GameHub, IGameHub> hubContext) {
        _hubContext = hubContext;

        WaitingState = new(this);
        InGameState = new(this);

        _state = WaitingState;
    }

    public void Init(string lobbyName) {
        if (Group != null && Name != null) {
            return;
        }

        Name = lobbyName;
        Group = _hubContext.Clients.Group(lobbyName);
    }

    public async Task JoinGame(HubCallerContext hubCallerContext, string userName) {
        await State.JoinGame(hubCallerContext, userName);
    }

    public async Task StartGame() {
        await State.StartGame();
    }

    public async Task AddUser(HubCallerContext hubCallerContext, string userName) {
        if (Group == null || Name == null) {
            return;
        }

        string connectionId = hubCallerContext.ConnectionId;
        IGameHub connection =_hubContext.Clients.Client(connectionId);

        if (hubCallerContext.Items.ContainsKey(LobbyManagerService.LobbyKey)) {
            await connection.WriteMessage("User is already in a lobby!");
            return;
        }

        if (Users.ContainsKey(connectionId) || UserNames.Contains(userName)) {
            await connection.WriteMessage("User is already in the lobby.");
            return;
        }

        if (Users.TryAdd(connectionId, new User(connection, userName))) {
            await _hubContext.Groups.AddToGroupAsync(connectionId, Name);

            await Group.LobbyUsersUpdate(string.Join(", ", UserNames));

            hubCallerContext.ConnectionAborted.Register(async () => await RemoveUser(connectionId));

            hubCallerContext.Items[LobbyManagerService.LobbyKey] = this;
            Completed.Register(() => hubCallerContext.Items.Remove(LobbyManagerService.LobbyKey));
        } else {
            await connection.WriteMessage("Could not join lobby.");
        }

    }

    public async Task RemoveUser(string connectionId) {
        if (Group == null) {
            return;
        }

        Users.TryRemove(connectionId, out _);
        await Group.LobbyUsersUpdate(string.Join(", ", UserNames));
    }
}
