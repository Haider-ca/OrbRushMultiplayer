using UnityEngine;
using OrbRush.Networking;

namespace OrbRush.GameLogic
{
	// Author: Haider
	// Responsibility: Spawn and respawn the orb
	public class OrbSpawner : MonoBehaviour
	{
		public static OrbSpawner Instance;

		public GameObject orbPrefab;

		private GameObject currentOrb;

		private void Awake()
		{
			Instance = this;
		}

		private void Start()
		{
			SpawnOrb(GameManager.Instance.GetRandomSpawnPosition());
		}

		public void SpawnOrb(Vector3 position)
		{
			if (currentOrb != null)
				Destroy(currentOrb);

			currentOrb = Instantiate(orbPrefab, position, Quaternion.identity);
		}

		public void RespawnOrbAndBroadcast()
		{
			Vector3 newPos = GameManager.Instance.GetRandomSpawnPosition();
			SpawnOrb(newPos);

			if (UdpNetworkManager.Instance != null)
			{
				PlayerState state = new PlayerState
				{
					messageType = "ORB_SPAWN",
					x = newPos.x,
					y = newPos.y,
					z = newPos.z,
					sequence = System.DateTime.UtcNow.Ticks
				};

				UdpNetworkManager.Instance.SendState(state);
			}
		}

		public void ApplyRemoteOrbSpawn(Vector3 position)
		{
			SpawnOrb(position);
		}
	}
}