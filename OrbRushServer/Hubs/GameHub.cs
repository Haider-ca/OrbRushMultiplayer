using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace OrbRushServer.Hubs
{
	// Author: Jerry
	// Responsibility: SignalR hub that relays game messages between players
	public class GameHub : Hub
	{
		private static readonly ConcurrentDictionary<string, PlayerInfo> PlayersByConnection =
			new ConcurrentDictionary<string, PlayerInfo>();

		public async Task SendJoin(int playerId, float x, float y, float z)
		{
			PlayersByConnection[Context.ConnectionId] = new PlayerInfo(playerId, x, y, z);

			foreach (PlayerInfo player in PlayersByConnection.Values)
			{
				if (player.PlayerId == playerId)
					continue;

				await Clients.Caller.SendAsync("ReceiveJoin",
					player.PlayerId,
					player.X,
					player.Y,
					player.Z);
			}

			await Clients.Others.SendAsync("ReceiveJoin", playerId, x, y, z);
		}

		public async Task SendMove(int playerId, float x, float y, float z, long sequence)
		{
			if (PlayersByConnection.TryGetValue(Context.ConnectionId, out PlayerInfo? player))
				PlayersByConnection[Context.ConnectionId] = player with { X = x, Y = y, Z = z };

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

		public async Task SendOrbCollect(int playerId, float x, float y, float z, long sequence)
		{
			await Clients.All.SendAsync("ReceiveOrbCollect", playerId, x, y, z, sequence);
		}

		public async Task SendGameOver(int winnerId)
		{
			await Clients.Others.SendAsync("ReceiveGameOver", winnerId);
		}

		public override async Task OnDisconnectedAsync(Exception? exception)
		{
			if (PlayersByConnection.TryRemove(Context.ConnectionId, out PlayerInfo? player))
				await Clients.Others.SendAsync("ReceivePlayerLeft", player.PlayerId);

			await base.OnDisconnectedAsync(exception);
		}

		private sealed record PlayerInfo(int PlayerId, float X, float Y, float Z);
	}
}
