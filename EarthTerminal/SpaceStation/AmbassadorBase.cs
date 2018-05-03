using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SpaceStation
{
    public abstract class HomelandBase : IntelligentCommunication
    {
        public override Task Establish(string homelandAddress, int port, string ambassador)
        {
            Station = Stations.Saea.Server;
            return base.Establish(homelandAddress, port, ambassador);
        }
    }

    public abstract class AmbassadorBase : IntelligentCommunication
    {
        public override Task Establish(string homelandAddress, int port, string homeland)
        {
            Station = Stations.Saea.Client; //P2P
            return base.Establish(homelandAddress, port, homeland);
        }
    }
}