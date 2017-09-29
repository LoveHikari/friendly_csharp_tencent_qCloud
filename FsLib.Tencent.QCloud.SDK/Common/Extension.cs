using System;

namespace FsLib.Tencent.QCloud.SDK.Common
{
    internal static class Extension
    {
        internal static long ToUnixTime(this DateTime nowTime)
        {
            DateTime startTime = System.TimeZoneInfo.ConvertTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0),TimeZoneInfo.Utc, TimeZoneInfo.Local);
            return (long)Math.Round((nowTime - startTime).TotalMilliseconds, MidpointRounding.AwayFromZero);
        }
    }
}
