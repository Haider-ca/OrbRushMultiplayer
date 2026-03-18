namespace OrbRush.Networking
{
	// Author: Jerry
	// Responsibility: Interface for network send implementations (UDP or SignalR)
	public interface INetworkSender
	{
		void SendState(PlayerState state);
	}
}
