using System.Collections.Generic;
using System.Threading;
using OrbRush.GameLogic;
using OrbRush.UI;
using OrbRush.Utilities;
using UnityEngine;

namespace OrbRush.Networking
{
	// Author: Networking Team, Jerry(Edit)
	// Responsibility: UDP multicast send/receive and basic loss recovery
	public class UdpNetworkManager : MonoBehaviour, INetworkSender
	{
		public static UdpNetworkManager Instance;

		public string multicastAddress = "230.0.0.1";
		public int port = 11000;
		public float joinBroadcastIntervalSeconds = 2f;
		public float scoreBroadcastIntervalSeconds = 2f;
		public float orbBroadcastIntervalSeconds = 2f;
		public float gameOverBroadcastIntervalSeconds = 1f;
		public float remotePlayerTimeoutSeconds = 8f;

		private MulticastSocket multicastSocket;
		private Thread receiveThread;
		private bool isRunning;
		private bool leaveSent;
		private float nextJoinBroadcastTime;
		private float nextScoreBroadcastTime;
		private float nextOrbBroadcastTime;
		private float nextGameOverBroadcastTime;

		private readonly Dictionary<int, long> lastMoveSequenceByPlayer = new Dictionary<int, long>();
		private readonly Dictionary<int, long> lastMembershipSequenceByPlayer = new Dictionary<int, long>();
		private readonly Dictionary<int, long> lastScoreSequenceByPlayer = new Dictionary<int, long>();
		private readonly Dictionary<int, float> lastSeenTimeByPlayer = new Dictionary<int, float>();
		private readonly HashSet<long> processedOrbCollectSequences = new HashSet<long>();
		private long lastOrbSpawnSequence = -1;
		private long lastGameOverSequence = -1;

		private void Awake()
		{
			Instance = this;
		}

		private void Start()
		{
			if (NetworkBridge.Instance != null)
				NetworkBridge.Instance.RegisterSender(this);

			int localPlayerId = Random.Range(1000, 9999);
			Vector3 spawn = new Vector3(Random.Range(-6f, 6f), 5f, Random.Range(-6f, 6f));

			GameManager.Instance.SpawnLocalPlayer(localPlayerId, spawn);

			if (HUDController.Instance != null)
				HUDController.Instance.SetStatusText("Local Player ID: " + localPlayerId);

			multicastSocket = new MulticastSocket(multicastAddress, port);
			multicastSocket.MsgReceived += OnMessageReceived;
			multicastSocket.Start();

			isRunning = true;
			receiveThread = new Thread(multicastSocket.ReceiveMessages) { IsBackground = true };
			receiveThread.Start();

			SendJoin(localPlayerId, spawn);
			nextJoinBroadcastTime = Time.time + joinBroadcastIntervalSeconds;
			nextScoreBroadcastTime = Time.time + scoreBroadcastIntervalSeconds;
			nextOrbBroadcastTime = Time.time + orbBroadcastIntervalSeconds;
			nextGameOverBroadcastTime = Time.time + gameOverBroadcastIntervalSeconds;
		}

		private void Update()
		{
			if (!isRunning)
			{
				CleanupTimedOutRemotePlayers();
				return;
			}

			if (joinBroadcastIntervalSeconds > 0f && Time.time >= nextJoinBroadcastTime)
			{
				SendJoin(GameManager.Instance.localPlayerId, GetLocalPlayerPosition());
				nextJoinBroadcastTime = Time.time + joinBroadcastIntervalSeconds;
			}

			if (scoreBroadcastIntervalSeconds > 0f && Time.time >= nextScoreBroadcastTime)
			{
				BroadcastLocalScoreSnapshot();
				nextScoreBroadcastTime = Time.time + scoreBroadcastIntervalSeconds;
			}

			if (orbBroadcastIntervalSeconds > 0f && Time.time >= nextOrbBroadcastTime)
			{
				BroadcastOrbSnapshot();
				nextOrbBroadcastTime = Time.time + orbBroadcastIntervalSeconds;
			}

			if (gameOverBroadcastIntervalSeconds > 0f && Time.time >= nextGameOverBroadcastTime)
			{
				BroadcastGameOverSnapshot();
				nextGameOverBroadcastTime = Time.time + gameOverBroadcastIntervalSeconds;
			}

			CleanupTimedOutRemotePlayers();
		}

		private void OnApplicationQuit()
		{
			ShutdownNetwork(true);
		}

		private void OnDestroy()
		{
			ShutdownNetwork(!leaveSent);
		}

		private void ShutdownNetwork(bool sendLeave)
		{
			if (sendLeave)
				SendLeave();

			isRunning = false;
			multicastSocket?.Stop();
		}

		public void SendState(PlayerState state)
		{
			if (multicastSocket == null)
				return;

			string json = JsonUtility.ToJson(state);
			multicastSocket.SendMessage(json);
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
				sequence = System.DateTime.UtcNow.Ticks
			};

			SendState(state);
		}

		private void SendLeave()
		{
			if (leaveSent || multicastSocket == null || GameManager.Instance == null || GameManager.Instance.localPlayerId == 0)
				return;

			PlayerState state = new PlayerState
			{
				messageType = "LEAVE",
				playerId = GameManager.Instance.localPlayerId,
				sequence = System.DateTime.UtcNow.Ticks
			};

			leaveSent = true;
			SendState(state);
		}

		private void BroadcastLocalScoreSnapshot()
		{
			if (ScoreManager.Instance == null || GameManager.Instance == null)
				return;

			PlayerState state = new PlayerState
			{
				messageType = "SCORE",
				playerId = GameManager.Instance.localPlayerId,
				score = ScoreManager.Instance.GetScore(GameManager.Instance.localPlayerId),
				sequence = System.DateTime.UtcNow.Ticks
			};

			SendState(state);
		}

		private void BroadcastOrbSnapshot()
		{
			if (OrbSpawner.Instance == null || !OrbSpawner.Instance.TryGetCurrentOrbPosition(out Vector3 orbPosition))
				return;

			PlayerState state = new PlayerState
			{
				messageType = "ORB_SPAWN",
				x = orbPosition.x,
				y = orbPosition.y,
				z = orbPosition.z,
				sequence = System.DateTime.UtcNow.Ticks
			};

			SendState(state);
		}

		private void BroadcastGameOverSnapshot()
		{
			if (ScoreManager.Instance == null || !ScoreManager.Instance.TryGetGameOverWinner(out int winnerId))
				return;

			PlayerState state = new PlayerState
			{
				messageType = "GAME_OVER",
				winnerId = winnerId,
				sequence = System.DateTime.UtcNow.Ticks
			};

			SendState(state);
		}

		private Vector3 GetLocalPlayerPosition()
		{
			PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
			foreach (PlayerController controller in players)
			{
				if (controller.isLocalPlayer)
					return controller.transform.position;
			}

			return Vector3.zero;
		}

		private void OnMessageReceived(string json)
		{
			PlayerState state = JsonUtility.FromJson<PlayerState>(json);

			if (state == null)
				return;

			if (state.playerId == GameManager.Instance.localPlayerId &&
				state.messageType != "ORB_SPAWN" &&
				state.messageType != "GAME_OVER")
			{
				return;
			}

			if (ShouldIgnoreState(state))
				return;

			if (state.messageType == "ORB_COLLECT")
			{
				if (processedOrbCollectSequences.Contains(state.sequence))
					return;

				processedOrbCollectSequences.Add(state.sequence);
			}

			MainThreadDispatcher.Enqueue(() => ApplyState(state));
		}

		private bool ShouldIgnoreState(PlayerState state)
		{
			switch (state.messageType)
			{
				case "MOVE":
					return !TryTrackLatestSequence(lastMoveSequenceByPlayer, state.playerId, state.sequence, true);

				case "JOIN":
				case "LEAVE":
					return !TryTrackLatestSequence(lastMembershipSequenceByPlayer, state.playerId, state.sequence, true);

				case "SCORE":
					return !TryTrackLatestSequence(lastScoreSequenceByPlayer, state.playerId, state.sequence, true);

				case "ORB_SPAWN":
					if (state.sequence <= lastOrbSpawnSequence)
						return true;

					lastOrbSpawnSequence = state.sequence;
					return false;

				case "GAME_OVER":
					if (state.sequence <= lastGameOverSequence)
						return true;

					lastGameOverSequence = state.sequence;
					return false;

				default:
					return false;
			}
		}

		private bool TryTrackLatestSequence(Dictionary<int, long> sequenceMap, int key, long sequence, bool rejectOlderOrDuplicate)
		{
			if (key == 0)
				return true;

			if (!sequenceMap.ContainsKey(key))
			{
				sequenceMap[key] = sequence;
				return true;
			}

			if (rejectOlderOrDuplicate && sequence <= sequenceMap[key])
				return false;

			sequenceMap[key] = sequence;
			return true;
		}

		private void CleanupTimedOutRemotePlayers()
		{
			if (remotePlayerTimeoutSeconds <= 0f || GameManager.Instance == null)
				return;

			List<int> timedOutPlayers = null;
			foreach (KeyValuePair<int, float> entry in lastSeenTimeByPlayer)
			{
				if (Time.time - entry.Value <= remotePlayerTimeoutSeconds)
					continue;

				timedOutPlayers ??= new List<int>();
				timedOutPlayers.Add(entry.Key);
			}

			if (timedOutPlayers == null)
				return;

			foreach (int playerId in timedOutPlayers)
				RemoveRemotePlayerState(playerId);
		}

		private void RemoveRemotePlayerState(int playerId)
		{
			if (GameManager.Instance == null)
				return;

			lastSeenTimeByPlayer.Remove(playerId);
			lastMoveSequenceByPlayer.Remove(playerId);
			lastMembershipSequenceByPlayer.Remove(playerId);
			lastScoreSequenceByPlayer.Remove(playerId);
			GameManager.Instance.RemoveRemotePlayer(playerId);
		}

		private void TrackRemotePlayerHeartbeat(PlayerState state)
		{
			if (state.playerId == 0 || state.playerId == GameManager.Instance.localPlayerId)
				return;

			switch (state.messageType)
			{
				case "JOIN":
				case "MOVE":
				case "SCORE":
					lastSeenTimeByPlayer[state.playerId] = Time.time;
					break;
			}
		}

		private void ApplyState(PlayerState state)
		{
			TrackRemotePlayerHeartbeat(state);

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

				case "ORB_COLLECT":
					OrbSpawner.Instance.ApplyRemoteOrbSpawn(
						new Vector3(state.x, state.y, state.z));
					ScoreManager.Instance.ApplyOrbCollected(state.playerId);
					break;

				case "GAME_OVER":
					ScoreManager.Instance.ApplyGameOver(state.winnerId);
					break;

				case "LEAVE":
					RemoveRemotePlayerState(state.playerId);
					break;
			}
		}
	}
}
