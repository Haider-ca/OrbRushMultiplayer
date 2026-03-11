using UnityEngine;
using OrbRush.Networking;

namespace OrbRush.GameLogic
{
	// Author: Haider, Jerry(Edit)
	// Responsibility: Local player movement and sending movement updates
	public class PlayerController : MonoBehaviour
	{
		public int playerId;
		public bool isLocalPlayer;
		public float moveSpeed = 5f;
		public float playfieldMinX = -9.5f;
		public float playfieldMaxX = 9.5f;
		public float playfieldMinZ = -9.5f;
		public float playfieldMaxZ = 9.5f;

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

			if (ScoreManager.Instance != null && ScoreManager.Instance.IsRoundEnded())
				return;

			float h = Input.GetAxis("Horizontal");
			float v = Input.GetAxis("Vertical");

			Vector3 move = new Vector3(h, 0f, v);
			transform.Translate(move * moveSpeed * Time.deltaTime, Space.World);
			WrapPosition();

			if (Vector3.Distance(transform.position, lastSentPosition) > 0.02f)
			{
				SendMove();
				lastSentPosition = transform.position;
			}
		}

		private void SendMove()
		{
			if (NetworkBridge.Instance == null)
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

			NetworkBridge.Instance.SendState(state);
		}

		private void WrapPosition()
		{
			Vector3 position = transform.position;

			if (position.x < playfieldMinX)
				position.x = playfieldMaxX;
			else if (position.x > playfieldMaxX)
				position.x = playfieldMinX;

			if (position.z < playfieldMinZ)
				position.z = playfieldMaxZ;
			else if (position.z > playfieldMaxZ)
				position.z = playfieldMinZ;

			transform.position = position;
		}
	}
}
