using UnityEngine;

namespace OrbRush.Networking
{
	// Author: Jerry
	// Responsibility: Unified send interface that routes to either UDP or SignalR
	public class NetworkBridge : MonoBehaviour
	{
		public static NetworkBridge Instance;

		public enum NetworkMode
		{
			UDP,
			SignalR
		}

		public NetworkMode mode = NetworkMode.UDP;

		private void Awake()
		{
			Instance = this;
		}

		public void SendState(PlayerState state)
		{
			switch (mode)
			{
				case NetworkMode.UDP:
					if (UdpNetworkManager.Instance != null)
						UdpNetworkManager.Instance.SendState(state);
					break;

				case NetworkMode.SignalR:
					if (SignalRNetworkManager.Instance != null)
						SignalRNetworkManager.Instance.SendState(state);
					break;
			}
		}
	}
}
