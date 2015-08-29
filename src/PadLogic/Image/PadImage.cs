using PadLogic.Game;
using PadLogic.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace PadLogic.Image
{
    public class PadImage
    {
        public static Board BoardFromBitmap(int rows, int columns, int height, int width, int h_off, int w_off, Bitmap bmp)
        {
            PadImage pi = new PadImage(bmp, rows, columns, height, width, h_off, w_off);
            Board b = new Board(rows, columns);
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < columns; j++)
                {
                    b.Orbs[i, j] = pi.GetOrb(i, j);
                }
            Console.WriteLine(b.ToString());
            return b;
        }

        private Bitmap m_bmp;
        private int m_rows;
        private int m_columns;
        private int m_height;
        private int m_width;
        private int m_h_off;
        private int m_w_off;
        private PadImage(Bitmap bmp, int rows, int columns, int height, int width, int h_off, int w_off)
        {
            m_bmp = bmp;
            m_rows = rows;
            m_columns = columns;
            m_height = height;
            m_width = width;
            m_h_off = h_off;
            m_w_off = w_off;
        }

        private Orb GetOrb(int row, int col)
        {
            int y_center = m_height * row / m_rows + m_h_off;
            int x_center = m_width * col / m_columns + m_w_off;
            int r = 0;
            int g = 0;
            int b = 0;
            for (int i = -5; i <= 5; i++)
                for (int j = -5; j <= 5; j++)
                {
                    Color c = m_bmp.GetPixel(x_center + j, y_center + i);
                    r += c.R;
                    g += c.G;
                    b += c.B;
                }
            Color cur = Color.FromArgb(r / 121, g / 121, b / 121);
            Orb ret = OrbsToColors.MinBy(s => ColorDiff(cur, s.Value)).Key;
            return ret;
        }

        private static Dictionary<Orb, Color> OrbsToColors = new Dictionary<Orb, Color> {
            { Orb.Red, Color.FromArgb(254, 119, 77) },
            { Orb.Blue, Color.FromArgb(64, 174, 238) },
            { Orb.Green, Color.FromArgb(85, 238, 102) },
            { Orb.Light, Color.FromArgb(255, 251, 132) },
            { Orb.Dark, Color.FromArgb(170, 89, 178) },
            { Orb.Heal, Color.FromArgb(220, 34, 127) }
        };

        private static double ColorDiff(Color a, Color b)
        {
            int dr = Math.Abs(a.R - b.R);
            int dg = Math.Abs(a.G - b.G);
            int db = Math.Abs(a.B - b.B);
            return dr + dg + db;
        }
    }
}
