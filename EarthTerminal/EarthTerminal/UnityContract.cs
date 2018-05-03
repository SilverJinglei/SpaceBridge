using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SpaceStation;

namespace EarthTerminal
{
    public interface IAmbassador
    {
        
    }

    public class UnityContract : HomelandBase
    {
        protected override MethodInfo GetCurrentMethodInfo(string name)
        {
            Debug.Assert(!string.IsNullOrEmpty(name));
            return GetType().GetMethod(name);
        }

        public UnityContract()
        {
            Establish("127.0.0.1", nameof(IAmbassador));
        }
    }
}
