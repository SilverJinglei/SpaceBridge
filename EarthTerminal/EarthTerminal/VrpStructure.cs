using System;
using System.Collections.Generic;

namespace EarthTerminal
{
    public class VrpStructure
    {
        public class Group
        {
            public string Name { get; set; }

            public List<Guid> Guids { get; set; }
        }

        public class Model
        {
            public string Name { get; set; }

            public Guid Guid { get; set; } 
        }

        public List<Group> Groups { get; set; }

        public List<Model> Models { get; set; }
    }
}