using System;
using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections;
using System.Net;
using NetworkCommsDotNet.DPSBase;

//newConn.AppendShutdownHandler(MethodToRunOnConnectionClose);
//connection.ConnectionAlive();

namespace CardGameServ
{

    class ServerNetwork
    {
        WaitingRoom room;

        public ServerNetwork(WaitingRoom nroom, int port)
        {
            room = nroom;

            NetworkComms.AppendGlobalIncomingPacketHandler<Hello>("Hello", processHello);
            NetworkComms.AppendGlobalIncomingPacketHandler<Msg>("Msg", processMsg);
            NetworkComms.AppendGlobalIncomingPacketHandler<GameCommand>("GameCommand", processGC);
            NetworkComms.AppendGlobalConnectionCloseHandler(ClientDisconnected);

            Connection.StartListening(ConnectionType.TCP, new IPEndPoint(IPAddress.Parse("127.0.0.1"), port));

            Console.WriteLine("Listening for messages on:");
            foreach (System.Net.IPEndPoint localEndPoint in Connection.ExistingLocalListenEndPoints(ConnectionType.TCP))
                Console.WriteLine("{0}:{1}", localEndPoint.Address, localEndPoint.Port);
        }
        private void processHello(PacketHeader packetHeader, Connection connection, Hello msg)
        {
            Back b = new Back();

            if (!room.isPlayerRegistered(msg.text))
            {
                b.text = "[server] Welcome, you are currently in the waiting room, waiting for a game to begin.";
                connection.SendObject<Back>("Back", b);
                room.AddPlayer(connection, msg.text);
            }
            else
            {
                b.text = "[!server] Seems like an other player is already using this name. Try an other one";
                connection.SendObject<Back>("Back", b);
            }
        }

        private void processMsg(PacketHeader packetHeader, Connection connection, Msg msg)
        {
            room.sendChatMessage(msg);
        }

        private void processGC(PacketHeader packetHeader, Connection connection, GameCommand msg)
        {
            room.dispatchCommand(msg);
        }

        private void ClientDisconnected(Connection connection)
        {
            Console.WriteLine("A client has disconnected - " + connection.ToString());
            room.removePlayer(connection);
        }

    }
}
