using System.Collections.Concurrent;
using mao.Hubs;
using mao.GameLogic;
using mao.Services.LobbyStates;
using Microsoft.AspNetCore.SignalR;
using mao.Utils;

namespace mao.Services;

public class LobbyService {
    public IGameHub Group { get; private set; }
    public string Name { get; private set; }
    public ConcurrentDictionary<string, User> Users { get; }= new();
    public User? Winner { get; set; }

    public User? Admin { get; private set; }

    private readonly IHubContext<GameHub, IGameHub> _hubContext;

    public LobbyWaiting WaitingState { get; }
    public LobbyInGame InGameState { get; }
    public LobbyFinished FinishedState { get; }
    public ILobbyState State { get; set; }

    public CancellationTokenSource CompletedCts { get; set; } = new();
    public CancellationToken Completed => CompletedCts.Token;

    public List<string> UserNames { get => Users.Values.Select(p => p.Name).ToList(); }

    public LobbyService(IHubContext<GameHub, IGameHub> hubContext) {
        _hubContext = hubContext;

        Name = LobbyNameGenerator.GenerateLobbyName();
        Group = _hubContext.Clients.Group(Name);

        WaitingState = new(this);
        InGameState = new(this);
        FinishedState = new(this);

        State = WaitingState;
    }

    public async Task ChangeState(ILobbyState newState) {
        State = newState;
        await State.OnStateChange();
    }

    public async Task JoinGame(HubCallerContext hubCallerContext, string userName) {
        await State.JoinGame(hubCallerContext, userName);
    }

    public async Task StartGame() {
        await State.StartGame();
    }

    public async Task SendChat(HubCallerContext hubCallerContext, string message) {
        await State.SendChat(hubCallerContext, message);
    }

    public async Task AddUser(HubCallerContext hubCallerContext, string userName) {
        string connectionId = hubCallerContext.ConnectionId;
        IGameHub connection =_hubContext.Clients.Client(connectionId);

        if (hubCallerContext.Items.ContainsKey(LobbyManagerService.LobbyKey)) {
            await connection.WriteMessage("User is already in a lobby!");
            return;
        }

        if (Users.ContainsKey(connectionId) || UserNames.Contains(userName)) {
            await connection.WriteMessage("User is already in the lobby.");
            System.Diagnostics.Debug.WriteLine("Hi");
            return;
        }

        User newUser = new User(connection, userName, connectionId);
        if (Users.TryAdd(connectionId, newUser)) {
            if (Users.Count == 1) {
                Admin = newUser;
                await connection.NotifyAdmin();
            }

            await _hubContext.Groups.AddToGroupAsync(connectionId, Name);

            await Group.LobbyUsersUpdate(UserNames);

            hubCallerContext.ConnectionAborted.Register(async () => await RemoveUser(connectionId));

            hubCallerContext.Items[LobbyManagerService.LobbyKey] = this;
            Completed.Register(() => hubCallerContext.Items.Remove(LobbyManagerService.LobbyKey));
        } else {
            await connection.WriteMessage("Could not join lobby.");
        }

    }

    public async Task RemoveUser(string connectionId) {
        Users.TryRemove(connectionId, out _);
        await Group.LobbyUsersUpdate(UserNames);

        if (Users.Count == 0) {
            CompletedCts.Cancel();
        } else {
            User newAdmin = Users.Values.First(); 
            Admin = newAdmin;
            await newAdmin.Connection.NotifyAdmin();
        }
    }
}
