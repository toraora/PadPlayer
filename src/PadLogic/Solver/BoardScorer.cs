using PadLogic.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PadLogic.Solver
{
    public static class BoardScorer
    {
        public static int Score(this Board board)
        {
            List<Combo> combos = board.GetCombos();
            if (combos.Count == 0)
                return 0;
            if (combos.Count == 1)
                return combos[0].NumOrbs;
            return combos
                    .Select(c => c.NumOrbs)
                    .Aggregate((s, t) => s + t);
        }
    }
}
