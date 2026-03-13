using System.Collections.Generic;
using System.Text;
using UnityEngine;
using OrbRush.Networking;
using OrbRush.UI;

namespace OrbRush.GameLogic
{
    // Author: Haider, Jerry(Edit)
    // Responsibility: Track scores, round timer, and game-over state
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance;

        public float roundDurationSeconds = 60f;

        private readonly Dictionary<int, int> _scores = new();
        private float _remainingTime;
        private bool _roundEnded;
        private int _currentWinnerId;

        private void Awake()
        {
            Instance = this;
            roundDurationSeconds = 60f;
            _remainingTime = roundDurationSeconds;
        }

        private void Start()
        {
            RefreshScoreUI();
        }

        private void Update()
        {
            if (_roundEnded)
                return;

            _remainingTime -= Time.deltaTime;
            if (_remainingTime <= 0f)
            {
                _remainingTime = 0f;
                EndRound();
            }

            RefreshScoreUI();
        }

        public void RegisterPlayer(int playerId)
        {
            _scores.TryAdd(playerId, 0);

            RefreshScoreUI();
        }

        public void RemovePlayer(int playerId)
        {
            if (!_scores.Remove(playerId))
                return;
            RefreshScoreUI();
        }

        public void AddScore(int playerId)
        {
            if (_roundEnded)
                return;

            _scores.TryAdd(playerId, 0);

            _scores[playerId]++;
            BroadcastScore(playerId);
        }

        public void ApplyOrbCollected(int playerId)
        {
            if (_roundEnded)
                return;

            _scores.TryAdd(playerId, 0);

            _scores[playerId]++;
            RefreshScoreUI();
        }

        public void DeductScore(int playerId, int amount = 1)
        {
            if (_roundEnded || amount <= 0)
                return;

            _scores.TryAdd(playerId, 0);
            _scores[playerId] -= amount;
            BroadcastScore(playerId);
        }

        public void SetRemoteScore(int playerId, int score)
        {
            _scores[playerId] = score;
            RefreshScoreUI();
        }

        public int GetScore(int playerId)
        {
            return _scores.GetValueOrDefault(playerId);
        }

        public void ApplyGameOver(int winnerId)
        {
            _roundEnded = true;
            _remainingTime = 0f;
            _currentWinnerId = winnerId;
            RefreshScoreUI();
        }

        public bool TryGetGameOverWinner(out int winnerId)
        {
            winnerId = _currentWinnerId;
            return _roundEnded;
        }

        public bool IsRoundEnded()
        {
            return _roundEnded;
        }

        public string GetWinnerSummary()
        {
            return GetWinnerText();
        }

        public string GetScoreboardSummary()
        {
            StringBuilder sb = new StringBuilder();

            foreach (KeyValuePair<int, int> entry in _scores)
                sb.AppendLine("Player " + entry.Key + ": " + entry.Value);

            return sb.ToString().TrimEnd();
        }

        public void RestartRound()
        {
            List<int> playerIds = new List<int>(_scores.Keys);

            _scores.Clear();
            foreach (int playerId in playerIds)
                _scores[playerId] = 0;

            _remainingTime = roundDurationSeconds;
            _roundEnded = false;
            _currentWinnerId = 0;

            OrbSpawner.Instance?.ApplyRemoteOrbSpawn(GameManager.Instance.GetRandomSpawnPosition());

            RefreshScoreUI();
        }

        private void RefreshScoreUI()
        {
            if (!HUDController.Instance)
                return;

            StringBuilder sb = new StringBuilder();
            int localPlayerId = GameManager.Instance ? GameManager.Instance.localPlayerId : 0;
            int localScore = _scores.GetValueOrDefault(localPlayerId);

            sb.AppendLine("Time Left: " + FormatTime(_remainingTime));
            sb.AppendLine("My Score: " + localScore);
            sb.AppendLine("Scores");

            foreach (KeyValuePair<int, int> entry in _scores)
                sb.AppendLine("Player " + entry.Key + ": " + entry.Value);

            if (_roundEnded)
                sb.AppendLine(GetWinnerText());

            HUDController.Instance.SetScoreText(sb.ToString());
            HUDController.Instance.SetStatusText(string.Empty);
        }

        private void EndRound()
        {
            if (_roundEnded)
                return;

            _roundEnded = true;
            _currentWinnerId = GetWinnerId();

            if (_currentWinnerId != 0 && NetworkBridge.Instance)
            {
                PlayerState state = new PlayerState
                {
                    messageType = "GAME_OVER",
                    winnerId = _currentWinnerId,
                    sequence = System.DateTime.UtcNow.Ticks
                };

                NetworkBridge.Instance.SendState(state);
            }

            RefreshScoreUI();
        }

        private void BroadcastScore(int playerId)
        {
            RefreshScoreUI();

            if (!NetworkBridge.Instance)
                return;

            PlayerState state = new PlayerState
            {
                messageType = "SCORE",
                playerId = playerId,
                score = _scores[playerId],
                sequence = System.DateTime.UtcNow.Ticks
            };

            NetworkBridge.Instance.SendState(state);
        }

        private int GetWinnerId()
        {
            int winnerId = 0;
            int bestScore = int.MinValue;

            foreach (KeyValuePair<int, int> entry in _scores)
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
            if (_scores.Count == 0 || _currentWinnerId == 0)
                return "Winner: None";

            int winnerScore = _scores.GetValueOrDefault(_currentWinnerId);
            return "Winner: Player " + _currentWinnerId + " (" + winnerScore + ")";
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
