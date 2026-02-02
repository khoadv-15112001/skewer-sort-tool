using System;
using System.Globalization;

namespace Helper
{
    public static class TimeHelper
    {
        public static DateTime GetUtcTimeFromISO(string s)
        {
            DateTimeOffset dto = DateTimeOffset.Parse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
            return dto.UtcDateTime;
        }

        public static long GetUnixTimeFromISO(string s)
        {
            DateTimeOffset dto = DateTimeOffset.Parse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
            return dto.ToUnixTimeSeconds();
        }

        public static DateTime GetLocalTimeFromISO(string s)
        {
            DateTimeOffset dto = DateTimeOffset.Parse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
            return dto.LocalDateTime;
        }

        public static long ToUnixTimeSeconds(this DateTime dateTime)
        {
            return new DateTimeOffset(dateTime).ToUnixTimeSeconds();
        }

        public static DateTime ToLocalDateTime(this long timeUnix)
        {
            return DateTimeOffset.FromUnixTimeSeconds(timeUnix).LocalDateTime;
        }

        /// <summary>
        /// Convert Unix timestamp to ISO 8601 string format (local time)
        /// </summary>
        /// <param name="unixTime">Unix timestamp</param>
        /// <returns>ISO 8601 formatted string in local time</returns>
        public static string ConvertUnixToISO(long unixTime)
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTime);
            return dateTimeOffset.ToLocalTime().ToString("o");
        }

        /// <summary>
        /// Convert Unix timestamp to ISO 8601 string format (UTC time)
        /// </summary>
        /// <param name="unixTime">Unix timestamp</param>
        /// <returns>ISO 8601 formatted string in UTC</returns>
        public static string ConvertUnixToISOUtc(long unixTime)
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTime);
            return dateTimeOffset.UtcDateTime.ToString("o");
        }
    }
}
