//-----------------------------------------------------------------------
// <copyright file="Client.cs" company="Leamware">
//     Copyright (c) Leamware. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SpaceMerchants
{
    using System.Threading.Tasks;
    using Spaghetti;
    using Windows.Storage.Streams;

    /// <summary>
    /// The networking client, which implements <see cref="StreamSocketClient"/>.
    /// </summary>
    public class Client : StreamSocketClient
    {
        /// <summary>
        /// Connects the specified host.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <returns>The task.</returns>
        public override async Task Connect(string host, ushort port)
        {
            await base.Connect(host, port);

            if (!Running)
            {
                Game.WriteLine($"Failed to connect to {host}:{port.ToString()}", MessageType.Error);
                return;
            }

            using (var writer = new DataWriter())
            {
                // bytes for name
                writer.WriteStringWithByteLength(Game.Player.Name);

                // byte for ship class
                writer.WriteByte((byte)Game.Player.ShipClass);

                // send the message
                await Send(MessageId.Connect, writer.DetachBuffer());
            }

            Game.WriteLine($"Client connected to {host}:{port.ToString()}", MessageType.Message);
        }

        /// <summary>
        /// Raises the <see cref="E:ConnectEvent"/> event.
        /// </summary>
        /// <param name="args">
        /// The <see cref="ConnectionEventArgs"/> instance containing the event data.
        /// </param>
        public override void OnConnectEvent(ConnectionEventArgs args)
        {
            // read ids for each player connecting
            using (var reader = DataReader.FromBuffer(args.Buffer))
            {
                var messageId = (MessageId)reader.ReadByte();
                byte id = reader.ReadByte();
                bool local = reader.ReadBoolean();
                string name = reader.ReadStringWithByteLength();

                Player player = new Player(name, local);

                if (local)
                    Game.Player = player;

                // add the new player
                AddPlayer(id, player);

                // raise the base method
                base.OnConnectEvent(new ConnectionEventArgs(args.Buffer, player, id));

                Game.WriteLine($"{(local ? "Local " : string.Empty)}client {name} joined", MessageType.Message);
            }
        }

        /// <summary>
        /// Raises the <see cref="Core.DisconnectEvent"/> event.
        /// </summary>
        /// <param name="args">
        /// The <see cref="ConnectionEventArgs"/> instance containing the event data.
        /// </param>
        public override void OnDisconnectEvent(ConnectionEventArgs args)
        {
            // read ids for each player disconnecting
            using (var reader = DataReader.FromBuffer(args.Buffer))
            {
                var messageId = (MessageId)reader.ReadByte();
                byte id = reader.ReadByte();

                if (id >= Players.Count)
                    Game.WriteLine($"Unknown player {id} left", MessageType.Message);

                var player = (Player)Players[id];

                // remove the player
                RemovePlayer(player);

                // raise the base method
                base.OnDisconnectEvent(new ConnectionEventArgs(args.Buffer, player, id));

                Game.WriteLine($"Player {player.Name} left", MessageType.Message);
            }
        }

        /// <summary>
        /// Raises the <see cref="Core.UpdateEvent"/> event.
        /// </summary>
        /// <param name="args">The <see cref="DataEventArgs"/> instance containing the event data.</param>
        public override void OnUpdateEvent(DataEventArgs args)
        {
            using (var reader = DataReader.FromBuffer(args.Buffer))
            {
                var messageId = (MessageId)reader.ReadByte();

                // read lines the server sent
                byte lines = reader.ReadByte();

                Game.WriteLine();

                // write server message
                for (int i = 0; i < lines; i++)
                    Game.WriteLine(reader.ReadStringWithByteLength(), (MessageType)reader.ReadByte());
            }

            base.OnUpdateEvent(args);

            Game.Player.ReceivedReply = true;
        }
    }
}