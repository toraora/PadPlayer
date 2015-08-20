using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PadLogic.Game
{
    public class Combo
    {
        public Orb OrbType { get; set; }
        public int NumOrbs { get; set; }
        public int NumEnahcements { get; set; }
        public bool IsRow { get; set; }
    }
}
