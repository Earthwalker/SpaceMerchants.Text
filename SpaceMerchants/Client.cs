//-----------------------------------------------------------------------
// <copyright file="Client.cs" company="Leamware">
//     Copyright (c) Leamware. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SpaceMerchants
{
    using LiteNetLib.Utils;
    using LiteNetLib;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System;
    using System.Threading;

    /// <summary>
    /// The networking client, which implements <see cref="StreamSocketClient"/>.
    /// </summary>
    public class Client
    {
        /// <summary>
        /// First byte of every message determining how the message should be handled.
        /// </summary>
        public enum MessageId : byte
        {
            /// <summary>
            /// New player has connected.
            /// </summary>
            Connect,

            /// <summary>
            /// Player has disconnected.
            /// </summary>
            Disconnect,

            /// <summary>
            /// Server or client data.
            /// </summary>
            Data,

            /// <summary>
            /// Server or client updates.
            /// </summary>
            Update
        }

        private EventBasedNetListener listener = new EventBasedNetListener();

        private NetManager client;

        private Timer timer;

        public bool IsRunning => client?.IsRunning ?? false;

        public int MaxPlayers { get; set; } = 1;

        public string ConnectionKey { get; set; } = "SpaceMerchants";

        public Dictionary<NetPeer, Player> Players { get; private set; } = new Dictionary<NetPeer, Player>();

        /// <summary>
        /// Starts listening at the specified port.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <returns>The task.</returns>
        public Client(string ip, int port)
        {
            client = new NetManager(listener);
            client.Start();
            client.DisconnectTimeout = 10000;

            var writer = new NetDataWriter();

            writer.Put(ConnectionKey);

            // bytes for name
            writer.Put(Game.Player.Name);

            // byte for ship class
            writer.Put((byte)Game.Player.ShipClass);

            client.Connect(ip, port, writer);

            listener.NetworkReceiveEvent += OnReceiveEvent;
            listener.PeerConnectedEvent += OnConnectedEvent;
            listener.PeerDisconnectedEvent += OnDisconnectedEvent;

            timer = new Timer((obj) => Update(), null, 0, client.UpdateTime);

            if (!IsRunning)
            {
                Game.WriteLine($"Failed to connect to {ip} : {port}", MessageType.Error);
                return;
            }

            Game.WriteLine($"Attempting to connect to {ip}:{port}", MessageType.Message);
        }

        public void Stop()
        {
            client.Stop(true);

            listener.NetworkReceiveEvent -= OnReceiveEvent;
            listener.PeerConnectedEvent -= OnConnectedEvent;
            listener.PeerDisconnectedEvent -= OnDisconnectedEvent;

            timer?.Dispose();
        }

        private void OnReceiveEvent(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
        {
            // read lines the server sent
            byte lines = reader.GetByte();

            Game.WriteLine();

            // write server message
            for (int i = 0; i < lines; i++)
                Game.WriteLine(reader.GetString(), (MessageType)reader.GetByte());

            Game.Player.ReceivedReply = true;

            reader.Recycle();
        }

        private void OnConnectedEvent(NetPeer peer)
        {
            Game.WriteLine($"Client connected to {peer.EndPoint.Address}:{peer.EndPoint.Port}", MessageType.Message);
        }

        private void OnDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Game.WriteLine($"Client disconnected {disconnectInfo.Reason}", MessageType.Message);

            Game.Player.MainMenu();
        }

        public void Send(NetDataWriter writer)
        {
            client.SendToAll(writer, DeliveryMethod.ReliableOrdered);
        }

        private void Update()
        {
            client.PollEvents();
        }
    }
}