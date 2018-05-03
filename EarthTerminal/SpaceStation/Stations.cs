using SpaceStation.Core;
using SpaceStation.HighConcurrence;
using SpaceStation.PeerToPeer;

namespace SpaceStation
{
    internal class Stations
    {
        private static ServerClientPair<P2PServerTerminal, P2PClientTerminal> _p2P;

        public static ServerClientPair<P2PServerTerminal, P2PClientTerminal> P2P
            => _p2P ??
               (_p2P = new ServerClientPair<P2PServerTerminal, P2PClientTerminal>());

        private static ServerClientPair<SaeaServerTerminal, SaeaClientTerminal> _saea;

        public static ServerClientPair<SaeaServerTerminal, SaeaClientTerminal> Saea
            => _saea ??
               (_saea = new ServerClientPair<SaeaServerTerminal, SaeaClientTerminal>());

        public class ServerClientPair<TServer, TClient>
            where TServer: ITerminal, new()
            where TClient : ITerminal, new()
        {
            private Station _server;

            public Station Server => _server ?? (_server = new Station(new TServer()));

            private Station _client;
            public Station Client => _client ?? (_client = new Station(new TClient()));
        }
    }
}