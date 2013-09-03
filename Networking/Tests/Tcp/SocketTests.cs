﻿using System.Threading;
using DeltaEngine.Datatypes;
using DeltaEngine.Networking.Messages;
using DeltaEngine.Networking.Tcp;
using NUnit.Framework;

namespace DeltaEngine.Networking.Tests.Tcp
{
	public class SocketTests
	{
		[SetUp]
		public void CreateLocalEchoServer()
		{
			serverPort++;
			serverReceivedMessageCount = 0;
			clientReceivedResponseCount = 0;
			echoServer = new TcpServer();
			echoServer.Start(serverPort);
			echoServer.ClientDataReceived +=
				(connection, data) => connection.Send(new TextMessage("TestMessage"));
		}

		private int serverReceivedMessageCount;
		private int clientReceivedResponseCount;
		private TcpServer echoServer;
		private static int serverPort = 8585;

		[TearDown]
		public void DisposeConnectionsSockets()
		{
			echoServer.Dispose();
		}

		[Test, Category("Slow")]
		public void ConnectAndDisposeClientConnection()
		{
			echoServer.ClientConnected +=
				connection => Assert.AreEqual(1, echoServer.NumberOfConnectedClients);
			echoServer.ClientDisconnected +=
				connection => Assert.AreEqual(0, echoServer.NumberOfConnectedClients);
			var client = CreatedConnectedClient();
			bool clientDisconnected = false;
			client.Disconnected += () => clientDisconnected = true;
			WaitForServerResponse();
			Assert.AreEqual(ServerAddress + ":" + echoServer.ListenPort, client.TargetAddress);
			client.Dispose();
			Assert.IsTrue(clientDisconnected);
		}

		private const string ServerAddress = "127.0.0.1";

		private static Client CreatedConnectedClient()
		{
			var client = new TcpSocket();
			client.Connect(ServerAddress, serverPort);
			return client;
		}

		private static void WaitForServerResponse(int milliseconds = 10)
		{
			Thread.Sleep(milliseconds);
		}

		[Test, Category("Slow")]
		public void ConnectClientAndDisposeServer()
		{
			var client = CreatedConnectedClient();
			Thread.Sleep(1);
			echoServer.ClientConnected += connection => Assert.IsTrue(client.IsConnected);
			ShutDownServer();
		}

		private void ShutDownServer()
		{
			echoServer.Dispose();
			WaitForServerResponse();
		}

		[Test, Category("Slow")]
		public void ConnectClientWithoutServer()
		{
			ShutDownServer();
			using (var client = CreatedConnectedClient())
			{
				client.Send(new Color(3, 3, 3));
				Assert.IsFalse(client.IsConnected);
			}
		}

		[Test, Category("Slow")]
		public void ConnectionWithServerShouldWork()
		{
			using (var client = CreatedConnectedClient())
			{
				WaitForServerResponse();
				Assert.IsTrue(client.IsConnected);
				Assert.IsTrue(echoServer.IsRunning);
			}
		}

		[Test, Category("Slow")]
		public void SendSingleMessageToServer()
		{
			using (var client = CreatedConnectedClient())
			{
				var message = new TextMessage("TestMessage");
				echoServer.ClientDataReceived += (c, m) => Assert.IsTrue(m is TextMessage);
				client.Send(message);
			}
		}

		[Test, Category("Slow")]
		public void SendMessagesToServer()
		{
			using (var client = CreatedConnectedClient())
			{
				client.DataReceived += message => clientReceivedResponseCount++;
				echoServer.ClientDataReceived += (connection, data) => serverReceivedMessageCount++;
				WaitForServerResponse();
				Assert.AreEqual(1, echoServer.NumberOfConnectedClients);
				for (int messageIndex = 1; messageIndex <= 5; messageIndex++)
					SendTestMessageAndCheckMessageCount(client, messageIndex);
			}
		}

		private void SendTestMessageAndCheckMessageCount(Client client, int expectedMessageCount)
		{
			client.Send(new TextMessage("TestMessage"));
			WaitForServerResponse();
			Assert.AreEqual(1, echoServer.NumberOfConnectedClients);
			Assert.AreEqual(expectedMessageCount, serverReceivedMessageCount);
			Assert.AreEqual(expectedMessageCount, clientReceivedResponseCount);
		}

		[Test, Category("Slow")]
		public void SendMalformedMessageToServer()
		{
			using (var client = CreatedConnectedClient())
			{
				client.Dispose();
				client.Send(new TextMessage("TestMessage"));
			}
		}

		[Test, Category("Slow")]
		public void ConnectToUnknownHostShouldTimeOut()
		{
			using (var client = new TcpSocket())
			{
				bool isTimedOut = false;
				client.TimedOut += () => isTimedOut = true;
				client.Connect(ServerAddress, 12345);
				Assert.IsFalse(isTimedOut);
				Thread.Sleep(3500);
				Assert.IsTrue(isTimedOut);
			}
		}
	}
}