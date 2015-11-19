using PadLogic.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PadLogic.Solver
{
    public static class BoardScorer
    {
        public delegate double ComboFactor(List<Combo> combos);

        public class Options
        {
            public static Options Horus = new Options();

            public Dictionary<Orb, int> Power = new Dictionary<Orb, int>
            {
                { Orb.Red, 3750 },
                { Orb.Blue, 1814 },
                { Orb.Green, 1715 },
                { Orb.Light, 3000 },
                { Orb.Dark, 1000 }
            };

            public Dictionary<Orb, int> TpaPower = new Dictionary<Orb, int>
            {
                { Orb.Red, 2281 },
                { Orb.Blue, 900 },
                { Orb.Green, 200 },
                { Orb.Light, 2875 },
                { Orb.Dark, 225 }
            };

            public Dictionary<Orb, int> Rows = new Dictionary<Orb, int>
            {
                { Orb.Red, 0 },
                { Orb.Blue, 0 },
                { Orb.Green, 0 },
                { Orb.Light, 0 },
                { Orb.Dark, 0 }
            };

            public Dictionary<Orb, int> Enhances = new Dictionary<Orb, int>
            {
                { Orb.Red, 4 },
                { Orb.Blue, 1 },
                { Orb.Green, 0 },
                { Orb.Light, 0 },
                { Orb.Dark, 0 }
            };

            public ComboFactor factor = (c) =>
            {
                var orbTypes = new HashSet<Orb>(c.Select(s => s.OrbType));
                var orbTypeCount = orbTypes.Intersect(new List<Orb> { Orb.Red, Orb.Blue, Orb.Green, Orb.Light, Orb.Dark }).Count();
                if (orbTypeCount == 4)
                    return 16;
                if (orbTypeCount == 5)
                    return 20.25;
                return 1;
            };
        }

        public static double Score(this Board board, Options o)
        {
            List<Combo> combos = board.GetCombos();
            return combos.Sum(c => c.Score(o)) * o.factor(combos) * (0.75 + 0.25 * combos.Count) + combos.Count;
        }

        public static double Score(this Combo combo, Options o)
        {
            if (combo.OrbType == Orb.Heal || combo.OrbType == Orb.Jammer)
                return 200 * combo.NumOrbs;
            return 
                (o.Power[combo.OrbType] + (combo.IsTpa ? o.TpaPower[combo.OrbType] : 0))  // base power
                * 0.25 * (1 + combo.NumOrbs)                                            // num orbs
                * (combo.IsRow ? (1 + 0.1 * o.Rows[combo.OrbType]) : 1)                 // row
                * (combo.NumEnahcements != 0 ? 1 + 0.04 * o.Enhances[combo.OrbType] + 0.06 * combo.NumEnahcements : 1) // enhances
            ;
        }
    }
}
