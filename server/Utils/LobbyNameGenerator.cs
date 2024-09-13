namespace mao.Utils;

internal static class LobbyNameGenerator
{
    public static string GenerateLobbyName()
    {
        return Guid.NewGuid().ToString()[0..8];
    }
}