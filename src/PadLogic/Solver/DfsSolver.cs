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

            public int MaxDepth = 12;
            public int NumToKeep = 100;
            public int WhenToPrune = 2500;
        }

        public static Path GetBestPath(Board b)
        {
            return GetBestPath(b, Options.Default);
        }

        public static Path GetBestPath(Board b, Options o)
        {
            Path bestPath = new Path();
            object pathLock = new Object();

            Parallel.For(0, b.Height, i =>
            {
                //for (int i = 0; i < b.Height; i++)
                Parallel.For(0, b.Width, j =>
                {
                    //for (int j = 0; j < b.Width; j++)
                    var curPath = GetBestPathFrom(b, o, i, j);
                    lock (pathLock)
                    {
                        if (curPath.Score > bestPath.Score)
                            bestPath = curPath;
                    }
                    var c = b.GetBoardsAfterPath(curPath.Start.Item1, curPath.Start.Item2, curPath.Actions);
                    var curPath2 = GetBestPathFrom(c.Item1, o, curPath.Current.Item1, curPath.Current.Item2);
                    var actions = new List<int[]>(curPath.Actions);
                    actions.AddRange(curPath2.Actions);
                    curPath2.Actions = actions;
                    lock (pathLock)
                    {
                        if (curPath2.Score > bestPath.Score)
                            curPath = bestPath = new Path
                            {
                                Start = curPath.Start,
                                Current = curPath2.Current,
                                Depth = curPath.Depth + curPath2.Depth,
                                Score = curPath2.Score,
                                Actions = curPath2.Actions
                            };
                    }
                });
            });

            var cc = b.GetBoardsAfterPath(bestPath.Start.Item1, bestPath.Start.Item2, bestPath.Actions);
            var bestPath2 = GetBestPathFrom(cc.Item1, o, bestPath.Current.Item1, bestPath.Current.Item2);
            var actions2 = new List<int[]>(bestPath.Actions);
            actions2.AddRange(bestPath2.Actions);
            bestPath2.Actions = actions2;
            lock (pathLock)
            {
                if (bestPath2.Score > bestPath.Score)
                    bestPath = new Path
                    {
                        Start = bestPath.Start,
                        Current = bestPath2.Current,
                        Depth = bestPath.Depth + bestPath2.Depth,
                        Score = bestPath2.Score,
                        Actions = bestPath2.Actions
                    };
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
                Score = b.Score(BoardScorer.Options.Horus)
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
                    paths.Push(new Tuple<Board, Path>(newBoard, new Path
                    {
                        Start = curPath.Start,
                        Current = new Tuple<int, int>(newY, newX),
                        Depth = curPath.Depth + 1,
                        Score = newBoard.Score(BoardScorer.Options.Horus) - curPath.Depth / 100,
                        Actions = newPath
                    }));
                }
            }
            return bestPath;
        }
    }
}
