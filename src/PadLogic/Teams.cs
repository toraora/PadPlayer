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
                    { Orb.Red, 100 },
                    { Orb.Blue, 100 },
                    { Orb.Green, 100 },
                    { Orb.Light, 100 },
                    { Orb.Dark, 1000 }
                },

            TpaPower = new Dictionary<Orb, int>
                {
                    { Orb.Red, 100 },
                    { Orb.Blue, 100 },
                    { Orb.Green, 100 },
                    { Orb.Light, 100 },
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
                double mult = 1.0;
                var orbTypes = new HashSet<Orb>(c.Select(s => s.OrbType));
                var orbTypeCount = orbTypes.Intersect(new List<Orb> { Orb.Red, Orb.Blue, Orb.Green, Orb.Light, Orb.Dark }).Count();
                if (orbTypeCount == 5)
                    mult = 5;

                if (c.Count == 9) return mult * 4;
                if (c.Count == 10) return mult * 6;
                if (c.Count == 11) return mult * 8;
                if (c.Count >= 12) return mult * 10;
                return mult;
            }
        };
    }
}
