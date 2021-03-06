﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNTCaptcha.Core.Internal
{
    internal static class UrlHelper
    {
        internal static string EscapeAll(this string url)
        {
            return url
                .Replace("%", "%25")
                .Replace("+", "%2B")
                .Replace(" ", "%20")
                .Replace("/", "%2F")
                .Replace("?", "%3F")
                .Replace("=", "%3D")
                .Replace("#", "%23")
                .Replace("&", "%26");
        }
    }
}
