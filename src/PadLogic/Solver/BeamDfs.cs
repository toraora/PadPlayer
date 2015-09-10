using PadLogic.Game;
using PadLogic.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PadLogic.Solver
{
    public class BeamDfs
    {
        public class Options
        {
            public static Options Default = new Options();

            public int MaxDepth = 80;
            public int BeamWidth = 100;
        }

        public static Path GetBestPath(Board b, BoardScorer.Options bso)
        {
            return GetBestPath(b, Options.Default, bso);
        }

        public static Path GetBestPath(Board b, Options o, BoardScorer.Options bso)
        {
            Path bestPath = new Path();
            object pathLock = new object();

            Parallel.For(0, b.Height, i =>
            {
                //for (int i = 0; i < b.Height; i++)
                Parallel.For(0, b.Width, j =>
                {
                    //for (int j = 0; j < b.Width; j++)
                    var curPath = GetBestPathFrom(b, o, i, j, bso);
                    lock (pathLock)
                    {
                        if (curPath.Score > bestPath.Score)
                            bestPath = curPath;
                    }
                });
            });

            return bestPath;
        }

        private static Path GetBestPathFrom(Board b, Options o, int y, int x, BoardScorer.Options bso)
        {
            Path bestPath = new Path();
            PriorityQueue<Tuple<Board, Path>, double> paths = new PriorityQueue<Tuple<Board, Path>, double>();
            paths.Enqueue(new Tuple<Board, Path>(b, new Path
            {
                Start = new Tuple<int, int>(y, x),
                Current = new Tuple<int, int>(y, x),
                Depth = 1,
                Score = b.Score(bso)
            }));
            int depth = 0;
            while (depth++ < o.MaxDepth)
            {
               // Console.WriteLine("currently at depth {0}...", depth);
                PriorityQueue<Tuple<Board, Path>, double> newPaths = new PriorityQueue<Tuple<Board, Path>, double>();
                while (paths.Count != 0)
                {
                    var cur = paths.Dequeue();
                    var curPath = cur.Item2;
                    var curBoard = cur.Item1;
                    if (curPath.Score > bestPath.Score)
                        bestPath = curPath;

                    //if (paths.Count() > o.WhenToPrune)
                    //{
                    //    var newPaths = new Stack<Tuple<Board, Path>>();
                    //    foreach (var path in paths.OrderByDescending(p => p.Item2.Score).Take(o.NumToKeep).Reverse())
                    //        newPaths.Push(path);
                    //    paths = newPaths;
                    //}

                    foreach (var direction in Board.MoveDirections)
                    {
                        if (curPath.Length != 0 &&
                            curPath.Actions.Last()[0] == -direction[0] &&
                            curPath.Actions.Last()[1] == -direction[1])
                            continue;
                        var newY = curPath.Current.Item1 + direction[0];
                        var newX = curPath.Current.Item2 + direction[1];
                        if (newY < 0 || newY >= b.Height ||
                            newX < 0 || newX >= b.Width)
                            continue;
                        Board newBoard = new Board(curBoard);
                        Orb tempOrb = newBoard.Orbs[newY, newX];
                        newBoard.Orbs[newY, newX] = newBoard.Orbs[curPath.Current.Item1, curPath.Current.Item2];
                        newBoard.Orbs[curPath.Current.Item1, curPath.Current.Item2] = tempOrb;
                        var newPath = new List<int[]>(curPath.Actions);
                        newPath.Add(direction);
                        double score = newBoard.Score(bso) - curPath.Depth / 100;
                        newPaths.Enqueue(new Tuple<Board, Path>(newBoard, new Path
                        {
                            Start = curPath.Start,
                            Current = new Tuple<int, int>(newY, newX),
                            Depth = curPath.Depth + 1,
                            Score = score,
                            Actions = newPath
                        }), score);
                    }
                }
                paths = newPaths.TrimToSize(o.BeamWidth);
            }
            return bestPath;
        }
    }
}
