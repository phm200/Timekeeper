using System;

namespace Phm.Time
{
    public class TimekeeperClock
    {
        private static TimeZoneInfo _timeZone = TimeZoneInfo.Utc;
        /// <summary>
        /// Returns the current time for the time zone (default UTC)
        /// </summary>
        public static Func<DateTime> Now = () => TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.Utc, _timeZone);

        /// <summary>
        /// Overrides the time zone to use for Now()
        /// </summary>
        public static TimeZoneInfo TimeZone
        {
            get { return _timeZone; }
            set { _timeZone = value; }
        }
    }
}