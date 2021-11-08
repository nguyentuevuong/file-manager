using System;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;

namespace RN.FileManagers.Core
{
    internal static class Utils
    {
        public static string StripDiacritics(this string accented)
        {
            if (String.IsNullOrEmpty(accented))
                return String.Empty;

            var regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string strFormD = accented.Normalize(NormalizationForm.FormD);
            return regex.Replace(strFormD, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }

        public static string MatchPath(this string path)
        {
            return Regex.Replace(Regex.Replace(path, @"\/+", "\\"), @"\\+", "\\");
        }

        public static string MatchUrl(this string url)
        {
            return Regex.Replace(Regex.Replace(url, @"\\+", "/"), @"\/+", "/").Replace(":/", "://");
        }

        public static Image Resize(this Image image, float? w, float? h)
        {
            if (!w.HasValue && !h.HasValue)
            {
                return image;
            }

            float pl = 0, pt = 0, rw = 0, rh = 0, rt1 = image.Width / image.Height;

            rw = (w = w ?? (h * rt1)).Value;
            rh = (h = h ?? (w / rt1)).Value;

            var rt2 = w / h;
            if (rt1 < rt2)
            {
                rw = h.Value * rt1;
                pl = (w.Value - rw) / 2;
            }
            else if (rt1 > rt2)
            {
                rh = w.Value / rt1;
                pt = (h.Value - rh) / 2;
            }


            var newImage = new Bitmap((int)w.Value, (int)h.Value);

            using (var graphic = Graphics.FromImage(newImage))
                graphic.DrawImage(image, pl, pt, w.Value, h.Value);

            return newImage;
        }
    }
}
