using SpaceStation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var proxy = Station.Server;

            while (true)
            {
                Console.Write(">");

                string cmd = Console.ReadLine();
                var ps = cmd.Split(' ');

                switch (ps[0].ToLower())
                {
                    case "exit":
                        {
                            break;
                        }
                    case "invoke":
                        {
                            var mc = new OperationMetadata
                            {
                                Type = (MetadataType)Enum.Parse(typeof(MetadataType), ps[1]),
                                Class = ps[2],
                                Name = ps[3],
                                Parameters = ps.Skip(4).ToList()
                            };
                 
                            Console.WriteLine(proxy.Invoke(mc).GetAwaiter().GetResult());
                            break;
                        }
                }
            }
        }
    }
}
