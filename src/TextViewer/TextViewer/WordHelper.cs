﻿using System.Linq;
using System.Text.RegularExpressions;

namespace TextViewer
{
    public static class WordHelper
    {
        public const string InertChars = "\\|«»<>[]{}()'/،.,:!@#$%٪^&~*_-+=~‍‍‍‍\"`×?";

        // All "AL" or "R" of Unicode 6.0 (from http://www.unicode.org/Public/6.0.0/ucd/UnicodeData.txt)
        public static bool IsRtl(this char c)
        {
            if (c >= 0x5BE && c <= 0x10B7F)
            {
                if (c <= 0x85E)
                {
                    if (c == 0x5BE) return true;
                    if (c == 0x5C0) return true;
                    if (c == 0x5C3) return true;
                    if (c == 0x5C6) return true;
                    if (0x5D0 <= c && c <= 0x5EA) return true;
                    if (0x5F0 <= c && c <= 0x5F4) return true;
                    if (c == 0x608) return true;
                    if (c == 0x60B) return true;
                    if (c == 0x60D) return true;
                    if (c == 0x61B) return true;
                    if (0x61E <= c && c <= 0x64A) return true;
                    if (0x66D <= c && c <= 0x66F) return true;
                    if (0x671 <= c && c <= 0x6D5) return true;
                    if (0x6E5 <= c && c <= 0x6E6) return true;
                    if (0x6EE <= c && c <= 0x6EF) return true;
                    if (0x6FA <= c && c <= 0x70D) return true;
                    if (c == 0x710) return true;
                    if (0x712 <= c && c <= 0x72F) return true;
                    if (0x74D <= c && c <= 0x7A5) return true;
                    if (c == 0x7B1) return true;
                    if (0x7C0 <= c && c <= 0x7EA) return true;
                    if (0x7F4 <= c && c <= 0x7F5) return true;
                    if (c == 0x7FA) return true;
                    if (0x800 <= c && c <= 0x815) return true;
                    if (c == 0x81A) return true;
                    if (c == 0x824) return true;
                    if (c == 0x828) return true;
                    if (0x830 <= c && c <= 0x83E) return true;
                    if (0x840 <= c && c <= 0x858) return true;
                    if (c == 0x85E) return true;
                }
                else if (c == 0x200F) return true;
                else if (c >= 0xFB1D)
                {
                    if (c == 0xFB1D) return true;
                    if (0xFB1F <= c && c <= 0xFB28) return true;
                    if (0xFB2A <= c && c <= 0xFB36) return true;
                    if (0xFB38 <= c && c <= 0xFB3C) return true;
                    if (c == 0xFB3E) return true;
                    if (0xFB40 <= c && c <= 0xFB41) return true;
                    if (0xFB43 <= c && c <= 0xFB44) return true;
                    if (0xFB46 <= c && c <= 0xFBC1) return true;
                    if (0xFBD3 <= c && c <= 0xFD3D) return true;
                    if (0xFD50 <= c && c <= 0xFD8F) return true;
                    if (0xFD92 <= c && c <= 0xFDC7) return true;
                    if (0xFDF0 <= c && c <= 0xFDFC) return true;
                    if (0xFE70 <= c && c <= 0xFE74) return true;
                    if (0xFE76 <= c && c <= 0xFEFC) return true;
                    if (0x10800 <= c && c <= 0x10805) return true;
                    if (c == 0x10808) return true;
                    if (0x1080A <= c && c <= 0x10835) return true;
                    if (0x10837 <= c && c <= 0x10838) return true;
                    if (c == 0x1083C) return true;
                    if (0x1083F <= c && c <= 0x10855) return true;
                    if (0x10857 <= c && c <= 0x1085F) return true;
                    if (0x10900 <= c && c <= 0x1091B) return true;
                    if (0x10920 <= c && c <= 0x10939) return true;
                    if (c == 0x1093F) return true;
                    if (c == 0x10A00) return true;
                    if (0x10A10 <= c && c <= 0x10A13) return true;
                    if (0x10A15 <= c && c <= 0x10A17) return true;
                    if (0x10A19 <= c && c <= 0x10A33) return true;
                    if (0x10A40 <= c && c <= 0x10A47) return true;
                    if (0x10A50 <= c && c <= 0x10A58) return true;
                    if (0x10A60 <= c && c <= 0x10A7F) return true;
                    if (0x10B00 <= c && c <= 0x10B35) return true;
                    if (0x10B40 <= c && c <= 0x10B55) return true;
                    if (0x10B58 <= c && c <= 0x10B72) return true;
                    if (0x10B78 <= c && c <= 0x10B7F) return true;
                }
            }
            else if (!char.IsDigit(c) &&
                     Regex.IsMatch(c.ToString(), @"\p{IsArabic}|\p{IsHebrew}")) return true;

            return false;
        }

        /// <summary>
        /// To check whether the given string is Arabic.
        /// </summary>
        /// <param name="input"></param>
        /// <returns>Returns True if Arabic</returns>
        public static bool IsRtl(this string input)
        {
            return input.Any(c => c.IsRtl());
        }
    }
}