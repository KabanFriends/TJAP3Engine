using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TJAPlayer3
{
    class CDVDLogo
    {
        public int x = 0;
        public int y = 0;
        private int hSpeed;
        private int vSpeed;

        private int width;
        private int height;

        public CDVDLogo()
        {
            x = 0;
            y = 0;
            hSpeed = 5;
            vSpeed = 4;

            width = TJAPlayer3.Tx.DVD_Logo.sz画像サイズ.Width;
            height = TJAPlayer3.Tx.DVD_Logo.sz画像サイズ.Height;
        }

        public void tStep()
        {
            x += hSpeed;
            y += vSpeed;

            stepHits = 0;

            if (x + width > 1280)
            {
                hSpeed = -Math.Abs(hSpeed);
                x = 1280 - width;
                this.tHit();
            }
            if (y + height > 720)
            {
                vSpeed = -Math.Abs(vSpeed);
                y = 720 - height;
                this.tHit();
            }

            if (x < 0)
            {
                hSpeed = Math.Abs(hSpeed);
                x = 0;
                this.tHit();
            }
            if (y < 0)
            {
                vSpeed = Math.Abs(vSpeed);
                y = 0;
                this.tHit();
            }
        }

        private void tHit()
        {
            CStageサンプル.hits++;
            stepHits++;

            //Sadly TJAPlayer3 texture hates color changing :(
            /*
            Color color = CDVDLogo.ColorFromHSV(rnd.Next(1, 256), 1, 1);
            Console.WriteLine(color);
            TJAPlayer3.Tx.DVD_Logo.color4.Red = color.R;
            TJAPlayer3.Tx.DVD_Logo.color4.Green = color.G;
            TJAPlayer3.Tx.DVD_Logo.color4.Blue = color.B;
            Console.WriteLine(TJAPlayer3.Tx.DVD_Logo.color4);
            */

            if (stepHits >= 2)
            {
                CStageサンプル.cornerHits++;
            }
        }

        private static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }

        Random rnd = new Random();

        private int stepHits = 0;
    }
}
