using Microsoft.AspNetCore.SignalR;

namespace battleships.api;

public class MatchHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}