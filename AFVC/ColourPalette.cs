using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFVC
{
    class ColourPalette
    {
        private static Color[] MFR =
        {
            Color.FromArgb(255, 190, 15),
            Color.FromArgb(213, 254, 119),
            Color.FromArgb(107, 228, 143),
            Color.FromArgb(27, 208, 161),
            Color.FromArgb(0, 201, 167),
            Color.FromArgb(4, 139, 132),
            Color.FromArgb(8, 76, 97)
        };

        private Color background, body, second;
        private Color[] colourRange;
        public static ColourPalette MarineFields = new ColourPalette(MFR,Color.FromArgb(3, 30, 38),Color.Wheat,Color.FromArgb(208,60,27));

        public ColourPalette(Color[] colourRange,Color background, Color body, Color second)
        {
            this.background = background;
            this.body = body;
            this.second = second;
            this.colourRange = colourRange;
        }

        public Color this[int index] => colourRange[index<=0?0:(index>=colourRange.Length?colourRange.Length-1:index)];
        public Color this[ColourPurpose purp] => purp == ColourPurpose.Background
            ? background
            : (purp == ColourPurpose.Body ? body : second);
    }
    enum ColourPurpose { Background,Body,Second}
}
