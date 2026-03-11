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
		}

		public void SetRemoteScore(int playerId, int score)
		{
			scores[playerId] = score;
			RefreshScoreUI();
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
			sb.AppendLine("Scores");

			foreach (var kv in scores)
			{
				sb.AppendLine("Player " + kv.Key + ": " + kv.Value);
			}

			HUDController.Instance.SetScoreText(sb.ToString());
		}
	}
}