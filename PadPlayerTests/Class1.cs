using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PadPlayerTests
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class Class1
    {

        [Fact]
        public void UniTest1()
        {
            Board b = new Board(5, 6);
            Random r = new Random();
            Array vals = Enum.GetValues(typeof(Orb));
            for (int i = 0; i < 5; i++)
                for (int j = 0; j < 6; j++)
                    b.Orbs[i, j] = (Orb)vals.GetValue(r.Next(vals.Length));
            DfsSolver.GetBestPath(b);
        }
    }
}
