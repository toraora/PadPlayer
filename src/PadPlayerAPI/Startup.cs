using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Framework.DependencyInjection;
using PadLogic.Game;
using PadLogic.Solver;
using Newtonsoft.Json;

namespace PadPlayerAPI
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {

        }

        public void Configure(IApplicationBuilder app)
        {
            app.Run(async (context) =>
            {
                try {
                    Board b = new Board(5, 6);
                    Array vals = Enum.GetValues(typeof(Orb));
                    for (int i = 0; i < 5; i++)
                        for (int j = 0; j < 6; j++)
                        {
                            b.Orbs[i, j] = (i + j) % 2 == 0 ? Orb.Red : Orb.Blue;
                            b.Orbs[i, j] = (Orb) vals.GetValue(1 + (i + j) % 5);
                        }
                    var p = DfsSolver.GetBestPath(b);
                    var b2 = b.GetBoardsAfterPath(p.Start.Item1, p.Start.Item2, p.Actions);
                    Board opt = SAASSolver.GetOptimalBoards(b, SAASSolver.Options.Default, BoardScorer.Options.Horus).First();
                    var b3 = new Board(opt);
                    b3.GetCombos(false);
                    var p2 = SAASSolver.GetBestPath(b, SAASSolver.Options.Default, BoardScorer.Options.Horus);
                    //await context.Response.WriteAsync(JsonConvert.SerializeObject(p));
                    //return;
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(p)
                        + "\n\n\n"
                        + JsonConvert.SerializeObject(b)
                        + "\n\n\n"
                        + JsonConvert.SerializeObject(b2)
                        + b.ToString()
                        + b2.Item1.ToString()
                        + b2.Item2.ToString()
                        + "\n\n"
                        + b2.Item1.Score(BoardScorer.Options.Horus)
                        + "\n\n"
                        + opt.ToString()
                        + b3.ToString()
                        + "\n\n"
                        + opt.Score(BoardScorer.Options.Horus)
                        + "\n\n"
                        + JsonConvert.SerializeObject(p2)
                    );
                }
                catch (Exception ex)
                {
                    await context.Response.WriteAsync(ex.ToString());
                }
            });
        }
    }
}
