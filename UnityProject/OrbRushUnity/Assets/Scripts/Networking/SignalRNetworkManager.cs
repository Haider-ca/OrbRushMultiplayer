using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using UnityEngine;
using OrbRush.GameLogic;
using OrbRush.Utilities;
using OrbRush.UI;

namespace OrbRush.Networking
{
	// Author: Jerry
	// Responsibility: SignalR/WebSocket client that replaces UDP for internet play
	public class SignalRNetworkManager : MonoBehaviour
	{
		public static SignalRNetworkManager Instance;

		public string serverUrl = "http://localhost:5264/gamehub";

		private HubConnection connection;
		private bool isConnected = false;
		private int localPlayerId;
		private Vector3 localSpawnPosition;

		private void Awake()
		{
			Instance = this;
		}

		private async void Start()
		{
			localPlayerId = UnityEngine.Random.Range(1000, 9999);
			localSpawnPosition = new Vector3(
				UnityEngine.Random.Range(-6f, 6f),
				5f,
				UnityEngine.Random.Range(-6f, 6f));

			GameManager.Instance.SpawnLocalPlayer(localPlayerId, localSpawnPosition);

			if (HUDController.Instance != null)
					HUDController.Instance.SetStatusText("Local Player ID: " + localPlayerId + " (SignalR)");

			connection = new HubConnectionBuilder()
				.WithUrl(serverUrl)
				.WithAutomaticReconnect()
				.Build();

			RegisterHandlers();

			try
			{
				await connection.StartAsync();
				isConnected = true;
				Debug.Log("[SignalR] Connected to server: " + serverUrl);

				await SendLocalJoinAsync();
			}
			catch (Exception ex)
			{
				Debug.LogError("[SignalR] Connection failed: " + ex.Message);
				if (HUDController.Instance != null)
					HUDController.Instance.SetStatusText("SignalR connection failed!");
			}

			connection.Closed += async (error) =>
			{
				isConnected = false;
				Debug.LogWarning("[SignalR] Disconnected. Reconnecting...");
				await Task.Delay(2000);
					try
					{
						await connection.StartAsync();
						isConnected = true;
						await SendLocalJoinAsync();
					}
					catch (Exception ex)
					{
					Debug.LogError("[SignalR] Reconnect failed: " + ex.Message);
				}
			};
		}

		private void RegisterHandlers()
		{
			connection.On<int, float, float, float>("ReceiveJoin",
				(playerId, x, y, z) =>
				{
					MainThreadDispatcher.Enqueue(() =>
					{
						GameManager.Instance.SpawnRemotePlayer(
							playerId, new Vector3(x, y, z));
					});
				});

			connection.On<int>("ReceivePlayerLeft",
				(playerId) =>
				{
					MainThreadDispatcher.Enqueue(() =>
					{
						GameManager.Instance.RemoveRemotePlayer(playerId);
					});
				});

			connection.On<int, float, float, float, long>("ReceiveMove",
				(playerId, x, y, z, sequence) =>
				{
					MainThreadDispatcher.Enqueue(() =>
					{
						GameManager.Instance.UpdateRemotePlayerPosition(
							playerId, new Vector3(x, y, z));
					});
				});

			connection.On<int, int>("ReceiveScore",
				(playerId, score) =>
				{
					MainThreadDispatcher.Enqueue(() =>
					{
						ScoreManager.Instance.SetRemoteScore(playerId, score);
					});
				});

			connection.On<float, float, float>("ReceiveOrbSpawn",
				(x, y, z) =>
				{
					MainThreadDispatcher.Enqueue(() =>
					{
						OrbSpawner.Instance.ApplyRemoteOrbSpawn(
							new Vector3(x, y, z));
					});
				});

			connection.On<int, float, float, float, long>("ReceiveOrbCollect",
				(playerId, x, y, z, sequence) =>
				{
					MainThreadDispatcher.Enqueue(() =>
					{
						ScoreManager.Instance.ApplyOrbCollected(
							playerId,
							new Vector3(x, y, z));
					});
				});

			connection.On<int>("ReceiveGameOver",
				(winnerId) =>
				{
					MainThreadDispatcher.Enqueue(() =>
					{
						ScoreManager.Instance.ApplyGameOver(winnerId);
					});
				});
		}

		public void SendState(PlayerState state)
		{
			if (!isConnected || connection == null)
				return;

			switch (state.messageType)
			{
				case "JOIN":
					_ = connection.InvokeAsync("SendJoin",
						state.playerId, state.x, state.y, state.z);
					break;

				case "MOVE":
					_ = connection.InvokeAsync("SendMove",
						state.playerId, state.x, state.y, state.z, state.sequence);
					break;

				case "SCORE":
					_ = connection.InvokeAsync("SendScore",
						state.playerId, state.score);
					break;

				case "ORB_SPAWN":
					_ = connection.InvokeAsync("SendOrbSpawn",
						state.x, state.y, state.z);
					break;

				case "GAME_OVER":
					_ = connection.InvokeAsync("SendGameOver",
						state.winnerId);
					break;

				case "ORB_COLLECT":
					_ = connection.InvokeAsync("SendOrbCollect",
						state.playerId, state.x, state.y, state.z, state.sequence);
					break;
			}
		}

		private Task SendLocalJoinAsync()
		{
			return connection.InvokeAsync("SendJoin",
				localPlayerId,
				localSpawnPosition.x,
				localSpawnPosition.y,
				localSpawnPosition.z);
		}

		private async void OnDestroy()
		{
			if (connection != null)
			{
				try
				{
					await connection.StopAsync();
					await connection.DisposeAsync();
				}
				catch (Exception ex)
				{
					Debug.LogWarning("[SignalR] Error during cleanup: " + ex.Message);
				}
			}
		}
	}
}
