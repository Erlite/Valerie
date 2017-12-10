using System;

namespace Valerie.Extensions
{
    public class DateExt
    {
        public static DateTime UnixDT(double Unix) => new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Unix).ToLocalTime();
    }
}