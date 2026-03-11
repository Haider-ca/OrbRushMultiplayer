using System.Collections.Generic;
using System.Text;
using UnityEngine;
using OrbRush.Networking;
using OrbRush.UI;

namespace OrbRush.GameLogic
{
	// Author: Haider, Jerry(Edit)
	// Responsibility: Track scores and game-over state
	public class ScoreManager : MonoBehaviour
	{
		public static ScoreManager Instance;

		public int winScore = 5;

		private readonly Dictionary<int, int> scores = new Dictionary<int, int>();

		private void Awake()
		{
			Instance = this;
		}

		private void Start()
		{
			UpdateStatusText();
			RefreshScoreUI();
		}

		public void RegisterPlayer(int playerId)
		{
			if (!scores.ContainsKey(playerId))
				scores[playerId] = 0;

			RefreshScoreUI();
		}

		public void RemovePlayer(int playerId)
		{
			if (!scores.ContainsKey(playerId))
				return;

			scores.Remove(playerId);
			RefreshScoreUI();
		}

		public void AddScore(int playerId)
		{
			if (!scores.ContainsKey(playerId))
				scores[playerId] = 0;

			scores[playerId]++;

			RefreshScoreUI();

			if (NetworkBridge.Instance != null)
			{
				PlayerState state = new PlayerState
				{
					messageType = "SCORE",
					playerId = playerId,
					score = scores[playerId],
					sequence = System.DateTime.UtcNow.Ticks
				};

				NetworkBridge.Instance.SendState(state);
			}

			if (scores[playerId] >= winScore)
			{
				if (HUDController.Instance != null)
					HUDController.Instance.SetStatusText("Player " + playerId + " wins!");

				if (NetworkBridge.Instance != null)
				{
					PlayerState state = new PlayerState
					{
						messageType = "GAME_OVER",
						winnerId = playerId,
						sequence = System.DateTime.UtcNow.Ticks
					};

					NetworkBridge.Instance.SendState(state);
				}
			}
			else
			{
				UpdateStatusText();
			}
		}

		public void ApplyOrbCollected(int playerId, Vector3 newOrbPosition)
		{
			if (!scores.ContainsKey(playerId))
				scores[playerId] = 0;

			scores[playerId]++;
			RefreshScoreUI();
			OrbSpawner.Instance?.ApplyRemoteOrbSpawn(newOrbPosition);

			if (scores[playerId] >= winScore)
			{
				if (HUDController.Instance != null)
					HUDController.Instance.SetStatusText("Player " + playerId + " wins!");

				if (playerId == GameManager.Instance.localPlayerId && NetworkBridge.Instance != null)
				{
					PlayerState state = new PlayerState
					{
						messageType = "GAME_OVER",
						winnerId = playerId,
						sequence = System.DateTime.UtcNow.Ticks
					};

					NetworkBridge.Instance.SendState(state);
				}
			}
			else
			{
				UpdateStatusText();
			}
		}

		public void SetRemoteScore(int playerId, int score)
		{
			scores[playerId] = score;
			RefreshScoreUI();
			UpdateStatusText();
		}

		public void ApplyGameOver(int winnerId)
		{
			if (HUDController.Instance != null)
				HUDController.Instance.SetStatusText("Player " + winnerId + " wins!");
		}

		private void RefreshScoreUI()
		{
			if (HUDController.Instance == null)
				return;

			StringBuilder sb = new StringBuilder();
			int localPlayerId = GameManager.Instance != null ? GameManager.Instance.localPlayerId : 0;
			int localScore = scores.ContainsKey(localPlayerId) ? scores[localPlayerId] : 0;

			sb.AppendLine("First to " + winScore + " points wins");
			sb.AppendLine("My Score: " + localScore);
			sb.AppendLine("Scores");

			foreach (var kv in scores)
			{
				sb.AppendLine("Player " + kv.Key + ": " + kv.Value);
			}

			HUDController.Instance.SetScoreText(sb.ToString());
		}

		private void UpdateStatusText()
		{
			if (HUDController.Instance == null)
				return;

			HUDController.Instance.SetStatusText("Game ends when a player reaches " + winScore + " points");
		}
	}
}
