using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Annapurnaworld.common
{
    public static class DateUtils
    {
        public static DateTime ConvertUtcToIst(DateTimeOffset utcDateTime)
        {
            // IST is UTC+5:30
            TimeZoneInfo istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime.UtcDateTime, istZone);
        }
    }
}
