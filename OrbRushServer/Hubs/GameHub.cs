using Microsoft.AspNetCore.SignalR;

namespace OrbRushServer.Hubs
{
	// Author: Jerry
	// Responsibility: SignalR hub that relays game messages between players
	public class GameHub : Hub
	{
		public async Task SendJoin(int playerId, float x, float y, float z)
		{
			await Clients.Others.SendAsync("ReceiveJoin", playerId, x, y, z);
		}

		public async Task SendMove(int playerId, float x, float y, float z, long sequence)
		{
			await Clients.Others.SendAsync("ReceiveMove", playerId, x, y, z, sequence);
		}

		public async Task SendScore(int playerId, int score)
		{
			await Clients.Others.SendAsync("ReceiveScore", playerId, score);
		}

		public async Task SendOrbSpawn(float x, float y, float z)
		{
			await Clients.Others.SendAsync("ReceiveOrbSpawn", x, y, z);
		}

		public async Task SendGameOver(int winnerId)
		{
			await Clients.Others.SendAsync("ReceiveGameOver", winnerId);
		}
	}
}
