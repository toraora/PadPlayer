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
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app)
        {
            app.Run(async (context) =>
            {
                try {
                    Board b = new Board(5, 6);
                    Random r = new Random();
                    Array vals = Enum.GetValues(typeof(Orb));
                    for (int i = 0; i < 5; i++)
                        for (int j = 0; j < 6; j++)
                            b.Orbs[i, j] = (i + j) % 2 == 0 ? Orb.Red : Orb.Blue;
                    var p = DfsSolver.GetBestPath(b);
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(p) + "\n\n\n" + JsonConvert.SerializeObject(b));
                }
                catch (Exception ex)
                {
                    await context.Response.WriteAsync(ex.ToString());
                }
            });
        }
    }
}
