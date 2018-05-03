using System;
using EarthTerminal.Properties;
using Newtonsoft.Json.Linq;
using SpaceStation.Core;

namespace EarthTerminal
{
    class ResultMetadataTest
    {
        public static void TestResultMetadata()
        {
            try
            {
                var r1 = ResultMetadata.Parse(Resources.VrpResult1);

                var r2 = ResultMetadata.Parse(Resources.VrpResult2);

                var vrp = (r2.Value as JToken).ToObject<VrpStructure>();
                var vrp2 = (r1.Value as JToken).ToObject<VrpStructure>();

            }
            catch (Exception ex)
            {
            }
        }
    }
}
