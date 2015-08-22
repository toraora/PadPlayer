using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PadLogic.Game
{
    public class Board
    {
        public Orb[,] Orbs { get; private set; }
        public bool[,] Enhancements { get; private set; }

        public int Height { get { return Orbs.GetLength(0); } }
        public int Width { get { return Orbs.GetLength(1); } }

        public Board(int height, int width)
        {
            Orbs = new Orb[height, width];
            Enhancements = new bool[height, width];
        }

        public Board(Board b)
            : this(b.Height, b.Width)
        {
            Array.Copy(b.Orbs, Orbs, b.Height * b.Width);
            Array.Copy(b.Enhancements, Enhancements, b.Height * b.Width);
        }

        public static int[][] MoveDirections = { new int[] { 1, 0 }, new int[] { -1, 0 }, new int[] { 0, 1 }, new int[] { 0, -1 } };

        public Tuple<Board, Board, List<Combo>> GetBoardsAfterPath(int y, int x, List<int[]> path)
        {
            Board b = new Board(this);
            foreach (var direction in path)
            {
                var newY = y + direction[0];
                var newX = x + direction[1];
                Orb temp = b.Orbs[newY, newX];
                b.Orbs[newY, newX] = b.Orbs[y, x];
                b.Orbs[y, x] = temp;
                y = newY;
                x = newX;
            }
            Board clear = new Board(b);
            var combos = clear.GetCombos(false);
            return new Tuple<Board, Board, List<Combo>>(b, clear, combos);
        }

        public List<Combo> GetCombos(bool copy = true)
        {
            Board copyBoard = new Board(this);
            if (!copy)
                copyBoard = this;
            List<Combo> combos = new List<Combo>();
            List<Combo> lastComboSet = new List<Combo>();
            do
            {
                lastComboSet = copyBoard.GetCombosWithoutGravity();
                combos.AddRange(lastComboSet);
                copyBoard.GravityFill();
            } while (lastComboSet.Any());
            return combos;
        }

        private static int[][] MatchDirections = { new int[] { 1, 0 }, new int[] { -1, 0 }, new int[] { 0, 1 }, new int[] { 0, -1 } };

        // this function changes the board! make a copy before calling it
        private List<Combo> GetCombosWithoutGravity()
        {
            List<Combo> combos = new List<Combo>();
            Board comboBoard = new Board(Height, Width);
            for (int i = 0; i < Height; i++)
                for (int j = 1; j < Width - 1; j++)
                {
                    if (Orbs[i, j] == Orb.None)
                        continue;
                    if (Orbs[i, j - 1] == Orbs[i, j] && Orbs[i, j] == Orbs[i, j + 1])
                    {
                        comboBoard.Orbs[i, j - 1] = Orbs[i, j];
                        comboBoard.Orbs[i, j] = Orbs[i, j];
                        comboBoard.Orbs[i, j + 1] = Orbs[i, j];
                    }
                }
            for (int i = 1; i < Height - 1; i++)
                for (int j = 0; j < Width; j++)
                {
                    if (Orbs[i, j] == Orb.None)
                        continue;
                    if (Orbs[i - 1, j] == Orbs[i, j] && Orbs[i, j] == Orbs[i + 1, j])
                    {
                        comboBoard.Orbs[i - 1, j] = Orbs[i, j];
                        comboBoard.Orbs[i, j] = Orbs[i, j];
                        comboBoard.Orbs[i + 1, j] = Orbs[i, j];
                    }
                }
            // row check
            bool[] isRows = new bool[Height];
            for (int i = 0; i < Height; i++)
                for (int j = 1; j < Width; j++)
                    isRows[i] = Orbs[i, j] == Orbs[i, 0];

            Stack<Tuple<int, int>> orbStack = new Stack<Tuple<int, int>>();
            HashSet<Tuple<int, int>> visited = new HashSet<Tuple<int, int>>();
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                {
                    int comboNumOrbs = 0;
                    int comboNumEnhances = 0;
                    bool isRow = false;
                    Orb orbType = Orbs[i, j];

                    if (comboBoard.Orbs[i, j] == Orb.None)
                        continue;

                    orbStack.Clear();
                    orbStack.Push(new Tuple<int, int>(i, j));
                    while (orbStack.Any())
                    {
                        var curLoc = orbStack.Pop();
                        if (visited.Contains(new Tuple<int, int>(curLoc.Item1, curLoc.Item2)))
                            continue;
                        comboBoard.Orbs[curLoc.Item1, curLoc.Item2] = Orb.None;
                        Orbs[curLoc.Item1, curLoc.Item2] = Orb.None;
                        comboNumOrbs++;
                        if (Enhancements[curLoc.Item1, curLoc.Item2])
                            comboNumEnhances++;
                        isRow = isRow ? isRows[curLoc.Item1] : false;
                        visited.Add(curLoc);
                        foreach (var direction in MatchDirections)
                        {
                            var newY = curLoc.Item1 + direction[0];
                            var newX = curLoc.Item2 + direction[1];
                            if (newY < 0 || newY >= Height ||
                                newX < 0 || newX >= Width ||
                                visited.Contains(new Tuple<int, int>(newY, newX)) ||
                                orbType != comboBoard.Orbs[newY, newX])
                                continue;
                            orbStack.Push(new Tuple<int, int>(newY, newX));
                        }
                    }

                    combos.Add(new Combo
                    {
                        OrbType = orbType,
                        NumOrbs = comboNumOrbs,
                        NumEnahcements = comboNumEnhances,
                        IsRow = isRow
                    });
                }

            return combos;
        }
        private void GravityFill()
        {
            Stack<Tuple<int, int>> orbPositions = new Stack<Tuple<int, int>>();
            for (int j = 0; j < Width; j++)
            {
                orbPositions.Clear();
                for (int i = 0; i < Height; i++)
                    if (Orbs[i, j] != Orb.None)
                        orbPositions.Push(new Tuple<int, int>(i, j));
                for (int i = Height - 1; i >= 0; i--)
                {
                    if (orbPositions.Any())
                    {
                        var loc = orbPositions.Pop();
                        Orbs[i, j] = Orbs[loc.Item1, loc.Item2];
                    }
                    else
                        Orbs[i, j] = Orb.None;
                }
            }
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = 31 * hash + Orbs.GetHashCode();
            hash = 31 * hash + Enhancements.GetHashCode();
            return hash;
        }

        public bool EqualsBoard(Board b)
        {
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                    if (Orbs[i, j] != b.Orbs[i, j]
                        || Enhancements[i, j] != b.Enhancements[i, j])
                        return false;
            return true;
        }

        public override string ToString()
        {
            string ret = "\n";
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    ret += Enum.GetName(typeof(Orb), Orbs[i, j]).Substring(0, 1) + " ";
                }
                ret += "\n";
            }
            return ret;              
        }
    }
}
