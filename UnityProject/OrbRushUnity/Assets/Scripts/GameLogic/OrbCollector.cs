using UnityEngine;

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
				ScoreManager.Instance.AddScore(controller.playerId);
				OrbSpawner.Instance.RespawnOrbAndBroadcast();
			}
		}
	}
}