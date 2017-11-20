using System;
using System.Linq;
using System.Collections.Generic;

namespace Valerie.Extensions
{
    public static class BoolExt
    {
        public static (bool, string) IsValidUrl(string Url)
        {
            bool UriCheck = Uri.TryCreate(Url, UriKind.Absolute, out Uri Result) &&
                (Result.Scheme == Uri.UriSchemeHttp || Result.Scheme == Uri.UriSchemeHttps);
            return UriCheck == true && Result != null ? (true, Url) : (false, Url);
        }

        public static (bool, string) IsMatch(List<string> List, string Msg, string Warning)
            => List.Any(x => Msg.Contains(x)) ? (true, Warning) : (false, Warning);

    }
}