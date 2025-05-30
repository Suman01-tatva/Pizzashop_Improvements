using Microsoft.AspNetCore.SignalR;
using PizzaShop.Web.Hubs;

namespace PizzaShop.Web.Notifiers.cs;

public class KotNotifier : IKotNotifier
{
    private readonly IHubContext<KotHub> _hubContext;

    public KotNotifier(IHubContext<KotHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyKotUpdates()
    {
        await _hubContext.Clients.All.SendAsync("ReceiveKotUpdate");
    }
}
