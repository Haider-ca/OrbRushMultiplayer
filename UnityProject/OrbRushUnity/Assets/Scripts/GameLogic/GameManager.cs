using System.Collections.Generic;
using UnityEngine;

namespace OrbRush.GameLogic
{
	// Author: Haider
	// Responsibility: Spawning and tracking local/remote players
	public class GameManager : MonoBehaviour
	{
		public static GameManager Instance;

		public GameObject localPlayerPrefab;
		public GameObject remotePlayerPrefab;

		public int localPlayerId;

		private readonly Dictionary<int, GameObject> players = new Dictionary<int, GameObject>();

		private void Awake()
		{
			Instance = this;
		}

		public void SpawnLocalPlayer(int playerId, Vector3 position)
		{
			localPlayerId = playerId;

			if (players.ContainsKey(playerId))
				return;

			GameObject player = Instantiate(localPlayerPrefab, position, Quaternion.identity);
			PlayerController controller = player.GetComponent<PlayerController>();
			controller.playerId = playerId;
			controller.isLocalPlayer = true;

			players[playerId] = player;
		}

		public void SpawnRemotePlayer(int playerId, Vector3 position)
		{
			if (players.ContainsKey(playerId))
				return;

			GameObject player = Instantiate(remotePlayerPrefab, position, Quaternion.identity);
			PlayerController controller = player.GetComponent<PlayerController>();
			controller.playerId = playerId;
			controller.isLocalPlayer = false;

			players[playerId] = player;
		}

		public void UpdateRemotePlayerPosition(int playerId, Vector3 position)
		{
			if (playerId == localPlayerId)
				return;

			if (!players.ContainsKey(playerId))
				SpawnRemotePlayer(playerId, position);

			players[playerId].transform.position = position;
		}

		public void RemoveRemotePlayer(int playerId)
		{
			if (playerId == localPlayerId)
				return;

			if (!players.TryGetValue(playerId, out GameObject player))
				return;

			players.Remove(playerId);
			Destroy(player);
		}

		public Vector3 GetRandomSpawnPosition()
		{
			return new Vector3(Random.Range(-6f, 6f), 5f, Random.Range(-6f, 6f));
		}
	}
}
