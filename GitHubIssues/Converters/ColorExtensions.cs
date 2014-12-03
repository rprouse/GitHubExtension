// **********************************************************************************
// The MIT License (MIT)
// 
// Copyright (c) 2014 Rob Prouse
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// **********************************************************************************

#region Using Directives

using System;
using System.Windows.Media;

#endregion

namespace Alteridem.GitHub.Converters
{
    // An HSB color
    public struct HsbColor
    {
        public double A;
        public double H;
        public double S;
        public double B;
    }

    public static class ColorExtensions
    {
        /// <summary>
        /// Converts an RGB color to an HSB color.
        /// </summary>
        /// <param name="rgbColor">The RGB color to convert.</param>
        /// <returns>The HSB color equivalent of the RGBA color passed in.</returns>
        /// <remarks>Source: http://msdn.microsoft.com/en-us/library/ms771620.aspx</remarks>
        public static HsbColor ToHsb(this Color rgbColor)
        {
            /* Hue values range between 0 and 360. All 
             * other values range between 0 and 1. */

            // Create HSB color object
            var hsbColor = new HsbColor();

            // Get RGB color component values
            var r = (int)rgbColor.R;
            var g = (int)rgbColor.G;
            var b = (int)rgbColor.B;
            var a = (int)rgbColor.A;

            // Get min, max, and delta values
            double min = Math.Min(Math.Min(r, g), b);
            double max = Math.Max(Math.Max(r, g), b);
            double delta = max - min;

            /* Black (max = 0) is a special case. We 
             * simply set HSB values to zero and exit. */

            // Black: Set HSB and return
            if (max == 0.0)
            {
                hsbColor.H = 0.0;
                hsbColor.S = 0.0;
                hsbColor.B = 0.0;
                hsbColor.A = a;
                return hsbColor;
            }

            /* Now we process the normal case. */

            // Set HSB Alpha value
            var alpha = (double)a;
            hsbColor.A = alpha / 255;

            // Set HSB Hue value
            if (r == max) hsbColor.H = (g - b) / delta;
            else if (g == max) hsbColor.H = 2 + (b - r) / delta;
            else if (b == max) hsbColor.H = 4 + (r - g) / delta;
            hsbColor.H *= 60;
            if (hsbColor.H < 0.0) hsbColor.H += 360;

            // Set other HSB values
            hsbColor.S = delta / max;
            hsbColor.B = max / 255;

            // Set return value
            return hsbColor;
        }

        /// <summary>
        /// Converts an HSB color to an RGB color.
        /// </summary>
        /// <param name="hsbColor">The HSB color to convert.</param>
        /// <returns>The RGB color equivalent of the HSB color passed in.</returns>
        /// Source: http://msdn.microsoft.com/en-us/library/ms771620.aspx
        public static Color ToRgb(this HsbColor hsbColor)
        {
            // Initialize
            var rgbColor = new Color();

            /* Gray (zero saturation) is a special case.We simply
             * set RGB values to HSB Brightness value and exit. */

            // Gray: Set RGB and return
            if (hsbColor.S == 0.0)
            {
                rgbColor.A = (byte)(hsbColor.A * 255);
                rgbColor.R = (byte)(hsbColor.B * 255);
                rgbColor.G = (byte)(hsbColor.B * 255);
                rgbColor.B = (byte)(hsbColor.B * 255);
                return rgbColor;
            }

            /* Now we process the normal case. */

            var h = (hsbColor.H == 360) ? 0 : hsbColor.H / 60;
            var i = (int)(Math.Truncate(h));
            var f = h - i;

            var p = hsbColor.B * (1.0 - hsbColor.S);
            var q = hsbColor.B * (1.0 - (hsbColor.S * f));
            var t = hsbColor.B * (1.0 - (hsbColor.S * (1.0 - f)));

            double r, g, b;
            switch (i)
            {
                case 0:
                    r = hsbColor.B;
                    g = t;
                    b = p;
                    break;

                case 1:
                    r = q;
                    g = hsbColor.B;
                    b = p;
                    break;

                case 2:
                    r = p;
                    g = hsbColor.B;
                    b = t;
                    break;

                case 3:
                    r = p;
                    g = q;
                    b = hsbColor.B;
                    break;

                case 4:
                    r = t;
                    g = p;
                    b = hsbColor.B;
                    break;

                default:
                    r = hsbColor.B;
                    g = p;
                    b = q;
                    break;
            }

            // Set WPF Color object
            rgbColor.A = (byte)(hsbColor.A * 255);
            rgbColor.R = (byte)(r * 255);
            rgbColor.G = (byte)(g * 255);
            rgbColor.B = (byte)(b * 255);

            // Set return value
            return rgbColor;
        }

        public static Color ParseColor(this string colorString)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(colorString))
                {
                    object color = ColorConverter.ConvertFromString("#" + colorString);
                    if (color != null)
                        return (Color)color;
                }
            }
            catch (FormatException)
            {
            }
            return Color.FromArgb(0, 0, 0, 0);
        }
        
    }
}