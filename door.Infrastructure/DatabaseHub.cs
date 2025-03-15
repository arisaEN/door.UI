using Microsoft.AspNetCore.SignalR;

public class DatabaseHub : Hub
{
	public async Task NotifyClients()
	{
		await Clients.All.SendAsync("DataUpdated"); // すべてのクライアントに通知
	}
}