using System;

namespace Valerie.Extensions
{
    public static class BoolExt
    {
        public static (bool, Uri) IsValidUrl(this string Url)
        {
            bool UriCheck = Uri.TryCreate(Url, UriKind.Absolute, out Uri Result) &&
                (Result.Scheme == Uri.UriSchemeHttp || Result.Scheme == Uri.UriSchemeHttps);
            return UriCheck == true && Result != null ? (true, Result) : (false, null);
        }
    }
}