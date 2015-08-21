using PadLogic.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PadLogic.Solver
{
    public class DfsSolver
    {
        public class Options
        {
            public static Options Default = new Options();

            public int MaxDepth = 11;
            public int NumToKeep = 100;
            public int WhenToPrune = 10000;
        }
        public static Path GetBestPath(Board b)
        {
            return GetBestPath(b, Options.Default);
        }

        public static Path GetBestPath(Board b, Options o)
        {
            Path bestPath = new Path();

            for (int i = 0; i < b.Height; i++)
                for (int j = 0; j < b.Width; j++)
                {
                    var curPath = GetBestPathFrom(b, o, i, j);
                    if (curPath.Score > bestPath.Score)
                        bestPath = curPath;
                }

            return bestPath;
        }
        private static Path GetBestPathFrom(Board b, Options o, int y, int x)
        {
            Path bestPath = new Path();
            Stack<Tuple<Board, Path>> paths = new Stack<Tuple<Board, Path>>();
            paths.Push(new Tuple<Board, Path>(b, new Path
            {
                Start = new Tuple<int, int>(y, x),
                Current = new Tuple<int, int>(y, x),
                Depth = 1,
                Score = b.Score()
            }));
            while (paths.Any())
            {
                var cur = paths.Pop();
                var curPath = cur.Item2;
                var curBoard = cur.Item1;
                if (curPath.Score > bestPath.Score)
                    bestPath = curPath;
                if (curPath.Depth == o.MaxDepth)
                    continue;
                if (paths.Count() > o.WhenToPrune)
                {
                    var newPaths = new Stack<Tuple<Board, Path>>();
                    foreach (var path in paths.OrderByDescending(p => p.Item2.Score).Take(o.NumToKeep).Reverse())
                        newPaths.Push(path);
                    paths = newPaths;
                }

                foreach (var direction in Board.MoveDirections)
                {
                    if (curPath.Length != 0 &&
                        curPath.Actions.Last()[0] == -direction[0] &&
                        curPath.Actions.Last()[1] == -direction[1])
                        continue;
                    var newY = curPath.Current.Item1 + direction[0];
                    var newX = curPath.Current.Item2 + direction[1];
                    if (newY < 0 || newY >= b.Height ||
                        newX < 0 || newX >= b.Height)
                        continue;
                    Board newBoard = new Board(curBoard);
                    Orb tempOrb = newBoard.Orbs[newY, newX];
                    newBoard.Orbs[newY, newX] = newBoard.Orbs[curPath.Current.Item1, curPath.Current.Item2];
                    newBoard.Orbs[curPath.Current.Item1, curPath.Current.Item2] = tempOrb;
                    var newPath = new List<int[]>(curPath.Actions);
                    newPath.Add(direction);
                    paths.Push(new Tuple<Board, Path>(newBoard, new Path
                    {
                        Start = curPath.Start,
                        Current = new Tuple<int, int>(newY, newX),
                        Depth = curPath.Depth + 1,
                        Score = newBoard.Score() * 100 - curPath.Depth,
                        Actions = newPath
                    }));
                }
            }
            return bestPath;
        }
    }
}
