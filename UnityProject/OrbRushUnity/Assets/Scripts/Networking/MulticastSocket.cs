using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace OrbRush.Networking
{
	// Author: Jerry
	// Responsibility: Low-level multicast socket using raw Socket, MulticastOption,
	//                 and delegate/event pattern per instructor design requirements
	public class MulticastSocket
	{
		public delegate void MsgHandler(string message);
		public event MsgHandler MsgReceived;

		private readonly string multicastAddress;
		private readonly int port;
		private Socket sendSocket;
		private Socket receiveSocket;
		private volatile bool isRunning;

		public MulticastSocket(string multicastAddress, int port)
		{
			this.multicastAddress = multicastAddress;
			this.port = port;
		}

		public void Start()
		{
			isRunning = true;

			sendSocket = new Socket(AddressFamily.InterNetwork,
				SocketType.Dgram, ProtocolType.Udp);

			receiveSocket = new Socket(AddressFamily.InterNetwork,
				SocketType.Dgram, ProtocolType.Udp);

			EndPoint localEP = new IPEndPoint(IPAddress.Any, port);
			receiveSocket.SetSocketOption(SocketOptionLevel.Socket,
				SocketOptionName.ReuseAddress, 1);
			receiveSocket.Bind(localEP);

			MulticastOption mcastOption = new MulticastOption(
				IPAddress.Parse(multicastAddress), IPAddress.Any);
			receiveSocket.SetSocketOption(SocketOptionLevel.IP,
				SocketOptionName.AddMembership, mcastOption);
		}

		public void SendMessage(string message)
		{
			if (sendSocket == null)
				return;

			try
			{
				IPEndPoint endPoint = new IPEndPoint(
					IPAddress.Parse(multicastAddress), port);
				byte[] data = Encoding.UTF8.GetBytes(message);
				sendSocket.SendTo(data, endPoint);
			}
			catch (Exception)
			{
			}
		}

		public void ReceiveMessages()
		{
			byte[] buffer = new byte[4096];
			EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

			while (isRunning)
			{
				try
				{
					int bytesRead = receiveSocket.ReceiveFrom(buffer, ref remoteEP);
					string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
					MsgReceived?.Invoke(message);
				}
				catch (Exception)
				{
					if (!isRunning)
						break;
				}
			}
		}

		public void Stop()
		{
			isRunning = false;

			try
			{
				sendSocket?.Close();
			}
			catch (Exception)
			{
			}

			try
			{
				receiveSocket?.Close();
			}
			catch (Exception)
			{
			}
		}
	}
}
