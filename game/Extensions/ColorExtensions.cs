using System.Globalization;
using Microsoft.Xna.Framework;

namespace game
{
    public static class ColorExtensions
    {
        public static Color FromRgb(string hex)
        {
            int index = hex.IndexOf('#');
            hex = hex.Remove(index, 1);
            
            uint argb = uint.Parse(hex, NumberStyles.HexNumber);
            var wColor = System.Drawing.Color.FromArgb((int) (argb >> 24), 
                (int) ((argb >> 16) & 0xFF),
                (int) ((argb >> 8) & 0xFF),
                (int) (argb & 0xFF));
            
            return new Color(wColor.R, wColor.G, wColor.B, wColor.A);
        }
    }
}