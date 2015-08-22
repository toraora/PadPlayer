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
            public int SAIterations = 200;
            public double StartTemp = 100000;

            public int TILE_SAIterations = 5000;
            public double TILE_StartTemp = 10000;

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
        private static Board GetBoardAfterRandomSwap(Board b, Random r)
        {
            Board ret = new Board(b);

            int y_old = rng.Next(b.Height);
            int x_old = rng.Next(b.Width);
            int y_new, x_new;
            do
            {
                y_new = rng.Next(b.Height);
                x_new = rng.Next(b.Width);
            } while (y_old == y_new && x_old == x_new);
            
            Orb temp = ret.Orbs[y_old, x_old];
            ret.Orbs[y_old, x_old] = ret.Orbs[y_new, x_new];
            ret.Orbs[y_new, x_new] = temp;

            return ret;
        }
        private static Tuple<Board, int[]> GetBoardAfterRandomSwap(Board b, Random r, Path curPath)
        {
            Board ret = new Board(b);
            int[] direction = null;
            int newY = 0, newX = 0;
            do
            {
                var newDirection = Board.MoveDirections[r.Next(Board.MoveDirections.Length)];
                if (curPath.Length != 0 &&
                      curPath.Actions.Last()[0] == -newDirection[0] &&
                      curPath.Actions.Last()[1] == -newDirection[1])
                    continue;
                newY = curPath.Current.Item1 + newDirection[0];
                newX = curPath.Current.Item2 + newDirection[1];
                if (newY < 0 || newY >= b.Height ||
                    newX < 0 || newX >= b.Width)
                    continue;
                direction = newDirection;
            } while (direction == null);

            Orb temp = ret.Orbs[newY, newX];
            ret.Orbs[newY, newX] = ret.Orbs[curPath.Current.Item1, curPath.Current.Item2];
            ret.Orbs[curPath.Current.Item1, curPath.Current.Item2] = temp;

            return new Tuple<Board, int[]>(ret, direction);
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
            List<Board> particles = new List<Board>();
            for (int i = 0; i < o.NumParticles; i++)
            {
                particles.Add(GetRandomBoard(b));
            }
            Parallel.For(0, o.SAIterations, iteration =>
            //for (int iteration = 0; iteration < o.SAIterations; iteration++)
            {
                var r = new Random(rng.Next());
                var temp = o.StartTemp * (1.0 - (double)iteration / o.SAIterations);
                for (int n = 0; n < o.NumParticles; n++)
                {
                    var neighbor = GetBoardAfterRandomSwap(particles[n], r);
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

        public static Path GetBestPathSA(Board b, Options o, BoardScorer.Options bso)
        {
            object bestLock = new object();
            Path best = new Path();
            double bestScore = 100000;
            Board goalBoard = GetOptimalBoards(b, o, bso).First();
            Heuristic h = o.HeuristicGen(goalBoard);
            List<Board> particles = new List<Board>();
            List<Path> particlePaths = new List<Path>();
            for (int i = 0; i < o.NumParticles; i++)
            {
                var pos = new Tuple<int, int>(rng.Next(b.Height), rng.Next(b.Width));
                particles.Add(new Board(b));
                particlePaths.Add(new Path
                {
                    Start = pos,
                    Current = pos
                });
            }

            //Parallel.For(0, o.SAIterations, iteration =>
            for (int iteration = 0; iteration < o.TILE_SAIterations; iteration++)
            {
                var r = new Random(rng.Next());
                var temp = o.TILE_StartTemp * (1.0 - (double)iteration / o.SAIterations);
                for (int n = 0; n < o.NumParticles; n++)
                {
                    //if (goalBoard.EqualsBoard(particles[n]))
                    //    return particlePaths[n];
                    var neighbor = GetBoardAfterRandomSwap(particles[n], r, particlePaths[n]);
                    var oldBoardScore = h(particles[n]);
                    var newBoardScore = h(neighbor.Item1);
                    if (newBoardScore < oldBoardScore)
                    {
                        particlePaths[n].Actions.Add(neighbor.Item2);
                        var newY = particlePaths[n].Current.Item1 + neighbor.Item2[0];
                        var newX = particlePaths[n].Current.Item2 + neighbor.Item2[1];
                        particlePaths[n].Current = new Tuple<int, int>(newY, newX);
                        particles[n] = neighbor.Item1;
                        lock (bestLock)
                        {
                            if (newBoardScore < bestScore)
                            {
                                bestScore = newBoardScore;
                                best = new Path
                                {
                                    Start = particlePaths[n].Start,
                                    Actions = new List<int[]>(particlePaths[n].Actions),
                                    Current = new Tuple<int, int>(newY, newX)
                                };
                            }
                        }
                        continue;
                    }
                    if (Math.Exp((newBoardScore - oldBoardScore) / temp) < r.NextDouble())
                    {
                        particlePaths[n].Actions.Add(neighbor.Item2);
                        var newY = particlePaths[n].Current.Item1 + neighbor.Item2[0];
                        var newX = particlePaths[n].Current.Item2 + neighbor.Item2[1];
                        particlePaths[n].Current = new Tuple<int, int>(newY, newX);
                        particles[n] = neighbor.Item1;
                        continue;
                    }
                }
            }
            return best;
        }

        //public static Path GetBestPath(Board b, Options o, BoardScorer.Options bso)
        //{
        //    Board targetBoard = new Board(b.Height, b.Width);
        //    Board goalBoard = GetOptimalBoards(b, o, bso).First();
        //    Path path = new Path();
        //    for (int j = 0; j < targetBoard.Width; j++)
        //        for (int i = 0; i < targetBoard.Height; i++)
        //        {
        //            targetBoard.Orbs[i, j] = goalBoard.Orbs[i, j];
        //            path = GetBestPathFrom(b, targetBoard, bso, path);
        //            b = b.GetBoardsAfterPath(path.Start.Item1, path.Start.Item2, path.Actions).Item1;
        //        }
        //    return path;
        //}
        //private static Path GetBestPathFrom(Board b, Board target, BoardScorer.Options bso, Path p)
        //{
        //    HashSet<Board> explored = new HashSet<Board>();
        //    explored.Add(b);
        //    Queue<Tuple<Board, Path>> paths = new Queue<Tuple<Board, Path>>();
        //    paths.Enqueue(new Tuple<Board, Path>(b, p));
        //    while (paths.Any())
        //    {
        //        var cur = paths.Dequeue();
        //        var curPath = cur.Item2;
        //        var curBoard = cur.Item1;

        //        if (curBoard.EqualsBoard(target, ignoreNone: true))
        //            return curPath;

        //        foreach (var direction in Board.MoveDirections)
        //        {
        //            if (curPath.Length != 0 &&
        //                curPath.Actions.Last()[0] == -direction[0] &&
        //                curPath.Actions.Last()[1] == -direction[1])
        //                continue;
        //            var newY = curPath.Current.Item1 + direction[0];
        //            var newX = curPath.Current.Item2 + direction[1];
        //            if (newY < 0 || newY >= b.Height ||
        //                newX < 0 || newX >= b.Width)
        //                continue;
        //            Board newBoard = new Board(curBoard);
        //            Orb tempOrb = newBoard.Orbs[newY, newX];
        //            newBoard.Orbs[newY, newX] = newBoard.Orbs[curPath.Current.Item1, curPath.Current.Item2];
        //            newBoard.Orbs[curPath.Current.Item1, curPath.Current.Item2] = tempOrb;
        //            var newPath = new List<int[]>(curPath.Actions);
        //            newPath.Add(direction);
        //            if (explored.Contains(newBoard))
        //                continue;
        //            explored.Add(newBoard);
        //            paths.Enqueue(new Tuple<Board, Path>(newBoard, new Path
        //            {
        //                Start = curPath.Start,
        //                Current = new Tuple<int, int>(newY, newX),
        //                Depth = curPath.Depth + 1,
        //                Score = 0,
        //                Actions = newPath
        //            }));
        //        }
        //    }
        //    return null;
        //}

        // a-star is silly if all edges are length 1...?
        public static Path GetBestPath(Board b, Options o, BoardScorer.Options bso)
        {
            Board goalBoard = GetOptimalBoards(b, o, bso).First();
            Heuristic h = o.HeuristicGen(goalBoard);
            PriorityQueue<Tuple<Board, Path>, double> queue = new PriorityQueue<Tuple<Board, Path>, double>();
            HashSet<Board> visited = new HashSet<Board>();
            Dictionary<Board, double> toExplore = new Dictionary<Board, double>();
            toExplore.Add(b, 0);
            queue.Enqueue(new Tuple<Board, Path>(b, new Path { Start = new Tuple<int, int>(2, 2) }), -h(b));

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
                    }), -h(newBoard) - curCost - 1);
                    toExplore[newBoard] = curCost + 1;
                }
            }

            return new Path();
        }
    }
}
