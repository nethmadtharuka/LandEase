using Microsoft.AspNetCore.SignalR;

namespace LandEase.Infrastructure.Hubs;

public class SosHub : Hub
{
    // When a client connects, they join a group based on their destination country
    // This allows broadcasting SOS alerts only to relevant users
    public async Task JoinCountryGroup(string destinationCountry)
    {
        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            destinationCountry.ToLower().Trim());
    }

    public async Task LeaveCountryGroup(string destinationCountry)
    {
        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            destinationCountry.ToLower().Trim());
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}