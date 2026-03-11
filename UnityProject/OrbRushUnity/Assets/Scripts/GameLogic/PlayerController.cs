using UnityEngine;
using OrbRush.Networking;

namespace OrbRush.GameLogic
{
	// Author: Haider
	// Responsibility: Local player movement and sending movement updates
	public class PlayerController : MonoBehaviour
	{
		public int playerId;
		public bool isLocalPlayer;
		public float moveSpeed = 5f;

		private long sequenceNumber = 0;
		private Vector3 lastSentPosition;

		private void Start()
		{
			lastSentPosition = transform.position;
		}

		private void Update()
		{
			if (!isLocalPlayer)
				return;

			float h = Input.GetAxis("Horizontal");
			float v = Input.GetAxis("Vertical");

			Vector3 move = new Vector3(h, 0f, v);
			transform.Translate(move * moveSpeed * Time.deltaTime, Space.World);

			if (Vector3.Distance(transform.position, lastSentPosition) > 0.02f)
			{
				SendMove();
				lastSentPosition = transform.position;
			}
		}

		private void SendMove()
		{
			if (UdpNetworkManager.Instance == null)
				return;

			PlayerState state = new PlayerState
			{
				messageType = "MOVE",
				playerId = playerId,
				x = transform.position.x,
				y = transform.position.y,
				z = transform.position.z,
				sequence = sequenceNumber++
			};

			UdpNetworkManager.Instance.SendState(state);
		}
	}
}