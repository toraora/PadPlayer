using PadLogic.Game;
using PadLogic.Solver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PadLogic
{
    public class Teams
    {
        public static BoardScorer.Options LuBu = new BoardScorer.Options
        {

            Power = new Dictionary<Orb, int>
                {
                    { Orb.Red, 3750 },
                    { Orb.Blue, 481 },
                    { Orb.Green, 0 },
                    { Orb.Light, 0 },
                    { Orb.Dark, 9400 }
                },

            TpaPower = new Dictionary<Orb, int>
                {
                    { Orb.Red, 250 },
                    { Orb.Blue, 0 },
                    { Orb.Green, 0 },
                    { Orb.Light, 0 },
                    { Orb.Dark, 2400 }
                },

            Rows = new Dictionary<Orb, int>
                {
                    { Orb.Red, 1 },
                    { Orb.Blue, 0 },
                    { Orb.Green, 0 },
                    { Orb.Light, 0 },
                    { Orb.Dark, 10 }
                },

            Enhances = new Dictionary<Orb, int>
                {
                    { Orb.Red, 0 },
                    { Orb.Blue, 0 },
                    { Orb.Green, 0 },
                    { Orb.Light, 0 },
                    { Orb.Dark, 0 }
                },

            factor = (c) =>
            {
                if (c.Count > 5)
                    return 1.2;
                return 1;
            }
        };
    }
}
