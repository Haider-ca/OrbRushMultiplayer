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

		private INetworkSender activeSender;

		private void Awake()
		{
			Instance = this;
		}

		private void Start()
		{
			// Disable the network manager that is NOT selected
			GameObject udpObj = GameObject.Find("UdpNetworkManager");
			GameObject signalRObj = GameObject.Find("SignalRNetworkManager");

			if (udpObj)
				udpObj.SetActive(mode == NetworkMode.UDP);

			if (signalRObj)
				signalRObj.SetActive(mode == NetworkMode.SignalR);
		}

		public void RegisterSender(INetworkSender sender)
		{
			activeSender = sender;
		}

		public void SendState(PlayerState state)
		{
			activeSender?.SendState(state);
		}
	}
}
