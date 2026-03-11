using UnityEngine;
using OrbRush.Networking;

namespace OrbRush.GameLogic
{
	// Author: Haider
	// Responsibility: Detect orb collection by the local player
	public class OrbCollector : MonoBehaviour
	{
		private void OnTriggerEnter(Collider other)
		{
			PlayerController controller = GetComponent<PlayerController>();

			if (controller == null || !controller.isLocalPlayer)
				return;

			if (other.CompareTag("Orb"))
			{
				if (OrbSpawner.Instance == null || !OrbSpawner.Instance.ConsumeCurrentOrb())
					return;

				Vector3 newOrbPosition = GameManager.Instance.GetRandomSpawnPosition();
				OrbSpawner.Instance.ApplyRemoteOrbSpawn(newOrbPosition);
				ScoreManager.Instance?.ApplyOrbCollected(controller.playerId);

				PlayerState state = new PlayerState
				{
					messageType = "ORB_COLLECT",
					playerId = controller.playerId,
					x = newOrbPosition.x,
					y = newOrbPosition.y,
					z = newOrbPosition.z,
					sequence = System.DateTime.UtcNow.Ticks
				};

				NetworkBridge.Instance?.SendState(state);
			}
		}
	}
}
