using mao.Models;
using mao.Services;
using Microsoft.AspNetCore.Mvc;

namespace mao.Controllers;

[ApiController]
[Route("[controller]")]
public class LobbyController : ControllerBase
{
    private readonly LobbyManagerService _lobbyManager;

    public LobbyController(LobbyManagerService lobby) {
        _lobbyManager = lobby;
    }

    [HttpPost("Start")]
    public CreatedLobby StartLobby()
    {
        return _lobbyManager.CreateLobby();
    }

    [HttpPost("Exists")]
    public ExistsLobbyResponse ExistsLobby(ExistsLobbyRequest existsLobbyRequest)
    {
        return new ExistsLobbyResponse {
            Exists = _lobbyManager.HasLobby(existsLobbyRequest.Name)
        };
    }
}
