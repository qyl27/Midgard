using System;

namespace Midgard.Utilities
{
    public class Time
    {
        public static long GetTimeStamp(DateTime time)
        {
            TimeSpan ts = time.ToUniversalTime() - new DateTime(1970, 1, 1);
            return (long)ts.TotalSeconds;
        }
    }
}