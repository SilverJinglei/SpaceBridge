using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EarthTerminal.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpaceStation;
using SpaceStation.Service;

namespace EarthTerminal
{
    class Program
    {
        static void Main2(string[] args)
        {
            
            Console.Read();
        }

        static void Main(string[] args)
        {

        }

        static void vrpTest()
        {
            //ResultMetadataTest.TestResultMetadata();

            string vrpFile = @"c:\动态vrp\吊车动画.vrp";

            var o = JValue.FromObject(vrpFile);

            var jo = JsonConvert.SerializeObject(o);
            //MainAsync().Wait();
        }

        /*
        static async  Task MainAsync()
        {
            
            //Stations.Client.ServerIp = "192.168.0.101"; //"192.168.0.9"

            var proxy = Stations.P2P.Client;
            var vrpProxy = new VrpAmbassadorBase();

            while (true)
            {
                Console.Write(">");
                
                string cmd = Console.ReadLine();
                var ps = cmd.Split(' ');

                string vrpFile = @"c:\动态vrp\吊车动画.vrp"; 
                //"D:\\项目资料\\vrp\\动态vrp\\吊车动画.vrp";
                //@"c:\动态vrp\吊车动画.vrp";
                switch (ps[0].ToLower())
                {
                    case "exit":
                        {
                            break;
                        }
                    case "async":
                    {
                        var result = vrpProxy.ImportVrpFileAsync(vrpFile);

                        Console.WriteLine(await result);
                        break;
                    }
                    case "vrp":
                        {
                        //var result = vrpProxy.ImportVrpFile(vrpFile);

                        //Console.WriteLine(result);
                        break;
                    }
                    case "invoke":
                    {
                        var result = await proxy.InvokeRemoteAsync(
                            nameof(LocalService),
                            nameof(LocalService.Add),
                            false,
                            new Dictionary<string, object>
                            {
                                ["a"] = 1,
                                ["b"] = 5
                            });

                            Console.WriteLine(result);
                            break;
                        }
                    case "send":
                    {
                        //await proxy.Terminal.Send("中");
                        break;
                    }
                    case "demo":
                    {
                        var file = "D:\\中视典软件及相关教程\\SDK\\sdk案例测试\\测试案例\\SDK\\switchs.vrp";

                        Console.WriteLine(proxy.InvokeRemoteAsync("VRP_Model", "VRPGetModelsNameFromVrpFile", false, 
                            new Dictionary<string, object>
                            {
                                [nameof(file)] = file
                            }));
                        break;
                    }
                    case "draw":
                    {
                        //Console.WriteLine(proxy.InvokeRemote("VRP_Model", "VRPSetWireFrameMode", true,
                        //    ps[1]));
                        break;
                    }
                }
            }
        }
        */
    }
}
