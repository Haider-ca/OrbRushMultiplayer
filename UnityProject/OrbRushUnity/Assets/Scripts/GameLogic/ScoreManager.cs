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

		public float roundDurationSeconds = 60f;

		private readonly Dictionary<int, int> scores = new Dictionary<int, int>();
		private float remainingTime;
		private bool roundEnded;

		private void Awake()
		{
			Instance = this;
			roundDurationSeconds = 60f;
			remainingTime = roundDurationSeconds;
		}

		private void Start()
		{
			RefreshScoreUI();
		}

		private void Update()
		{
			if (roundEnded)
				return;

			remainingTime -= Time.deltaTime;
			if (remainingTime <= 0f)
			{
				remainingTime = 0f;
				EndRound();
			}

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
			if (roundEnded)
				return;

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

		}

		public void ApplyOrbCollected(int playerId)
		{
			if (roundEnded)
				return;

			if (!scores.ContainsKey(playerId))
				scores[playerId] = 0;

			scores[playerId]++;
			RefreshScoreUI();
		}

		public void SetRemoteScore(int playerId, int score)
		{
			scores[playerId] = score;
			RefreshScoreUI();
		}

		public void ApplyGameOver(int winnerId)
		{
			roundEnded = true;
			remainingTime = 0f;
			RefreshScoreUI();
		}

		public bool IsRoundEnded()
		{
			return roundEnded;
		}

		public string GetWinnerSummary()
		{
			return GetWinnerText();
		}

		public string GetScoreboardSummary()
		{
			StringBuilder sb = new StringBuilder();

			foreach (KeyValuePair<int, int> entry in scores)
				sb.AppendLine("Player " + entry.Key + ": " + entry.Value);

			return sb.ToString().TrimEnd();
		}

		public void RestartRound()
		{
			List<int> playerIds = new List<int>(scores.Keys);

			scores.Clear();
			foreach (int playerId in playerIds)
				scores[playerId] = 0;

			remainingTime = roundDurationSeconds;
			roundEnded = false;

			OrbSpawner.Instance?.ApplyRemoteOrbSpawn(
				GameManager.Instance.GetRandomSpawnPosition());

			RefreshScoreUI();
		}

		private void RefreshScoreUI()
		{
			if (HUDController.Instance == null)
				return;

			StringBuilder sb = new StringBuilder();
			int localPlayerId = GameManager.Instance != null ? GameManager.Instance.localPlayerId : 0;
			int localScore = scores.ContainsKey(localPlayerId) ? scores[localPlayerId] : 0;

			sb.AppendLine("Time Left: " + FormatTime(remainingTime));
			sb.AppendLine("My Score: " + localScore);
			sb.AppendLine("Scores");

			foreach (var kv in scores)
			{
				sb.AppendLine("Player " + kv.Key + ": " + kv.Value);
			}

			if (roundEnded)
				sb.AppendLine(GetWinnerText());

			HUDController.Instance.SetScoreText(sb.ToString());
			HUDController.Instance.SetStatusText(string.Empty);
		}

		private void EndRound()
		{
			if (roundEnded)
				return;

			roundEnded = true;

			int winnerId = GetWinnerId();
			if (winnerId != 0 && NetworkBridge.Instance != null)
			{
				PlayerState state = new PlayerState
				{
					messageType = "GAME_OVER",
					winnerId = winnerId,
					sequence = System.DateTime.UtcNow.Ticks
				};

				NetworkBridge.Instance.SendState(state);
			}

			RefreshScoreUI();
		}

		private int GetWinnerId()
		{
			int winnerId = 0;
			int bestScore = int.MinValue;

			foreach (KeyValuePair<int, int> entry in scores)
			{
				if (entry.Value > bestScore)
				{
					bestScore = entry.Value;
					winnerId = entry.Key;
				}
			}

			return winnerId;
		}

		private string GetWinnerText()
		{
			if (scores.Count == 0)
				return "Winner: None";

			int winnerId = GetWinnerId();
			int winnerScore = scores.ContainsKey(winnerId) ? scores[winnerId] : 0;
			return "Winner: Player " + winnerId + " (" + winnerScore + ")";
		}

		private static string FormatTime(float timeSeconds)
		{
			int totalSeconds = Mathf.CeilToInt(Mathf.Max(0f, timeSeconds));
			int minutes = totalSeconds / 60;
			int seconds = totalSeconds % 60;
			return minutes.ToString("00") + ":" + seconds.ToString("00");
		}
	}
}
