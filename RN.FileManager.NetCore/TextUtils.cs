using System;
using System.Text;
using System.Text.RegularExpressions;

namespace RN.FileManagers.NetCore
{
    public static class TextUtils
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
            return Regex.Replace(Regex.Replace(path, @"\/+", "\\"), @"\\+", "\\");// Regex.Replace(path, @"\+\/+|\\+|\/+\+|\/+", @"\");
        }

        public static string MatchUrl(this string url)
        {
            return Regex.Replace(Regex.Replace(url, @"\\+", "/"), @"\/+", "/").Replace(":/", "://");
        }
    }
}
