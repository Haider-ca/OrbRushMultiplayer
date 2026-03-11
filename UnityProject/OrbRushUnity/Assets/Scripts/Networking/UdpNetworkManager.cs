using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using OrbRush.GameLogic;
using OrbRush.Utilities;
using OrbRush.UI;

namespace OrbRush.Networking
{
	// Author: Networking Team
	// Responsibility: UDP multicast send/receive
	public class UdpNetworkManager : MonoBehaviour
	{
		public static UdpNetworkManager Instance;

		public string multicastAddress = "230.0.0.1";
		public int port = 11000;

		private UdpClient udpClient;
		private IPEndPoint multicastEndPoint;
		private Thread receiveThread;
		private bool isRunning = false;

		private readonly Dictionary<int, long> lastSequenceByPlayer = new Dictionary<int, long>();

		private void Awake()
		{
			Instance = this;
		}

		private void Start()
		{
			int localPlayerId = Random.Range(1000, 9999);
			Vector3 spawn = new Vector3(Random.Range(-6f, 6f), 5f, Random.Range(-6f, 6f));

			GameManager.Instance.SpawnLocalPlayer(localPlayerId, spawn);

			if (HUDController.Instance != null)
				HUDController.Instance.SetStatusText("Local Player ID: " + localPlayerId);

			udpClient = new UdpClient();
			udpClient.ExclusiveAddressUse = false;

			IPEndPoint localEp = new IPEndPoint(IPAddress.Any, port);
			udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			udpClient.Client.Bind(localEp);
			udpClient.JoinMulticastGroup(IPAddress.Parse(multicastAddress));

			multicastEndPoint = new IPEndPoint(IPAddress.Parse(multicastAddress), port);

			isRunning = true;
			receiveThread = new Thread(ReceiveLoop);
			receiveThread.IsBackground = true;
			receiveThread.Start();

			SendJoin(localPlayerId, spawn);
		}

		private void OnDestroy()
		{
			isRunning = false;

			try
			{
				receiveThread?.Interrupt();
			}
			catch
			{
			}

			try
			{
				udpClient?.Close();
			}
			catch
			{
			}
		}

		public void SendState(PlayerState state)
		{
			if (udpClient == null)
				return;

			string json = JsonUtility.ToJson(state);
			byte[] data = Encoding.UTF8.GetBytes(json);
			udpClient.Send(data, data.Length, multicastEndPoint);
		}

		private void SendJoin(int playerId, Vector3 spawn)
		{
			PlayerState state = new PlayerState
			{
				messageType = "JOIN",
				playerId = playerId,
				x = spawn.x,
				y = spawn.y,
				z = spawn.z,
				sequence = 0
			};

			SendState(state);
		}

		private void ReceiveLoop()
		{
			while (isRunning)
			{
				try
				{
					IPEndPoint remoteEP = null;
					byte[] data = udpClient.Receive(ref remoteEP);
					string json = Encoding.UTF8.GetString(data);
					PlayerState state = JsonUtility.FromJson<PlayerState>(json);

					if (state == null)
						continue;

					if (state.playerId == GameManager.Instance.localPlayerId &&
						state.messageType != "ORB_SPAWN" &&
						state.messageType != "GAME_OVER")
					{
						continue;
					}

					if (state.messageType == "MOVE" || state.messageType == "JOIN")
					{
						if (state.playerId != 0)
						{
							if (!lastSequenceByPlayer.ContainsKey(state.playerId))
								lastSequenceByPlayer[state.playerId] = -1;

							if (state.messageType == "MOVE" &&
								state.sequence <= lastSequenceByPlayer[state.playerId])
							{
								continue;
							}

							lastSequenceByPlayer[state.playerId] = state.sequence;
						}
					}

					MainThreadDispatcher.Enqueue(() => ApplyState(state));
				}
				catch
				{
				}
			}
		}

		private void ApplyState(PlayerState state)
		{
			switch (state.messageType)
			{
				case "JOIN":
					GameManager.Instance.SpawnRemotePlayer(
						state.playerId,
						new Vector3(state.x, state.y, state.z));
					break;

				case "MOVE":
					GameManager.Instance.UpdateRemotePlayerPosition(
						state.playerId,
						new Vector3(state.x, state.y, state.z));
					break;

				case "SCORE":
					ScoreManager.Instance.SetRemoteScore(state.playerId, state.score);
					break;

				case "ORB_SPAWN":
					OrbSpawner.Instance.ApplyRemoteOrbSpawn(
						new Vector3(state.x, state.y, state.z));
					break;

				case "GAME_OVER":
					ScoreManager.Instance.ApplyGameOver(state.winnerId);
					break;
			}
		}
	}
}