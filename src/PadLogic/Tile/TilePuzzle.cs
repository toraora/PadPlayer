using PadLogic.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PadLogic.Tile
{
    public class TilePuzzle
    {
        private int[,] m_current;
        private int[,] m_target;

        private int Height { get { return m_current.GetLength(0); } }
        private int Width { get { return m_current.GetLength(1); } }

        private TilePuzzle(int[,] current, int[,] target)
        {

        }

        private List<int[]> Solve()
        {
            List<int[]> moves = new List<int[]>();
            Tuple<int, int> cursor = new Tuple<int, int>(Height - 1, Width - 1);

            // beam search
            PriorityQueue<int[,]> queue = new PriorityQueue<int[,]>();

            return moves;
        }
    }
}
