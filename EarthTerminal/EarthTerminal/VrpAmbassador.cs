using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SpaceStation;

namespace EarthTerminal
{
    public class VrpAmbassadorBase : AmbassadorBase
    {
        protected override MethodInfo GetCurrentMethodInfo(string name)
        {
            throw new System.NotImplementedException();
        }

        public VrpAmbassadorBase()
        {
            ClassName = "Vrp_Model";
        }

        //public VrpStructure ImportVrpFile(string path)
        //{
        //    var result = IntelligentInvoke(path);

        //    return (result as JToken)?.ToObject<VrpStructure>();
        //}

        public async Task<VrpStructure> ImportVrpFileAsync(string path)
        {
            var result = await Station.InvokeRemoteAsync(
                ClassName, 
                RemoveAsync(nameof(ImportVrpFileAsync)), 
                false,
                new Dictionary<string, object>
                {
                    [nameof(path)] = path
                });

            return (result as JToken) ?.ToObject<VrpStructure>();
        }
    }
}