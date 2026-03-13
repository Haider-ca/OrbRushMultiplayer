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

        private INetworkSender _activeSender;

        private void Awake()
        {
            Instance = this;
            ApplyNetworkMode();
        }

        private void Start()
        {
            ApplyNetworkMode();
        }

        public void RegisterSender(INetworkSender sender)
        {
            _activeSender = sender;
        }

        public void SendState(PlayerState state)
        {
            _activeSender?.SendState(state);
        }

        private void ApplyNetworkMode()
        {
            // Disable the unused network manager before its Start can spawn another local player.
            GameObject udpObj = GameObject.Find("UdpNetworkManager");
            GameObject signalRObj = GameObject.Find("SignalRNetworkManager");

            if (udpObj)
                udpObj.SetActive(mode == NetworkMode.UDP);

            if (signalRObj)
                signalRObj.SetActive(mode == NetworkMode.SignalR);
        }
    }
}
