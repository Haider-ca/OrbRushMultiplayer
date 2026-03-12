using UnityEngine;
using OrbRush.Networking;

namespace OrbRush.GameLogic
{
	// Author: Haider, Jerry(Edit)
	// Responsibility: Spawn and respawn the orb
	public class OrbSpawner : MonoBehaviour
	{
		public static OrbSpawner Instance;

		public GameObject orbPrefab;

		private GameObject _currentOrb;

		private void Awake()
		{
			Instance = this;
		}

		private void Start()
		{
			SpawnOrb(GameManager.Instance.GetRandomSpawnPosition());
		}

		private void SpawnOrb(Vector3 position)
		{
			if (_currentOrb)
				Destroy(_currentOrb);

			_currentOrb = Instantiate(orbPrefab, position, Quaternion.identity);
		}

		public bool ConsumeCurrentOrb()
		{
			if (!_currentOrb)
				return false;

			Destroy(_currentOrb);
			_currentOrb = null;
			return true;
		}

		public void RespawnOrbAndBroadcast()
		{
			Vector3 newPos = GameManager.Instance.GetRandomSpawnPosition();
			SpawnOrb(newPos);

			if (!NetworkBridge.Instance)
				return;

			PlayerState state = new PlayerState
			{
				messageType = "ORB_SPAWN",
				x = newPos.x,
				y = newPos.y,
				z = newPos.z,
				sequence = System.DateTime.UtcNow.Ticks
			};

			NetworkBridge.Instance.SendState(state);
		}

		public void ApplyRemoteOrbSpawn(Vector3 position)
		{
			SpawnOrb(position);
		}

		public bool TryGetCurrentOrbPosition(out Vector3 position)
		{
			if (!_currentOrb)
			{
				position = default;
				return false;
			}

			position = _currentOrb.transform.position;
			return true;
		}
	}
}
