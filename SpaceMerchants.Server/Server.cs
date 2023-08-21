//-----------------------------------------------------------------------
// <copyright file="Server.cs" company="Leamware">
//     Copyright (c) Leamware. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SpaceMerchants.Server
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using LiteNetLib;
    using LiteNetLib.Utils;

    /// <summary>
    /// The networking server.
    /// </summary>
    public class Server
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

        private NetManager server;

        private Timer timer;

        private Player connectingPlayer;

        public bool IsRunning => server?.IsRunning ?? false;

        public int MaxPlayers { get; set; } = 1;

        public string ConnectionKey { get; set; } = "SpaceMerchants";

        public Dictionary<NetPeer, Player> Players { get; private set; } = new Dictionary<NetPeer, Player>();

        /// <summary>
        /// Starts listening at the specified port.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <returns>The task.</returns>
        public Server(int port)
        {
            server = new NetManager(listener);
            server.Start(port);

            listener.ConnectionRequestEvent += OnRequestEvent;
            listener.NetworkReceiveEvent += OnReceiveEvent;
            listener.PeerConnectedEvent += OnConnectedEvent;
            listener.PeerDisconnectedEvent += OnDisconnectedEvent;

            timer = new Timer((obj) => Update(), null, 0, server.UpdateTime);

            Game.WriteLine($"Server listening on port {port}", MessageType.Message);
        }

        public void Stop()
        {
            server.Stop(true);

            listener.ConnectionRequestEvent -= OnRequestEvent;
            listener.NetworkReceiveEvent -= OnReceiveEvent;
            listener.PeerConnectedEvent -= OnConnectedEvent;
            listener.PeerDisconnectedEvent -= OnDisconnectedEvent;

            timer?.Dispose();
        }

        private void OnRequestEvent(ConnectionRequest request)
        {
            if (server.ConnectedPeersCount < MaxPlayers)
            {
                string key = request.Data.GetString();

                if (string.CompareOrdinal(key, ConnectionKey) != 0)
                {
                    request.Reject();
                    Game.WriteLine($"Player attempted to connect with wrong key", MessageType.Message);

                    return;
                }

                string name = request.Data.GetString();

                // create the player
                connectingPlayer = new Player(name, false);

                // ensure the player has a valid ship class
                byte shipClassIndex = request.Data.GetByte();

                if (shipClassIndex <= (byte)ShipClass.Num || shipClassIndex >= (byte)ShipClass.Num)
                    shipClassIndex = (byte)ShipClass.Medium;

                // set up the new player's ship at a random outpost
                connectingPlayer.Ship = new Ship(name, Utility.PickOutpost(), (ShipClass)shipClassIndex, true);

                // add the ship to the game's ship list
                Game.Ships.Add(connectingPlayer.Ship);
                request.Accept();
            }
            else
            {
                request.Reject();
                Game.WriteLine($"Player attempted to connect with wrong key", MessageType.Message);
            }
        }

        private void OnReceiveEvent(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
        {
            OnUpdateEvent(peer, reader);

            reader.Recycle();
        }

        private void OnConnectedEvent(NetPeer peer)
        {
            Players.Add(peer, connectingPlayer);

            Game.WriteLine($"Player {connectingPlayer?.Name}({peer.Id}) connected", MessageType.Message);
        }

        private void OnDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            var player = Players[peer];

            Game.WriteLine($"Player {player?.Name}({peer.Id}) disconnected", MessageType.Message);

            // move the ship to somewhere nothing is
            player?.Ship.Warp();

            // remove the player's ship
            Game.Ships.Remove(player?.Ship);

            // remove the players
            Players.Remove(peer);
        }

        private void OnUpdateEvent(NetPeer peer, NetPacketReader reader)
        {
            var player = Players[peer];

            if (player == null)
                return;

            byte selection = reader.GetByte();
            player.Input = reader.GetString();
            player.UseMenu((PlayMenuItem)selection);
        }

        public void Send(NetPeer peer, NetDataWriter writer)
        {
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }

        private void Update()
        {
            server.PollEvents();
        }
    }
}