using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using PadLogic.Game;
using PadLogic.Solver;
using Newtonsoft.Json;
using System.Drawing;
using PadLogic.Image;
using PadLogic;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace PadPlayerAPI
{
    [Route("/")]
    public class PadController : Controller
    {
        [Route("solve")]
        public string SolveBoard(string path, int rows, int cols, int width, int height, int w_off, int h_off)
        {
            path = Uri.UnescapeDataString(path);
            Console.WriteLine(path);
            Bitmap bmp = new Bitmap(path);
            Board b = PadImage.BoardFromBitmap(rows, cols, height, width, h_off, w_off, bmp);
            return JsonConvert.SerializeObject(DfsSolver.GetBestPath(b));
        }

        [Route("solve2")]
        public string SolveBoard2(string path, int rows, int cols, int width, int height, int w_off, int h_off)
        {
            path = Uri.UnescapeDataString(path);
            Console.WriteLine(path);
            Bitmap bmp = new Bitmap(path);
            Board b = PadImage.BoardFromBitmap(rows, cols, height, width, h_off, w_off, bmp);
            return JsonConvert.SerializeObject(BeamDfs.GetBestPath(b, BoardScorer.Options.Horus));
        }

        [Route("solveone ")]
        public string SolveBoardOne(string path, int rows, int cols, int width, int height, int w_off, int h_off)
        {
            Board b = new Board(5, 6);
            b.Orbs = new Orb[,]
            {
                { Orb.Green, Orb.Heal, Orb.Light, Orb.Green, Orb.Green, Orb.Red }, 
                { Orb.Dark, Orb.Blue, Orb.Red, Orb.Red, Orb.Blue, Orb.Light },
                { Orb.Heal, Orb.Red, Orb.Heal, Orb.Dark, Orb.Blue, Orb.Blue},
                { Orb.Green, Orb.Green, Orb.Dark, Orb.Light, Orb.Dark, Orb.Dark},
                { Orb.Dark, Orb.Green, Orb.Dark, Orb.Light, Orb.Red, Orb.Green}
            };
            return JsonConvert.SerializeObject(BeamDfs.GetBestPath(b, BoardScorer.Options.Horus));
        }

        [Route("htest")]
        public string TestHeuristic()
        {
            Board b = new Board(6, 6);
            Array vals = Enum.GetValues(typeof(Orb));
            for (int i = 0; i < b.Height; i++)
                for (int j = 0; j < b.Width; j++)
                    b.Orbs[i, j] = Orb.Red;

            Board b2 = new Board(b);
            b.Orbs[0, 0] = Orb.Blue;
            b.Orbs[3, 3] = Orb.Blue;
            b2.Orbs[1, 1] = Orb.Blue;
            b2.Orbs[2, 2] = Orb.Blue;
            b.Orbs[3, 0] = Orb.Green;
            b.Orbs[0, 3] = Orb.Green;
            b2.Orbs[1, 2] = Orb.Green;
            b2.Orbs[2, 1] = Orb.Green;

            SAASSolver.Heuristic h = SAASSolver.Options.Default.HeuristicGen(b);
            return h(b2).ToString();
        }

        [Route("saas")]
        public string TestSAAS(string path, int rows, int cols, int width, int height, int w_off, int h_off)
        {
            path = Uri.UnescapeDataString(path);
            Console.WriteLine(path);
            Bitmap bmp = new Bitmap(path);
            Board b = PadImage.BoardFromBitmap(rows, cols, height, width, h_off, w_off, bmp);
            Board opt = SAASSolver.GetOptimalBoards(b, SAASSolver.Options.Default, BoardScorer.Options.Horus).First();
            var b3 = new Board(opt);
            b3.GetCombos(false);
            //var p2 = SAASSolver.GetBestPathSA(b, SAASSolver.Options.Default, BoardScorer.Options.Horus);
            //var b4 = b.GetBoardsAfterPath(p2.Start.Item1, p2.Start.Item2, p2.Actions);
            return ""
                + b.ToString()
                + "\n\nSAAS optimal:"
                + opt.ToString()
                + b3.ToString()
                + "\n\n"
                + opt.Score(BoardScorer.Options.Horus)
                + "\n\n";
              //  + JsonConvert.SerializeObject(p2)
              //  + b4.Item1.ToString()
              //  + b4.Item2.ToString();
        }

        [Route("boardeq")]
        public bool TestBoardEq()
        {
            Board b = new Board(4, 4);
            Board b2 = new Board(b);
            Array vals = Enum.GetValues(typeof(Orb));
            for (int i = 0; i < b.Height; i++)
                for (int j = 0; j < b.Width; j++)
                {
                    b.Orbs[i, j] = (i + j) % 2 == 0 ? Orb.Red : Orb.Blue;
                    b.Orbs[i, j] = (Orb)vals.GetValue(1 + (i + j) % 5);
                    if (i == 0)
                        b2.Orbs[i, j] = b.Orbs[i, j];
                }
            return b2.EqualsBoard(b, false);
        }
    }
}
