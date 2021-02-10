//-----------------------------------------------------------------------
// <copyright file="Server.cs" company="Leamware">
//     Copyright (c) Leamware. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SpaceMerchants.Server
{
    using System.Linq;
    using System.Threading.Tasks;
    using Spaghetti;
    using Windows.Storage.Streams;

    /// <summary>
    /// The networking server, which implements <see cref="StreamSocketServer"/>.
    /// </summary>
    public class Server : StreamSocketServer
    {
        /// <summary>
        /// Starts listening at the specified port.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <param name="maxPlayers">The maximum players.</param>
        /// <returns>The task.</returns>
        public async Task Start(ushort port, byte maxPlayers)
        {
            MaxPlayers = maxPlayers;

            await Start(port);

            if (Running)
                Game.WriteLine($"Server listening on port {port}", MessageType.Message);
            else
                Game.WriteLine($"Failed to start server on port {port}", MessageType.Error);
        }

        /// <summary>
        /// Raises the <see cref="Core.ConnectEvent"/> event.
        /// </summary>
        /// <param name="args">
        /// The <see cref="ConnectionEventArgs"/> instance containing the event data.
        /// </param>
        public override void OnConnectEvent(ConnectionEventArgs args)
        {
            // read player data from the provided buffer
            using (var reader = DataReader.FromBuffer(args.Buffer))
            {
                var messageId = (MessageId)reader.ReadByte();
                string name = reader.ReadStringWithByteLength();

                // create the player
                var newPlayer = new Player(name, false, args.Connection);

                // ensure the player has a valid ship class
                byte shipClassIndex = reader.ReadByte();

                if (shipClassIndex <= (byte)ShipClass.Num || shipClassIndex >= (byte)ShipClass.Num)
                    shipClassIndex = (byte)ShipClass.Medium;

                // set up the new player's ship at a random outpost
                newPlayer.Ship = new Ship(name, Utility.PickOutpost(), (ShipClass)shipClassIndex, true);

                // add the ship to the game's ship list
                Game.Ships.Add(newPlayer.Ship);

                byte id = (byte)AddPlayer(newPlayer);

                Game.WriteLine($"Player {name}({id}) connected", MessageType.Message);

                // raise the base event
                base.OnConnectEvent(new ConnectionEventArgs(args.Buffer, newPlayer));
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
            var disconnectingPlayers = Players.Where(p => p.Value.Connection == args.Connection);

            // raise the base event
            base.OnDisconnectEvent(new ConnectionEventArgs(args.Buffer, args.Connection));

            foreach (var disconnectingPlayer in disconnectingPlayers)
            {
                Game.WriteLine($"Player {(disconnectingPlayer.Value as Player).Name}({disconnectingPlayer.Key}) disconnected", MessageType.Message);

                // move the ship to somewhere nothing is
                (disconnectingPlayer.Value as Player).Ship.Warp();

                // remove the player's ship
                Game.Ships.Remove((disconnectingPlayer.Value as Player).Ship);
            }

            // remove the players
            RemovePlayers(args.Connection);
        }

        /// <summary>
        /// Raises the <see cref="Core.UpdateEvent"/> event.
        /// </summary>
        /// <param name="args">The <see cref="DataEventArgs"/> instance containing the event data.</param>
        public override void OnUpdateEvent(DataEventArgs args)
        {
            using (var reader = DataReader.FromBuffer(args.Buffer))
            {
                // 1 byte for the message id
                var messageId = (MessageId)reader.ReadByte();

                // loop through each player on the connection
                foreach (var player in Players.Where(p => p.Value.Connection == args.Connection))
                {
                    byte selection = reader.ReadByte();
                    (player.Value as Player).Input = reader.ReadStringWithByteLength();
                    (player.Value as Player).UseMenu((PlayMenuItem)selection);
                }
            }

            base.OnUpdateEvent(args);
        }
    }
}