using PadLogic.Game;
using PadLogic.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PadLogic.Solver
{
    public class SAASSolver
    {
        public delegate double Heuristic(Board b);
        public delegate Heuristic HeuristicGenerator(Board goal);

        public class Options
        {
            public static Options Default = new Options();

            public int NumParticles = 100;
            public int SAIterations = 100;
            public double StartTemp = 100000;

            private static int OrbDistance(Tuple<int, int> loc1, Tuple<int, int> loc2)
            {
                if (Board.MoveDirections.Length == 4)
                    return    Math.Abs(loc1.Item1 - loc2.Item1) 
                            + Math.Abs(loc1.Item2 - loc2.Item2);
                return    Math.Abs(loc1.Item1 - loc2.Item1)
                        + Math.Abs(loc1.Item2 - loc2.Item2)
                        - Math.Min(
                            Math.Abs(loc1.Item1 - loc2.Item1),
                            Math.Abs(loc1.Item2 - loc2.Item2)
                          );
            }
            public HeuristicGenerator HeuristicGen = (goal) =>
            {
                Dictionary<Orb, HashSet<Tuple<int, int>>> goalOrbLocations = new Dictionary<Orb, HashSet<Tuple<int, int>>>();
                for (int i = 0; i < goal.Height; i++)
                    for (int j = 0; j < goal.Width; j++)
                    {
                        var curOrb = goal.Orbs[i, j];
                        if (goalOrbLocations.ContainsKey(curOrb))
                            goalOrbLocations[curOrb].Add(new Tuple<int, int>(i, j));
                        else
                            goalOrbLocations[curOrb] = new HashSet<Tuple<int, int>> { new Tuple<int, int>(i, j) };
                    }

                return (b) =>
                {
                    Dictionary<Orb, HashSet<Tuple<int, int>>> orbLocations = new Dictionary<Orb, HashSet<Tuple<int, int>>>();
                    for (int i = 0; i < b.Height; i++)
                        for (int j = 0; j < b.Width; j++)
                        {
                            var curOrb = b.Orbs[i, j];
                            if (orbLocations.ContainsKey(curOrb))
                                orbLocations[curOrb].Add(new Tuple<int, int>(i, j));
                            else
                                orbLocations[curOrb] = new HashSet<Tuple<int, int>> { new Tuple<int, int>(i, j) };
                        }
                    int heuristicValue = 0;
                    foreach (var key in orbLocations.Keys)
                    {
                        var curOrbs = orbLocations[key]; 
                        var goalOrbs = new HashSet<Tuple<int, int>>(goalOrbLocations[key]);
                        var common = goalOrbs.Intersect(curOrbs).ToList();
                        goalOrbs.ExceptWith(common);
                        curOrbs.ExceptWith(common);
                        while (curOrbs.Any())
                        {
                            var allPairs = curOrbs
                                .SelectMany(s => goalOrbs.Select(g => new Tuple<Tuple<int, int>, Tuple<int, int>>(s, g)));
                            int shortestDist = 99;
                            Tuple<Tuple<int, int>, Tuple<int, int>> best = null;
                            foreach (var pair in allPairs)
                            {
                                var curDist = OrbDistance(pair.Item1, pair.Item2);
                                if (curDist < shortestDist)
                                {
                                    shortestDist = curDist;
                                    best = pair;
                                }
                            }
                            heuristicValue += shortestDist;
                            curOrbs.Remove(best.Item1);
                            goalOrbs.Remove(best.Item2);
                        }
                    }
                    return heuristicValue / 2.0;
                };
            };
        }

        private static Random rng = new Random();
        private static Board GetRandomBoard(Board b)
        {
            Board newBoard = new Board(b);
            List<Orb> orbs = new List<Orb>();
            for (int i = 0; i < b.Height; i++)
                for (int j = 0; j < b.Width; j++)
                    orbs.Add(b.Orbs[i, j]);
            Shuffle(orbs);
            for (int i = 0; i < b.Height; i++)
                for (int j = 0; j < b.Width; j++)
                    newBoard.Orbs[i, j] = orbs[j + i * b.Width];
            return newBoard;
        }       
        private static Board GetRandomBoardAfterSwap(Board b)
        {
            int y_old = rng.Next(b.Height);
            int x_old = rng.Next(b.Width);
            int y_new, x_new;
            do
            {
                y_new = rng.Next(b.Height);
                x_new = rng.Next(b.Width);
            } while (y_old == y_new && x_old == x_new);
            Board ret = new Board(b);
            Orb temp = ret.Orbs[y_old, x_old];
            ret.Orbs[y_old, x_old] = ret.Orbs[y_new, x_new];
            ret.Orbs[y_new, x_new] = temp;
            return ret;
        }
        private static void Shuffle<T>(List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static List<Board> GetOptimalBoards(Board b, Options o, BoardScorer.Options bso)
        {
            object bestLock = new object();
            Board best = new Board(b);
            double bestScore = 0;
            var r = new Random(rng.Next());
            List<Board> particles = new List<Board>();
            for (int i = 0; i < o.NumParticles; i++)
            {
                particles.Add(GetRandomBoard(b));
            }
            Parallel.For(0, o.SAIterations, iteration =>
            //for (int iteration = 0; iteration < o.SAIterations; iteration++)
            {
                var temp = o.StartTemp * (1.0 - (double)iteration / o.SAIterations);
                for (int n = 0; n < o.NumParticles; n++)
                {
                    var neighbor = GetRandomBoardAfterSwap(particles[n]);
                    var oldBoardScore = particles[n].Score(bso);
                    var newBoardScore = neighbor.Score(bso);
                    if (oldBoardScore < newBoardScore)
                    {
                        particles[n] = neighbor;
                        lock (bestLock)
                        {
                            if (newBoardScore > bestScore)
                            {
                                bestScore = newBoardScore;
                                best = neighbor;
                            }
                        }
                        continue;
                    }
                    if (Math.Exp(-(newBoardScore - oldBoardScore) / temp) < r.NextDouble())
                    {
                        particles[n] = neighbor;
                        continue;
                    }
                }
            });
            return new List<Board> { best };
            //return particles.Where(p => p.Score(bso) == particles.Max(s => s.Score(bso))).ToList();
        } 

        public static Path GetBestPath(Board b, Options o, BoardScorer.Options bso)
        {
            Board goalBoard = GetOptimalBoards(b, o, bso).First();
            Heuristic h = o.HeuristicGen(goalBoard);
            PriorityQueue<Tuple<Board, Path>, double> queue = new PriorityQueue<Tuple<Board, Path>, double>();
            HashSet<Board> visited = new HashSet<Board>();
            Dictionary<Board, double> toExplore = new Dictionary<Board, double>();
            toExplore.Add(b, 0);
            queue.Enqueue(new Tuple<Board, Path>(b, new Path()), -h(b));

            while (!queue.Empty)
            {
                var cur = queue.Dequeue();
                var curBoard = cur.Item1;
                var curPath = cur.Item2;
                var curCost = curPath.Depth;
                if (curBoard.EqualsBoard(goalBoard))
                    return curPath;
                visited.Add(curBoard);
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
                    var tentativeCost = toExplore
                    if (visited.Contains(newBoard) || toExplore.ContainsKey(newBoard) && toExplore[newBoard] <= curCost + 1)
                        continue;
                    var newPath = new List<int[]>(curPath.Actions);
                    newPath.Add(direction);
                    queue.Enqueue(new Tuple<Board, Path>(newBoard, new Path
                    {
                        Start = curPath.Start,
                        Current = new Tuple<int, int>(newY, newX),
                        Depth = curCost + 1,
                        Score = 0, //newBoard.Score(BoardScorer.Options.Horus) - curPath.Depth / 100,
                        Actions = newPath
                    }), -h(newBoard));
                    toExplore[newBoard] = curCost + 1;
                }
            }

            return new Path();
        }
    }
}
