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

			// Disable the network manager that is NOT selected
			if (UdpNetworkManager.Instance != null)
				UdpNetworkManager.Instance.gameObject.SetActive(mode == NetworkMode.UDP);

			if (SignalRNetworkManager.Instance != null)
				SignalRNetworkManager.Instance.gameObject.SetActive(mode == NetworkMode.SignalR);
		}

		private void Start()
		{
			// Also check after all Awake() calls have finished
			GameObject udpObj = GameObject.Find("UdpNetworkManager");
			GameObject signalRObj = GameObject.Find("SignalRNetworkManager");

			if (udpObj != null)
				udpObj.SetActive(mode == NetworkMode.UDP);

			if (signalRObj != null)
				signalRObj.SetActive(mode == NetworkMode.SignalR);
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
