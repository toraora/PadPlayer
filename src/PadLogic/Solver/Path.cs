using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PadLogic.Solver
{
    public class Path
    {
        public int Length { get { return Actions.Count(); } }
        public List<int[]> Actions { get; set; } = new List<int[]>();
        public Tuple<int, int> Start;
        public int Score;
        public int Depth;
        public Tuple<int, int> Current;
    }
}
